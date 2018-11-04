//-----------------------------------------------------------------------

// <copyright file="Parser.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Monday, November 5, 2018 12:22:54 AM</date>
// </copyright>

//-----------------------------------------------------------------------


using CWF.Core.ExecutionGraph;
using CWF.Core.ExecutionGraph.Flowchart;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;

namespace CWF.Core
{
    public class Parser
    {
        /// <summary>
        /// To initialize a activity.
        /// </summary>
        public class ActivityMemento
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public string Description { get; set; }

            public bool IsEnabled { get; set; }

            public Workflow Workflow {get;set; }

            public Setting[] Settings { get; set; }

            public Activity.GuardType Guard { get; set; }

        }

        /// <summary>
        /// To initialize a workflow
        /// </summary>
        public class WorkflowMemento
        {
            /// <summary>
            /// Workflow Id.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Workflow name.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Workflow description.
            /// </summary>
            public string Description { get; set; }

            /// <summary>
            /// Workflow lanch type.
            /// </summary>
            public LaunchType LaunchType { get; set; }

            /// <summary>
            /// Workflow period.
            /// </summary>
            public TimeSpan Period { get; set; }

            /// <summary>
            /// Shows whether this workflow is enabled or not.
            /// </summary>
            public bool IsEnabled { get; set; }

            /// <summary>
            /// State Token
            /// </summary>
            public object StateToken { get; set; }

            /// <summary>
            /// Workflow file path.
            /// </summary>
            public string WorkflowFilePath { get; set; }

            /// <summary>
            /// Xml Namespace Manager.
            /// </summary>
            public XmlNamespaceManager XmlNamespaceManager { get; set; }
        }

        WorkflowMemento _workflowMemento;
       
        static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        CyPAN.RuntimeAssemblyBuilder.ConditionCollectionFactory _conditionCollectionFactory;

