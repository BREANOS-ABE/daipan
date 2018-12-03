//-----------------------------------------------------------------------

// <copyright file="CWFEngine.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Monday, December 3, 2018 3:34:35 PM</date>
// </copyright>

//-----------------------------------------------------------------------

//-----------------------------------------------------------------------

// <copyright file="CWFEngine.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Tuesday, October 30, 2018 2:54:51 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using System.IO;
using System.Threading;
using NLog;
using System.Reflection;
using System.Threading.Tasks;
using BreanosConnectors.Interface;
using Microsoft.Extensions.Configuration;
using System.ComponentModel;

namespace CWF.Core
{
    /// <summary>
    /// Cwf engine.
    /// </summary>
    public class CWFEngine
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private IConfigurationRoot _configuration;
        public IConfigurationRoot Configuration => _configuration;
        /// <summary>
        /// This is the current logger instance
        /// </summary>
        private static NLog.Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Delay time for demo purposes. Should be set to zero for maximum performance.
        /// </summary>
        public int DelayTime { get; set; } = 500;

        //activemq connector
        public BreanosConnectors.ActiveMqConnector.Connector Amqc { get; set; } =  new BreanosConnectors.ActiveMqConnector.Connector();

        /// <summary>
        /// Setting URL for the build in ActiveMQ connector.
        /// </summary>
        public string EndpointUrl { get; private set; } = @"activemq:tcp://192.168.30.125:61616";

        public string User { get; private set; } = "admin";

        public string Passwort { get; private set; } = "admin";

        /// <summary>
        /// Topic name, set in ctor
        /// </summary>
        public string TopicName { get; private set; } = "queue://LineTopic_1";
              
        /// <summary>
        /// 
        /// </summary>
        public string SubscriptionName { get; set; } = "sub2";

        /// <summary>
        /// Settings file path.
        /// </summary>
        public string SettingsFile { get; private set; }
        /// <summary>
        /// Workflows folder path.
        /// </summary>
        public string WorkflowsFolder { get; private set; }       

        /// <summary>
        /// Activities path
        /// </summary>
        public string ActivitiesPath { get; private set; }
        public string StateMachineFolder { get; private set; }

        /// <summary>
        /// XSD path.
        /// </summary>
        public string XsdPath { get; private set; }
       
        public IList<Workflow> Workflows { get; private set; }

        private readonly Dictionary<int, List<CwfTimer>> _cwfTimers = new Dictionary<int, List<CwfTimer>>();

        /// <summary>
        /// Creates a new instance of Cwf engine.
        /// </summary>
        /// <param name="settingsFile">Settings file path.</param>
        public CWFEngine(string settingsFile, int delayTime)
        {
            SettingsFile = settingsFile;

            DelayTime = delayTime;
         
            Logger.Info("Starting Cwf Engine");

            LoadSettings();

            CWFEngineInit();
        }

        /// <summary>
        /// Helper for folder setup.
        /// </summary>
        /// <param name="workflowsFolder"></param>
        /// <param name="xsdPath"></param>
        /// <param name="activitiesPath"></param>
        /// <param name="stateMachineFolder"></param>
        private void SetSettings(string workflowsFolder, string xsdPath, string activitiesPath, string stateMachineFolder)
        {
            Logger.Info($"SetSettings workflowsFolder={workflowsFolder}. xsdPath={xsdPath}. activitiesPath={activitiesPath}. stateMachineFolder={stateMachineFolder}");
            WorkflowsFolder = workflowsFolder;
            XsdPath = xsdPath;
            ActivitiesPath = activitiesPath;
            StateMachineFolder = stateMachineFolder;
        }

        /// <summary>
        /// Initialisation
        /// </summary>
        private void CWFEngineInit()
        {
            Workflows = new List<Workflow>();            

            LoadWorkflows();           
        }

