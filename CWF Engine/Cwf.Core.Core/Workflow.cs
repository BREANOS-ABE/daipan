//-----------------------------------------------------------------------

// <copyright file="Workflow.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Monday, November 5, 2018 12:22:54 AM</date>
// </copyright>

//-----------------------------------------------------------------------


using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using CWF.Core.ExecutionGraph;

namespace CWF.Core
{
    
    /// <summary>
    /// Worflow.
    /// </summary>
    public class Workflow
    {

        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Workflow file path.
        /// </summary>
        public string WorkflowFilePath { get; private set; }

        public CWFEngine Engine { get; set; }

        /// <summary>
        /// Workflow Id.
        /// </summary>
        public int Id { get; private set; }
        /// <summary>
        /// Workflow name.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Workflow description.
        /// </summary>
        public string Description { get; private set; }
        /// <summary>
        /// Workflow lanch type.
        /// </summary>
        public LaunchType LaunchType { get; private set; }
        /// <summary>
        /// Workflow period.
        /// </summary>
        public TimeSpan Period { get; private set; }
        /// <summary>
        /// Shows whether this workflow is enabled or not.
        /// </summary>
        public bool IsEnabled { get; private set; }
        /// <summary>
        /// Shows whether this workflow is running or not.
        /// </summary>
        public bool IsRunning { get; private set; }
        /// <summary>
        /// Shows whether this workflow is suspended or not.
        /// </summary>
        public bool IsPaused { get; private set; }
        /// <summary>
        /// Workflow tasks.
        /// </summary>
        public Activity[] Activities { get; set; }

        /// <summary>
        /// These are the new defined transitions.
        /// </summary>
        public Transition[] Transitions { get; set; }

        /// <summary>
        /// Default parent node id to start with in the execution graph.
        /// </summary>
        public const int StartId = -1;

        /// <summary>
        /// Job Id.
        /// </summary>
        public int JobId { get; private set; }
        /// <summary>
        /// State Token
        /// </summary>
        public INotifyPropertyChanged StateToken { get; set; }
        /// <summary>
        /// Log tag.
        /// </summary>
        public string LogTag { get { return string.Format("[{0} / {1}]", Name, JobId); } }

        /// <summary>
        /// Execution graph.
        /// </summary>
        public Graph ActivitySetup { get; set; }

        /// <summary>
        /// Hashtable used as shared memory for tasks.
        /// </summary>
        public Hashtable Hashtable { get; private set; }

        /// <summary>
        /// Xml Namespace Manager.
        /// </summary>
        public XmlNamespaceManager XmlNamespaceManager { get; private set; }
        public int DelayTime { get; set; } = 500;

        private Task _task;
        public CancellationTokenSource CancellationSource { get; private set; } = new CancellationTokenSource();
        CancellationToken _cancelationToken;        