        /// <summary>
        /// 
        /// </summary>
        public Parser()
        {
            _conditionCheckMethodNames = new List<string>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xe"></param>
        /// <param name="wf"></param>
        /// <returns></returns>
        protected ActivityMemento ParseActivity(XElement xe, Workflow wf)
        {
            ActivityMemento activityMemento = new ActivityMemento();

            XElement _xElement;
            _xElement = xe;
            var xId = xe.Attribute("id");
            if (xId == null) throw new Exception("Task id attribute not found.");
            activityMemento.Id = int.Parse(xId.Value);
            var xName = xe.Attribute("name");
            if (xName == null) throw new Exception("Task name attribute not found.");
            activityMemento.Name = xName.Value;
            var xDesc = xe.Attribute("description");
            if (xDesc == null) throw new Exception("Task description attribute not found.");
            activityMemento.Description = xDesc.Value;
            var xEnabled = xe.Attribute("enabled");
            if (xEnabled == null) throw new Exception("Task enabled attribute not found.");
            activityMemento.IsEnabled = bool.Parse(xEnabled.Value);
            var xGuardType = xe.Attribute("guard");
            if (xGuardType?.Value?.CompareTo("and") == 0)
                activityMemento.Guard = Activity.GuardType.And;
            else
                activityMemento.Guard = Activity.GuardType.Or;

            activityMemento.Workflow = wf;

            // settings
            IList<Setting> settings = new List<Setting>();

            foreach (var xSetting in xe.XPathSelectElements("wf:Setting", activityMemento.Workflow.XmlNamespaceManager))
            {
                // setting name
                var xSettingName = xSetting.Attribute("name");
                if (xSettingName == null) throw new Exception("Setting name not found");
                string settingName = xSettingName.Value;

                // setting value
                var xSettingValue = xSetting.Attribute("value");
                string settingValue = string.Empty;
                if (xSettingValue != null) settingValue = xSettingValue.Value;

                // setting attributes
                IList<Attribute> attributes = new List<Attribute>();

                foreach (var xAttribute in xSetting.Attributes().Where(attr => attr.Name.LocalName != "name" && attr.Name.LocalName != "value"))
                {
                    Attribute attr = new Attribute(xAttribute.Name.LocalName, xAttribute.Value);
                    attributes.Add(attr);
                }

                Setting setting = new Setting(settingName, settingValue, attributes.ToArray());
                settings.Add(setting);
            }

            activityMemento.Settings = settings.ToArray();
            return activityMemento;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="engine"></param>
        /// <param name="WorkflowFilePath"></param>
        /// <param name="XsdPath"></param>
        /// <returns></returns>
        public Workflow CheckAndLoadWorkflow(CWFEngine engine, string WorkflowFilePath, string XsdPath)
        {
            _workflowMemento = new WorkflowMemento();

            _workflowMemento.WorkflowFilePath = WorkflowFilePath;
            _XsdPath = XsdPath;

            Check();
            var wf = Load(engine);
           
            foreach (var activity in wf.Activities)
            {
                activity.Finished += wf.Activity_Finished;
            }
            return wf;
        }

        /// <summary>
        /// 
        /// </summary>
        void Check()
        {
            var schemas = new XmlSchemaSet();
            schemas.Add("urn:cwf-schema", _XsdPath);

            var doc = XDocument.Load(_workflowMemento.WorkflowFilePath);
            string msg = string.Empty;
            doc.Validate(schemas, (o, e) =>
            {
                msg += e.Message + Environment.NewLine;
            });

            if (!string.IsNullOrEmpty(msg))
            {
                throw new Exception("The workflow XML document is not valid. Error: " + msg);
            }
        }
        private const string ConditionCheckMethodNamePrefix = "CheckCondition_";
        private const string ConditionCheckClassName = "TransitionConditions";
        private const string ConditionCheckNamespaceSuffix = ".Conditions";
        private List<string> _conditionCheckMethodNames;
        private Type _stateType;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tDef"></param>
        private void OnTransitionDefinitionInXml(Transition tDef)
        {
            if (_conditionCheckMethodNames == null) _conditionCheckMethodNames = new List<string>();
            if (string.IsNullOrEmpty(tDef.ConditionText)) return;
            _conditionCollectionFactory.AddConditionMethod(ConditionCheckMethodNamePrefix + tDef.Id, tDef.ConditionText);
            _conditionCheckMethodNames.Add(ConditionCheckMethodNamePrefix + tDef.Id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transitions"></param>
        /// <param name="conditionMethods"></param>
        private void LinkTransitionsToCompiledConditions(IEnumerable<Transition> transitions, Dictionary<string, MethodInfo> conditionMethods)
        {
            foreach (var t in transitions)
            {
                if (conditionMethods.ContainsKey(ConditionCheckMethodNamePrefix + t.Id))
                    t.SetMethodInfo(_stateType, conditionMethods[ConditionCheckMethodNamePrefix + t.Id]);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xdoc"></param>
        private void InitializeTransitionDefinitionBuilder(XDocument xdoc)
        {
            _conditionCollectionFactory = CyPAN.RuntimeAssemblyBuilder.ClassFactory.CreateWorkflowConditionCollectionClassDefinition(_workflowMemento.Name + ConditionCheckNamespaceSuffix, ConditionCheckClassName, _stateType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, MethodInfo> CompileAndGetMethodInfos()
        {
            var x = _conditionCollectionFactory.Finish();
            var (bytes, compilerOutput) = x.CompileToAssembly();
            if (bytes == null)
            {
                if (compilerOutput == null)
                {
                    logger.Fatal($"An unknown error occurred during compilation of the transition definitions. No compiler output was created.");
                }
                else
                {
                    if (compilerOutput.Success)
                    {
                        logger.Error($"Compiler returned success but no assembly was created");
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (var diag in compilerOutput.Diagnostics)
                        {
                            sb.AppendLine(diag.ToString());
                        }
                        logger.Error($"Compilation failed. diagnostic lines as follows:\n{sb.ToString()}");
                    }
                }
                return null;
            }
            var ass = Assembly.Load(bytes);
            var fullClassName = _workflowMemento.Name + ConditionCheckNamespaceSuffix + "." + ConditionCheckClassName;
            var t = ass.GetType(fullClassName);
            var d = new Dictionary<string, MethodInfo>();
            foreach (var methodName in _conditionCheckMethodNames)
            {
                var mi = t.GetMethod(methodName);
                if (mi != null)
                    d.Add(methodName, mi);
                else
                {
                    logger.Error($"{methodName} was among the known condition methods but there was no compiled version of that method.");
                }
            }
            return d;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="engine"></param>
        /// <returns></returns>
        Workflow Load(CWFEngine engine)
        {           
            using (var xmlReader = XmlReader.Create(_workflowMemento.WorkflowFilePath))
            {
                var xmlNameTable = xmlReader.NameTable;
                if (xmlNameTable != null)
                {
                    _workflowMemento.XmlNamespaceManager = new XmlNamespaceManager(xmlNameTable);
                    _workflowMemento.XmlNamespaceManager.AddNamespace("wf", "urn:cwf-schema");
                }
                else
                {
                    throw new Exception("xmlNameTable of " + _workflowMemento.WorkflowFilePath + " is null");
                }

                // Loading settings
                var xdoc = XDocument.Load(_workflowMemento.WorkflowFilePath);
                XDocument XDoc = xdoc;
                XNamespaceWf = "urn:cwf-schema";

                _workflowMemento.Id = int.Parse(GetWorkflowAttribute(xdoc, "id"));
                _workflowMemento.Name = GetWorkflowAttribute(xdoc, "name");
                _workflowMemento.Description = GetWorkflowAttribute(xdoc, "description");

                _workflowMemento.LaunchType = (LaunchType)Enum.Parse(typeof(LaunchType), GetWorkflowSetting(xdoc, "launchType"), true);
                if (_workflowMemento.LaunchType == LaunchType.Periodic) _workflowMemento.Period = TimeSpan.Parse(GetWorkflowSetting(xdoc, "period"));
                _workflowMemento.IsEnabled = bool.Parse(GetWorkflowSetting(xdoc, "enabled"));

                //create state token               
                try
                {
                     var typename = GetWorkflowSetting(xdoc, "statemachinetype");
                    _workflowMemento.StateToken = CreateStateToken(typename);
                    InitializeTransitionDefinitionBuilder(xdoc);                   

                    Logger.Info($"Instantiated State token");
                }
                catch (FileNotFoundException fnfe)
                {
                    Logger.Error($"A required library was not found: {fnfe.FileName}");
                }
                catch (Exception e)
                {
                    Logger.Error($"Exception on creating state machine token: {e.ToString()}");
                    return null;
                }

                if (xdoc.Root != null)
                {
                    var xExecutionGraph = xdoc.Root.Element(XNamespaceWf + Sym.ACTIVITY_SETUP);
                    IsExecutionGraphEmpty = xExecutionGraph == null || !xExecutionGraph.Elements().Any();
                }
                var wf = new Workflow(engine, _workflowMemento);
                // Loading tasks
                var activities = new List<Activity>();
                foreach (var xTaskElement in xdoc.XPathSelectElements("/wf:Workflow/wf:Activities/wf:Activity", _workflowMemento.XmlNamespaceManager))
                {
                    var xAttribute = xTaskElement.Attribute("name");
                    if (xAttribute != null)
                    {
                        var name = xAttribute.Value;

                        ActivityMemento activityMemento = ParseActivity(xTaskElement, wf);
                        Activity activity = LoadActivityInstance(name, activityMemento);

                        activities.Add(activity);
                    }
                    else
                    {
                        throw new Exception("Name attribute of the task " + xTaskElement + " does not exist.");
                    }
                }

                wf.Activities = activities.ToArray();

                //read all transition definitions
                var transitionList = new List<Transition>();
                foreach (var xTaskElement in xdoc.XPathSelectElements("/wf:Workflow/wf:Transitions/wf:Transition", _workflowMemento.XmlNamespaceManager))
                {
                    var xAttributeId = xTaskElement.Attribute("id");
                    var xAttributeFrom = xTaskElement.Attribute("from");
                    var xAttributeTo = xTaskElement.Attribute("to");
                    var xAttributeCondition = xTaskElement.Attribute("condition");

                    int id = int.Parse(xAttributeId.Value);
                    int from = int.Parse(xAttributeFrom.Value);
                    int to = int.Parse(xAttributeTo.Value);
                    string condition = xAttributeCondition?.Value;
                    var genericType = typeof(Transition<>).MakeGenericType(_stateType);
                    var transition = (Transition)Activator.CreateInstance(genericType,new object[] { id,from,to,condition});
                    OnTransitionDefinitionInXml(transition);
                    transitionList.Add(transition);
                    Logger.Info(transition.ToString());
                }
                wf.Transitions = transitionList.ToArray();
                var d = CompileAndGetMethodInfos();
                LinkTransitionsToCompiledConditions(wf.Transitions, d);

                // Loading execution graph
                var xExectionGraph = xdoc.XPathSelectElement("/wf:Workflow/wf:ActivitySetup", _workflowMemento.XmlNamespaceManager);
                if (xExectionGraph != null)
                {
                    var taskNodes = GetTaskNodes(xExectionGraph);

                    // Check startup node, parallel tasks and infinite loops
                    if (taskNodes.Any())
                    {
                        if (CheckStartupNode(taskNodes) == false)
                        {
                            Logger.Trace("Startup node with parentId=-1 not found in ExecutionGraph execution graph.");
                            throw new Exception();
                        }
                    }

                    if (CheckParallelTasks(taskNodes))
                    {
                        Logger.Trace("Parallel tasks execution detected in ExecutionGraph execution graph.");
                    }

                    CheckInfiniteLoop(taskNodes, "Infinite loop detected in ExecutionGraph execution graph.");

                    // OnSuccess
                    GraphEvent onSuccess = null;
                    var xOnSuccess = xExectionGraph.XPathSelectElement("wf:OnSuccess", _workflowMemento.XmlNamespaceManager);
                    if (xOnSuccess != null)
                    {
                        var onSuccessNodes = GetTaskNodes(xOnSuccess);

                        if (CheckStartupNode(onSuccessNodes) == false)
                        {
                            Logger.Trace("Startup node with parentId=-1 not found in OnSuccess execution graph.");
                            throw new Exception();
                        }

                        if (CheckParallelTasks(onSuccessNodes))
                        {
                            Logger.Trace("Parallel tasks execution detected in OnSuccess execution graph.");
                        }

                        CheckInfiniteLoop(onSuccessNodes, "Infinite loop detected in OnSuccess execution graph.");
                        onSuccess = new GraphEvent(onSuccessNodes);
                    }

                    // OnWarning
                    GraphEvent onWarning = null;
                    var xOnWarning = xExectionGraph.XPathSelectElement("wf:OnWarning", _workflowMemento.XmlNamespaceManager);
                    if (xOnWarning != null)
                    {
                        var onWarningNodes = GetTaskNodes(xOnWarning);

                        if (CheckStartupNode(onWarningNodes) == false)
                        {
                            Logger.Trace("Startup node with parentId=-1 not found in OnWarning execution graph.");
                            throw new Exception();
                        }

                        if (CheckParallelTasks(onWarningNodes))
                        {
                            Logger.Trace("Parallel tasks execution detected in OnSuccess execution graph.");
                        }

                        CheckInfiniteLoop(onWarningNodes, "Infinite loop detected in OnWarning execution graph.");
                        onWarning = new GraphEvent(onWarningNodes);
                    }

                    // OnError
                    GraphEvent onError = null;
                    var xOnError = xExectionGraph.XPathSelectElement("wf:OnError", _workflowMemento.XmlNamespaceManager);
                    if (xOnError != null)
                    {
                        var onErrorNodes = GetTaskNodes(xOnError);

                        if (CheckStartupNode(onErrorNodes) == false)
                        {
                            Logger.Trace("Startup node with parentId=-1 not found in OnError execution graph.");
                            return null;
                        }

                        if (CheckParallelTasks(onErrorNodes))
                        {
                            Logger.Trace("Parallel tasks execution detected in OnError execution graph.");
                        }

                        CheckInfiniteLoop(onErrorNodes, "Infinite loop detected in OnError execution graph.");
                        onError = new GraphEvent(onErrorNodes);
                    }

                    wf.ActivitySetup = new Graph(taskNodes, onSuccess, onWarning, onError);                   
                }
                return wf;
            }
        }
        
        /// <summary>
        /// Default parent node id to start with in the execution graph.
        /// </summary>
        public const int StartId = -1;
        /// <summary>
        /// Shows whether the execution graph is empty or not.
        /// </summary>
        protected bool IsExecutionGraphEmpty { get; private set; }
        /// <summary>
        /// XNamespace.
        /// </summary>
        protected XNamespace XNamespaceWf { get; private set; }
        /// <summary>
        /// XSD path.
        /// </summary>
        protected string _XsdPath { get; private set; }        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskNodes"></param>
        /// <param name="errorMsg"></param>
        void CheckInfiniteLoop(Node[] taskNodes, string errorMsg)
        {
            var startNode = taskNodes.FirstOrDefault(n => n.ParentId == StartId);

            if (startNode != null)
            {
                var infiniteLoopDetected = CheckInfiniteLoop(startNode, taskNodes);

                if (!infiniteLoopDetected)
                {
                    foreach (var taskNode in taskNodes.Where(n => n.Id != startNode.Id))
                    {
                        infiniteLoopDetected |= CheckInfiniteLoop(taskNode, taskNodes);

                        if (infiniteLoopDetected) break;
                    }
                }

                if (infiniteLoopDetected)
                {
                    throw new Exception(errorMsg);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startNode"></param>
        /// <param name="taskNodes"></param>
        /// <returns></returns>
        bool CheckInfiniteLoop(Node startNode, Node[] taskNodes)
        {
            foreach (var taskNode in taskNodes.Where(n => n.ParentId != startNode.ParentId))
            {
                if (taskNode.Id == startNode.Id)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xExectionGraph"></param>
        /// <returns></returns>
        Node[] GetTaskNodes(XElement xExectionGraph)
        {
            var elements = xExectionGraph.Elements();
            var onlyActual = elements.Where(xe => xe.Name.LocalName != "OnSuccess" && xe.Name.LocalName != "OnWarning" && xe.Name.LocalName != "OnError");
            var toNodes = onlyActual.Select(XNodeToNode);
            return toNodes.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodes"></param>
        /// <returns></returns>
        bool CheckStartupNode(Node[] nodes)
        {
            if (nodes == null) return false;
            if (nodes.All(n => n.ParentId != StartId)) return false;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xNode"></param>
        /// <returns></returns>
        Node XNodeToNode(XElement xNode)
        {
            switch (xNode.Name.LocalName)
            {
                case Sym.ACTIVITY:
                    var xId = xNode.Attribute("id");
                    if (xId == null) throw new Exception("Task id not found.");
                    var id = int.Parse(xId.Value);

                    var xParentId = xNode.XPathSelectElement("wf:Parent", _workflowMemento.XmlNamespaceManager)
                        .Attribute("id");

                    if (xParentId == null) throw new Exception("Parent id not found.");
                    var parentId = int.Parse(xParentId.Value);

                    var node = new Node(id, parentId);
                    return node;
                case Sym.IF:
                    return XIfToIf(xNode);
                case Sym.WHILE:
                    return XWhileToWhile(xNode);
                case Sym.SWITCH:
                    return XSwitchToSwitch(xNode);
                default:
                    throw new Exception(xNode.Name.LocalName + " is not supported.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xSwitch"></param>
        /// <returns></returns>
        Switch XSwitchToSwitch(XElement xSwitch)
        {
            var xId = xSwitch.Attribute("id");
            if (xId == null) throw new Exception("Switch Id attribute not found.");
            var id = int.Parse(xId.Value);
            var xParent = xSwitch.Attribute("parent");
            if (xParent == null) throw new Exception("Switch parent attribute not found.");
            var parentId = int.Parse(xParent.Value);
            var xSwitchId = xSwitch.Attribute("switch");
            if (xSwitchId == null) throw new Exception("Switch attribute not found.");
            var switchId = int.Parse(xSwitchId.Value);

            var cases = xSwitch
                .XPathSelectElements("wf:Case", _workflowMemento.XmlNamespaceManager)
                .Select(xCase =>
                {
                    var xValue = xCase.Attribute("value");
                    if (xValue == null) throw new Exception("Case value attribute not found.");
                    var val = xValue.Value;

                    var nodes = xCase
                        .Elements()
                        .Select(XNodeToNode)
                        .ToArray();

                    var nodeName = string.Format("Switch>Case(value={0})", val);

                    if (CheckStartupNode(nodes) == false)
                    {
                        Logger.Trace("Startup node with parentId=-1 not found in " + nodeName + " execution graph.");
                        throw new Exception();
                    }

                    if (CheckParallelTasks(nodes))
                    {
                        Logger.Trace("Parallel tasks execution detected in " + nodeName + " execution graph.");
                    }

                    CheckInfiniteLoop(nodes, "Infinite loop detected in " + nodeName + " execution graph.");

                    return new Case(val, nodes);
                });

            var xDefault = xSwitch.XPathSelectElement("wf:Default", _workflowMemento.XmlNamespaceManager);
            if (xDefault == null) return new Switch(id, parentId, switchId, cases, null);
            var @default = xDefault
                .Elements()
                .Select(XNodeToNode)
                .ToArray();

            if (@default.Length > 0)
            {
                if (CheckStartupNode(@default) == false)
                {
                    Logger.Trace("Startup node with parentId=-1 not found in Switch>Default execution graph.");
                    throw new Exception();
                }

                if (CheckParallelTasks(@default))
                {
                    Logger.Trace("Parallel tasks execution detected in OnError execution graph.");
                }
                CheckInfiniteLoop(@default, "Infinite loop detected in Switch>Default execution graph.");
            }

            return new Switch(id, parentId, switchId, cases, @default);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xWhile"></param>
        /// <returns></returns>
        While XWhileToWhile(XElement xWhile)
        {
            var xId = xWhile.Attribute("id");
            if (xId == null) throw new Exception("While Id attribute not found.");
            var id = int.Parse(xId.Value);
            var xParent = xWhile.Attribute("parent");
            if (xParent == null) throw new Exception("While parent attribute not found.");
            var parentId = int.Parse(xParent.Value);
            var xWhileId = xWhile.Attribute("while");
            if (xWhileId == null) throw new Exception("While attribute not found.");
            var whileId = int.Parse(xWhileId.Value);

            var doNodes = xWhile
                .Elements()
                .Select(XNodeToNode)
                .ToArray();

            if (CheckStartupNode(doNodes) == false)
            {
                Logger.Trace("Startup node with parentId=-1 not found in DoWhile>Do execution graph.");
                throw new Exception();
            }

            if (CheckParallelTasks(doNodes))
            {
                Logger.Trace("Parallel tasks execution detected in DoWhile>Do execution graph.");
            }

            CheckInfiniteLoop(doNodes, "Infinite loop detected in DoWhile>Do execution graph.");

            return new While(id, parentId, whileId, doNodes);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xIf"></param>
        /// <returns></returns>
        If XIfToIf(XElement xIf)
        {
            var xId = xIf.Attribute("id");
            if (xId == null) throw new Exception("If id attribute not found.");
            var id = int.Parse(xId.Value);

            var xParent = xIf.Attribute("parent");
            if (xParent == null) throw new Exception("If parent attribute not found.");
            var parentId = int.Parse(xParent.Value);           

            // Do nodes
            var doNodes = xIf.XPathSelectElement("wf:Do", _workflowMemento.XmlNamespaceManager)
                .Elements()
                .Select(XNodeToNode)
                .ToArray();

            if (CheckStartupNode(doNodes) == false)
            {
                Logger.Trace("Startup node with parentId=-1 not found in DoIf>Do execution graph.");
                throw new Exception("Eduard macht leere Exceptions");
            }

            if (CheckParallelTasks(doNodes))
            {
                Logger.Trace("Parallel tasks execution detected in DoIf>Do execution graph.");
            }

            CheckInfiniteLoop(doNodes, "Infinite loop detected in DoIf>Do execution graph.");

            // Otherwise nodes
            Node[] elseNodes = null;
            var xElse = xIf.XPathSelectElement("wf:Else", _workflowMemento.XmlNamespaceManager);
            if (xElse != null)
            {
                elseNodes = xElse
                    .Elements()
                    .Select(XNodeToNode)
                    .ToArray();

                if (CheckStartupNode(elseNodes) == false)
                {
                    Logger.Trace("Startup node with parentId=-1 not found in DoIf>Otherwise execution graph.");
                    throw new Exception();
                }

                if (CheckParallelTasks(elseNodes))
                {
                    Logger.Trace("Parallel tasks execution detected in DoIf>Otherwise execution graph.");
                }

                CheckInfiniteLoop(elseNodes, "Infinite loop detected in DoIf>Otherwise execution graph.");
            }

            return new If(id, parentId, doNodes, elseNodes);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="activityMemento"></param>
        /// <returns></returns>
        private Activity LoadActivityInstance(string name, ActivityMemento activityMemento)
        {
            var assemblyName = "CWF.Tasks." + name;
            var typeName = "CWF.Tasks." + name + "." + name + ", " + assemblyName;
            var type = Type.GetType(typeName);

            if (type != null)
            {

                var task = (Activity)Activator.CreateInstance(type, activityMemento);
                return task;
            }
            else
            {
                throw new Exception("The type of the task " + name + " could not be loaded.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="typename"></param>
        /// <returns></returns>
        private object CreateStateToken(string typename)
        {
            object token = null;
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            var correctAssembly = loadedAssemblies.Where(ass => !ass.IsDynamic && ass.ExportedTypes.Select(et => et.Name).Contains(typename)).FirstOrDefault();
            var fsmType = correctAssembly.ExportedTypes.Where(x => x.Name == typename).FirstOrDefault();
            _stateType = fsmType;
            token = Activator.CreateInstance(fsmType);          

            return token;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xdoc"></param>
        /// <param name="attr"></param>
        /// <returns></returns>
        string GetWorkflowAttribute(XDocument xdoc, string attr)
        {
            var xAttribute = xdoc.XPathSelectElement("/wf:Workflow", _workflowMemento.XmlNamespaceManager).Attribute(attr);
            if (xAttribute != null)
            {
                return xAttribute.Value;
            }

            throw new Exception("Workflow attribute " + attr + "not found.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xdoc"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        string GetWorkflowSetting(XDocument xdoc, string name)
        {
            var xAttribute = xdoc
                .XPathSelectElement(
                    string.Format("/wf:Workflow[@id='{0}']/wf:Settings/wf:Setting[@name='{1}']", _workflowMemento.Id, name),
                    _workflowMemento.XmlNamespaceManager)
                .Attribute("value");
            if (xAttribute != null)
            {
                return xAttribute.Value;
            }

            throw new Exception("Workflow setting " + name + " not found.");
        }


        /// <summary>
        /// <exception cref="Exception">Why it's thrown.</exception>
        /// </summary>
        /// <param name="taskNodes"></param>
        /// <param name="errorMsg"></param>
        bool CheckParallelTasks(Node[] taskNodes)
        {
            bool parallelTasksDetected = false;
            foreach (var taskNode in taskNodes)
            {
                if (taskNodes.Count(n => n.ParentId == taskNode.Id) > 1)
                {
                    parallelTasksDetected = true;
                    break;
                }
            }
            return parallelTasksDetected;
        }       
    }
}