        /// <summary>
        /// public ctor
        /// </summary>
        /// <param name="WorkflowsFolder"></param>
        /// <param name="XsdFilePath"></param>
        /// <param name="ActivitiesPath"></param>
        /// <param name="StateMachinePath"></param>
        public CWFEngine(string WorkflowsFolder, string XsdFilePath, string ActivitiesPath, string StateMachinePath)
        {
            Logger.Info("Starting Cwf Engine CWFEngine(string WorkflowsFolder, string XsdPath, string ActivitiesPath, string StateMachinePath) ctor");

            SetSettings(WorkflowsFolder, XsdFilePath, ActivitiesPath, StateMachinePath);

            CWFEngineInit();
        }

        /// <summary>
        /// public ctor
        /// </summary>
        /// <param name="WorkflowsFolder"></param>
        /// <param name="XsdFilePath"></param>
        /// <param name="ActivitiesPath"></param>
        /// <param name="StateMachinePath"></param>
        /// <param name="delayTime"></param>
        public CWFEngine(string WorkflowsFolder, string XsdFilePath, string ActivitiesPath, string StateMachinePath, int delayTime)
        {
            Logger.Info("Starting Cwf Engine CWFEngine(string WorkflowsFolder, string XsdPath, string ActivitiesPath, string StateMachinePath) ctor");

            SetSettings(WorkflowsFolder, XsdFilePath, ActivitiesPath, StateMachinePath);

            DelayTime = delayTime;

            CWFEngineInit();
        }

        /// <summary>
        /// To enable Amqp in the workflow
        /// </summary>
        private async void ConfigureAmqp()
        {
            try
            {
                _configuration = new ConfigurationBuilder()

                    .AddXmlFile("app.config")
                    .Build();
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
            }

            EndpointUrl = Configuration["connection:Endpoint"];
            User = Configuration["connection:User"];
            Passwort = Configuration["connection:Password"];
            TopicName = Configuration["connection:Topic"];

            Logger.Info($"Reading data from app.config. EndpointUrl={EndpointUrl}. User={User}. Passwort={Passwort}. TopicName={TopicName}");

            Task<bool> connectionEstablished = Amqc.ConnectAsync(EndpointUrl, User, Passwort);          

            bool ok = await connectionEstablished;
            if (!ok)
            {
                logger.Error("Could not establish connection!");
            }

            Amqc.Message += Amqc_LineTopicMessage;

            Task<bool> listenEstablished = Amqc.ListenAsync(TopicName, SubscriptionName);
            bool testok = await listenEstablished;
            if (!testok)
            {
                logger.Error("Could not establish listen!");
            }
        }
      
        /// <summary>
        /// Endpoint URL for Service Bus
        /// </summary>
        private void Amqc_LineTopicMessage(object sender, OnMessageEventArgs e)
        {
            logger.Info(sender.ToString() + "LineTopic called");

        }