        private ConcurrentDictionary<int, Task> _runningActivities;
        private ConcurrentDictionary<int, List<int>> _incomingTransitionsOfActivities;
        public Workflow(CWFEngine cwfEngine, Parser.WorkflowMemento memento)
        {
            Description = memento.Description;
            Id = memento.Id;
            IsEnabled = memento.IsEnabled;
            LaunchType = memento.LaunchType;
            Name = memento.Name;
            Period = memento.Period;
            StateToken = memento.StateToken as INotifyPropertyChanged;
            WorkflowFilePath = memento.WorkflowFilePath;
            XmlNamespaceManager = memento.XmlNamespaceManager;
            StateToken.PropertyChanged += StateToken_PropertyChanged;
            Engine = cwfEngine;
            _runningActivities = new ConcurrentDictionary<int, Task>();
            _incomingTransitionsOfActivities = new ConcurrentDictionary<int, List<int>>();
            _cancelationToken = CancellationSource.Token;
        }
        /// <summary>
        /// Returns informations about this workflow.
        /// </summary>
        /// <returns>Informations about this workflow.</returns>
        public override string ToString()
        {
            return string.Format("{{id: {0}, name: {1}, enabled: {2}, launchType: {3}}}", Id, Name, IsEnabled, LaunchType);
        }
        public void Activity_Finished(object sender, OnActivityFinishedEventArgs e)
        {
            _runningActivities.TryRemove(e.Status.ActivityId, out var finishedActivity);

            if (DelayTime != 0)
            {
                Task.Delay(DelayTime).Wait();
            }

            var possibleTransitions = GetTransitionsWithFrom(e.Status.ActivityId);
            foreach (var pT in possibleTransitions)
            {
                Task.Run(() =>
                {
                    if (pT.CheckCondition(StateToken))
                    {
                        var nextActivity = GetActivity(pT.To);
                        if (nextActivity == null)
                        {
                            logger.Error($"A fired transition with id {pT.Id} referenced an activity with id {pT.To} that was not found");
                            return;
                        }
                        else
                        {
                            if (!_runningActivities.ContainsKey(nextActivity.Id))
                            {                               
                                if (!_incomingTransitionsOfActivities.ContainsKey(nextActivity.Id))
                                    _incomingTransitionsOfActivities[nextActivity.Id] = new List<int>();
                                _incomingTransitionsOfActivities[nextActivity.Id].Add(pT.Id);

                                bool isRunActivity = false;
                                switch (nextActivity.Guard)
                                {
                                    case Activity.GuardType.And:
                                        var requiredTrueTransitions = GetTransitionsWithTo(nextActivity.Id).Select(t => t.Id);
                                        if (requiredTrueTransitions.All(rtt => _incomingTransitionsOfActivities[nextActivity.Id].Contains(rtt)))
                                            isRunActivity = true;
                                        break;
                                    case Activity.GuardType.Or:
                                        isRunActivity = true;
                                        break;
                                    default:
                                        break;
                                }
                                if (isRunActivity)
                                {
                                    _runningActivities[nextActivity.Id] = Task.Run(() =>
                                        {
                                            var parameters = GetParamObjectForActivity(nextActivity);
                                            nextActivity.Run(_cancelationToken, StateToken, parameters);                                            
                                        }, _cancelationToken); //needs parameters

                                    _incomingTransitionsOfActivities.TryRemove(e.Status.ActivityId, out var finishedActivityTransitionList);
                                }                               
                            }
                            else
                            {
                                var state = _runningActivities[nextActivity.Id].Status.ToString();
                                logger.Trace($"Did not start activity {nextActivity.Id} as it was already running in state {state}");
                            }
                        }
                    }
                });
            }
        }
        /// <summary>
        /// Starts this workflow.
        /// </summary>
        public void Start()
        {
            if (IsRunning) return;

            CancellationSource = new CancellationTokenSource();//reset
            _cancelationToken = CancellationSource.Token;           

            _task = Task.Run(() =>
                {
                    try
                    {
                        IsRunning = true;
                        Logger.InfoFormat("{0} Workflow started.", LogTag);
                        var initTransition = GetStartTransition();
                        if (initTransition == null)
                        {
                            logger.Error($"Could not start workflow as there was no init-transition. An init-transition must have a 'from' value of '-1'");
                            return;
                        }
                        var initActivity = GetActivity(initTransition.To);
                        if (initActivity == null)
                        {
                            logger.Error($"Could not start workflow as the init-transition's 'to' value did not match any known activity id");
                            return;
                        }
                        var parameters = GetParamObjectForActivity(initActivity);
                        initActivity.Run(_cancelationToken, StateToken, parameters); //needs parameters
                    }
                    catch (ThreadAbortException eAbort)
                    {
                        Logger.ErrorFormat("Thread aborted while running the workflow. Error: {0}", eAbort.Message, this);
                    }
                    catch (Exception e)
                    {
                        Logger.ErrorFormat("An error occured while running the workflow. Error: {0}", e.Message, this);
                    }
                    finally
                    {
                        // Cleanup                      
                        GC.Collect();

                        Logger.InfoFormat("{0} Workflow finished.", LogTag);
                        JobId++;
                    }
                }, _cancelationToken);           
        }
        /// <summary>
        /// Stops this workflow.
        /// </summary>
        public async void Stop()
        {            
            if (IsRunning)
            {               
                try
                {                   
                    CancellationSource.Cancel();
                    Task.Delay(1000).Wait();
                }
                catch (Exception e)
                {
                    Logger.ErrorFormat("An error occured while stopping the workflow : {0}", e, this);
                }

                //Cancel every activity
                foreach (KeyValuePair<int, Task> item in _runningActivities)
                {
                    Task removedTask;
                    _runningActivities.TryRemove(item.Key, out removedTask);                    
                }
               
                IsRunning = false;
            }
        }
        /// <summary>
        /// Suspends this workflow.
        /// </summary>
        public void Pause()
        {
            if (IsRunning)
            {
                try
                {
                    IsPaused = true;
                    throw new NotImplementedException();
                }
                catch (Exception e)
                {
                    Logger.ErrorFormat("An error occured while suspending the workflow : {0}", e, this);
                }
            }
        }
        /// <summary>
        /// Resumes this workflow.
        /// </summary>
        public void Resume()
        {
            if (IsPaused)
            {
                try
                {
                    throw new NotImplementedException();
                }
                catch (Exception e)
                {
                    Logger.ErrorFormat("An error occured while resuming the workflow : {0}", e, this);
                }
                finally
                {
                    IsPaused = false;
                }
            }
        }
        private Activity GetActivity(int id)
        {
            return Activities.FirstOrDefault(t => t.Id == id);
        }
        private Transition GetStartTransition()
        {
            return Transitions.Where(t => t.From == StartId).FirstOrDefault();
        }
        private IEnumerable<Transition> GetTransitionsWithTo(int id)
        {
            return Transitions.Where(t => t.To == id);
        }
        private IEnumerable<Transition> GetTransitionsWithFrom(int id)
        {
            return Transitions.Where(t => t.From == id);
        }
        private object GetParamObjectForActivity(Activity a)
        {
            if (!a.Settings.Any(s => s.Name == "ParameterType")) return null;
            var typename = a.Settings.Where(s => s.Name == "ParameterType").FirstOrDefault().Value;
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            var correctAssembly = loadedAssemblies.Where(ass => !ass.IsDynamic && ass.ExportedTypes.Select(et => et.Name).Contains(typename)).FirstOrDefault();
            if (correctAssembly == null) return null;
            Type paramType = correctAssembly.ExportedTypes.Where(x => x.Name == typename).FirstOrDefault();
            if (paramType == null) return null;
            var parameterDto = Activator.CreateInstance(paramType);
            foreach (var prop in paramType.GetProperties())
            {
                var value = a.Settings.Where(s => s.Name == prop.Name).FirstOrDefault().Value;
                if (value != null)
                {
                    //needs type transformation
                    var propType = prop.PropertyType.Name;
                    switch (propType)
                    {
                        case string s when s == typeof(int).Name:
                            int.TryParse(value, out int actualInt);
                            prop.SetValue(parameterDto, actualInt);
                            break;
                        case string s when s == typeof(double).Name:
                            double.TryParse(value, out double actualDouble);
                            prop.SetValue(parameterDto, actualDouble);
                            break;
                        case string s when s == typeof(long).Name:
                            long.TryParse(value, out long actualLong);
                            prop.SetValue(parameterDto, actualLong);
                            break;
                        case string s when s == typeof(float).Name:
                            float.TryParse(value, out float actualFloat);
                            prop.SetValue(parameterDto, actualFloat);
                            break;
                        case string s when s == typeof(string).Name:
                            prop.SetValue(parameterDto, value);
                            break;
                        case string s when s == typeof(bool).Name:
                            bool.TryParse(value, out bool actualBool);
                            prop.SetValue(parameterDto, actualBool);
                            break;
                        case string s when s == typeof(DateTime).Name:
                            DateTime.TryParse(value, out DateTime actualDateTime);
                            prop.SetValue(parameterDto, actualDateTime);
                            break;
                        default:
                            break;
                    }
                }
            }
            return parameterDto;
        }
        private void StateToken_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(sender, e);
        }
    }
}