        /// <summary>
        /// 
        /// </summary>
        void LoadSettings()
        {
            var xdoc = XDocument.Load(SettingsFile);
            WorkflowsFolder = GetCwfSetting(xdoc, "workflowsFolder");
            
            XsdPath = GetCwfSetting(xdoc, "xsd");
            ActivitiesPath = GetCwfSetting(xdoc, "activitiesFolder");
            StateMachineFolder = GetCwfSetting(xdoc, "fsmFolder");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xdoc"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        string GetCwfSetting(XDocument xdoc, string name)
        {
            try
            {
                var xValue = xdoc.XPathSelectElement(string.Format("/Cwf/Setting[@name='{0}']", name)).Attribute("value");
                if (xValue == null) throw new Exception("Cwf Setting Value attribute not found.");
                return xValue.Value;
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("An error occured when reading Cwf settings: Setting[@name='{0}']", e, name);
                return string.Empty;
            }
        }  

        /// <summary>
        /// Load WorkflowFromDirectory
        /// </summary>
        void LoadWorkflowsFromDirectory()
        {
            foreach (string file in Directory.GetFiles(WorkflowsFolder,"*.xml"))
            {
                var workflow = LoadWorkflowFromFile(file);
                if (workflow != null)
                {
                    Workflows.Add(workflow);
                }
            }
        }

        /// <summary>
        /// Workflow Watcher
        /// </summary>
        void ConfigureWorkflowWatcher()
        {
            var watcher = new FileSystemWatcher(WorkflowsFolder, "*.xml")
            {
                EnableRaisingEvents = true,
                IncludeSubdirectories = false
            };

            watcher.Created += (_, args) =>
            {
                var workflow = LoadWorkflowFromFile(args.FullPath);
                if (workflow != null)
                {
                    Workflows.Add(workflow);
                    ScheduleWorkflow(workflow);
                }
            };

            watcher.Deleted += (_, args) =>
            {
                var removedWorkflow = Workflows.SingleOrDefault(wf => wf.WorkflowFilePath == args.FullPath);
                if (removedWorkflow != null)
                {
                    Logger.InfoFormat("Workflow {0} is stopped and removed because its definition file {1} was deleted", removedWorkflow.Name, removedWorkflow.WorkflowFilePath);
                    removedWorkflow.Stop();
                    Workflows.Remove(removedWorkflow);
                }
            };

            watcher.Changed += (_, args) =>
            {
                try
                {
                    if (Workflows != null)
                    {
                        var changedWorkflow = Workflows.SingleOrDefault(wf => wf.WorkflowFilePath == args.FullPath);

                        if (changedWorkflow != null)
                        {
                            // the existing file might have caused an error during loading, so there may be no corresponding
                            // workflow to the changed file
                            changedWorkflow.Stop();
                            Workflows.Remove(changedWorkflow);
                            Logger.InfoFormat("A change in the definition file {0} of workflow {1} has been detected. The workflow will be reloaded", changedWorkflow.WorkflowFilePath, changedWorkflow.Name);
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error("Error during workflow reload", e);
                }

                var reloaded = LoadWorkflowFromFile(args.FullPath);
                if (reloaded != null)
                {
                    var duplicateId = Workflows.SingleOrDefault(wf => wf.Id == reloaded.Id);
                    if (duplicateId != null)
                    {
                        Logger.ErrorFormat(
                            "An error occured while loading the workflow : {0}. The workflow Id {1} is already assgined in {2}",
                            args.FullPath, reloaded.Id, duplicateId.WorkflowFilePath);
                    }
                    else
                    {
                        Workflows.Add(reloaded);
                        ScheduleWorkflow(reloaded);
                    }
                }
            };
        }

        /// <summary>
        /// 
        /// </summary>
        void LoadWorkflows()
        {
            LoadStateMachines();
            //Prepare external Activities
            LoadExternalActivities();

            LoadWorkflowsFromDirectory();

            ConfigureWorkflowWatcher();
        }

        /// <summary>
        /// Load State dlls
        /// </summary>
        void LoadStateMachines()
        {
            Logger.Info($"{nameof(LoadStateMachines)}");
            try
            {
                var files = Directory.GetFiles(StateMachineFolder, "*.dll", SearchOption.AllDirectories);
                var assemblies = files.Select(file => Assembly.LoadFrom(file));
                logger.Info($"Loaded {assemblies.Count()} state machine assemblies");
                foreach (var assembly in assemblies)
                {
                    AppDomain.CurrentDomain.Load(assembly.GetName());
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Exception on trying to load state machines {e.ToString()}");
            }
        }
        /// <summary>
        /// Load activities dlls from directory
        /// </summary>
        void LoadExternalActivities()
        {
            Logger.Trace("LoadExternalActivities entered");
            try
            {
                var files = Directory.GetFiles(ActivitiesPath, "*.dll", SearchOption.AllDirectories)
                .Where(s => s.EndsWith(".dll", StringComparison.OrdinalIgnoreCase));

                foreach (string s in files)
                {
                    Assembly assembly = Assembly.LoadFrom(s);
                    Logger.Trace($"Loading {assembly.GetName()}");
                    AppDomain.CurrentDomain.Load(assembly.GetName());
                }

            }
            catch (Exception e)
            {
                Logger.ErrorFormat("An error occured while loading ExternalActivities", e);
            }
            Logger.Trace("LoadExternalActivities leaved");
        }

        /// <summary>
        /// Parse a workflow from a dll
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        Workflow LoadWorkflowFromFile(string file)
        {
            try
            {
                Parser parser = new Parser();
               
                var wf = parser.CheckAndLoadWorkflow(this, file, XsdPath);                
                wf.PropertyChanged += Wf_PropertyChanged;
                wf.DelayTime = DelayTime;
                Logger.InfoFormat("Workflow loaded: {0} ({1})", wf, file);
                return wf;
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("An error occured while loading the workflow : {0} Please check the workflow configuration. Error: {1}", file, e.Message);
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Wf_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
        }

        /// <summary>
        /// Starts Cwf engine.
        /// </summary>
        public void Run()
        {
            if (Workflows.Count == 0)
                Logger.Info($"No Workflows in collection!");

            foreach (Workflow workflow in Workflows)
            {
                ScheduleWorkflow(workflow);
            }
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wf"></param>
        private void ScheduleWorkflow(Workflow wf)
        {
            if (wf.IsEnabled)
            {
                if (wf.LaunchType == LaunchType.Startup)
                {
                    wf.Start();
                }
                else if (wf.LaunchType == LaunchType.Periodic)
                {
                    Action<object> callback = o =>
                    {
                        var workflow = o as Workflow;
                        if (workflow != null && !workflow.IsRunning)
                        {
                            workflow.Start();
                        }
                    };

                    var timer = new CwfTimer(new TimerCallback(callback), wf, wf.Period);

                    if (!_cwfTimers.ContainsKey(wf.Id))
                    {
                        _cwfTimers.Add(wf.Id, new List<CwfTimer>{ timer });
                    }
                    else
                    {
                        foreach (var wt in _cwfTimers[wf.Id])
                        {
                            wt.Stop();
                        }
                        _cwfTimers[wf.Id].Add(timer);
                    }
                    timer.Start();
                }
            }
        }

        /// <summary>
        /// Stops Cwf engine.
        /// </summary>
        public void Stop()
        {
            foreach (var wts in _cwfTimers.Values)
            {
                foreach (var wt in wts)
                {
                    wt.Stop();
                }
            }

            foreach (var wf in Workflows)
            {
                if (wf.IsRunning)
                {
                    wf.Stop();
                }
            }
        }

        /// <summary>
        /// Gets a workflow.
        /// </summary>
        /// <param name="workflowId">Workflow Id.</param>
        /// <returns></returns>
        public Workflow GetWorkflow(int workflowId)
        {
            return Workflows.FirstOrDefault(wf => wf.Id == workflowId);
        }

        /// <summary>
        /// Starts a workflow.
        /// </summary>
        /// <param name="workflowId">Workflow Id.</param>
        public void StartWorkflow(int workflowId)
        {
            var wf = GetWorkflow(workflowId);

            if (wf == null)
            {
                Logger.ErrorFormat("Workflow {0} not found.", workflowId);
            }
            else
            {
                if (wf.IsEnabled) wf.Start();
            }
        }

        /// <summary>
        /// Stops a workflow.
        /// </summary>
        /// <param name="workflowId">Workflow Id.</param>
        public void StopWorkflow(int workflowId)
        {
            var wf = GetWorkflow(workflowId);

            if (wf == null)
            {
                Logger.ErrorFormat("Workflow {0} not found.", workflowId);
            }
            else
            {
                if (wf.IsEnabled) wf.Stop();
            }
        }

        /// <summary>
        /// Suspends a workflow.
        /// </summary>
        /// <param name="workflowId">Workflow Id.</param>
        public void PauseWorkflow(int workflowId)
        {
            var wf = GetWorkflow(workflowId);

            if (wf == null)
            {
                Logger.ErrorFormat("Workflow {0} not found.", workflowId);
            }
            else
            {
                if (wf.IsEnabled) wf.Pause();
            }
        }

        /// <summary>
        /// Resumes a workflow.
        /// </summary>
        /// <param name="workflowId">Workflow Id.</param>
        public void ResumeWorkflow(int workflowId)
        {
            var wf = GetWorkflow(workflowId);

            if (wf == null)
            {
                Logger.ErrorFormat("Workflow {0} not found.", workflowId);
            }
            else
            {
                if (wf.IsEnabled) wf.Resume();
            }
        }
    }
}
