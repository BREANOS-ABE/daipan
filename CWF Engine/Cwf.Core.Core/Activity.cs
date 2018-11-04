//-----------------------------------------------------------------------

// <copyright file="Activity.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Monday, November 5, 2018 12:22:54 AM</date>
// </copyright>

//-----------------------------------------------------------------------


using CWF.Core.ExecutionGraph;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using static CWF.Core.Parser;

namespace CWF.Core
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class StatefulActivity<T> : Activity
    {
        public T StateToken { get; set; }
        protected StatefulActivity(ActivityMemento activityMemento) : base(activityMemento)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        protected void RunInit(object token)
        {
            StateToken = (T)token;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void FinishActivity()
        {

            FinishActivity(new OnActivityFinishedEventArgs()
            {
                Status = new ActivityStatus(this, Status.Success, false, null)
                {
                    ActivityId = this.Id,
                    NewState = StateToken,
                    ActivityName = this.Name,                  
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="switchCase"></param>
        protected override void FinishActivityAsSuccess(bool condition, string switchCase = null)
        {

            FinishActivity(new OnActivityFinishedEventArgs()
            {
                Status = new ActivityStatus(this, Status.Success, condition, switchCase)
                {
                    ActivityId = this.Id,
                    NewState = StateToken,
                    ActivityName = this.Name,                   
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="switchCase"></param>
        protected override void FinishActivityAsError(bool condition, string switchCase = null)
        {

            FinishActivity(new OnActivityFinishedEventArgs()
            {
                Status = new ActivityStatus(this, Status.Error, condition, switchCase)
                {
                    ActivityId = this.Id,
                    NewState = StateToken,
                    ActivityName = this.Name,                   
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="switchCase"></param>
        protected override void FinishActivityAsWarning(bool condition, string switchCase = null)
        {

            FinishActivity(new OnActivityFinishedEventArgs()
            {
                Status = new ActivityStatus(this, Status.Warning, condition, switchCase)
                {
                    ActivityId = this.Id,
                    NewState = StateToken,
                    ActivityName = this.Name,                   
                }
            });
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class OnActivityFinishedEventArgs
    {
        public Dictionary<string, object> Properties { get; set; }
        public ActivityStatus Status { get; set; }
    }

    /// <summary>
    /// Task.
    /// </summary>
    public abstract class Activity
    {

        public delegate void OnFinsihed(object sender, OnActivityFinishedEventArgs e);
        public event OnFinsihed Finished;
        /// <summary>
        /// Task Id.
        /// </summary>
        public int Id { get; private set; }
        /// <summary>
        /// Task name.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Task description.
        /// </summary>
        public string Description { get; private set; }
        /// <summary>
        /// Shows whether this task is enabled or not.
        /// </summary>
        public bool IsEnabled { get; private set; }
        /// <summary>
        /// Task settings.
        /// </summary>
        public Setting[] Settings { get; private set; }
        /// <summary>
        /// Workflow.
        /// </summary>
        public Workflow Workflow { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public enum GuardType
        {
            And,
            Or,
        };

        /// <summary>
        /// 
        /// </summary>
        public GuardType Guard { get; set; }       

        /// <summary>
        /// 
        /// </summary>
        /// <param name="activityMemento"></param>
        protected Activity(ActivityMemento activityMemento)
        {
            Id = activityMemento.Id;
            Name = activityMemento.Name;
            Description = activityMemento.Description;
            IsEnabled = activityMemento.IsEnabled;
            Workflow = activityMemento.Workflow;
            Settings = activityMemento.Settings;
            Guard = activityMemento.Guard;
        }
             
        /// <summary>
        /// Starts the task.
        /// </summary>
        /// <returns>Task status.</returns>
        public abstract void Run(System.Threading.CancellationToken token, object state, object parameterDto = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="switchCase"></param>
        protected virtual void FinishActivityAsSuccess(bool condition, string switchCase = null)
        {
            FinishActivity(new OnActivityFinishedEventArgs()
            {
                Status = new ActivityStatus(this, Status.Success, condition, switchCase)
                {
                    ActivityId = this.Id,
                    NewState = null,
                    ActivityName = this.Name,                   
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="switchCase"></param>
        protected virtual void FinishActivityAsError(bool condition, string switchCase = null)
        {
            FinishActivity(new OnActivityFinishedEventArgs()
            {
                Status = new ActivityStatus(this, Status.Error, condition, switchCase)
                {
                    ActivityId = this.Id,
                    NewState = null,
                    ActivityName = this.Name,                   
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="switchCase"></param>
        protected virtual void FinishActivityAsWarning(bool condition, string switchCase = null)
        {
            FinishActivity(new OnActivityFinishedEventArgs()
            {
                Status = new ActivityStatus(this, Status.Warning, condition, switchCase)
                {
                    ActivityId = this.Id,
                    NewState = null,
                    ActivityName = this.Name,                    
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        protected void FinishActivity(OnActivityFinishedEventArgs args)
        {            
            Finished?.Invoke(this, args);
        }       

        /// <summary>
        /// 
        /// </summary>
        protected virtual void FinishActivity()
        {            
            Finished?.Invoke(this, new OnActivityFinishedEventArgs()
            {
                Status = new ActivityStatus(this, Status.Success, false, null)
                {
                    ActivityId = this.Id,
                    NewState = null,
                    ActivityName = this.Name,                    
                }
            });
        }                 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        private string BuildLogMsg(string msg)
        {
            return string.Format("{0} [{1}] {2}", Workflow.LogTag, GetType().Name, msg);
        }

        /// <summary>
        /// Logs an information message.
        /// </summary>
        /// <param name="msg">Log message.</param>
        public void Info(string msg)
        {
            Logger.Info(BuildLogMsg(msg));
        }

        /// <summary>
        /// Logs a formatted information message.
        /// </summary>
        /// <param name="msg">Formatted log message.</param>
        /// <param name="args">Arguments.</param>
        public void InfoFormat(string msg, params object[] args)
        {
            Logger.InfoFormat(BuildLogMsg(msg), args);
        }

        /// <summary>
        /// Logs a Debug log message.
        /// </summary>
        /// <param name="msg">Log message.</param>
        public void Debug(string msg)
        {
            Logger.Debug(BuildLogMsg(msg));
        }

        /// <summary>
        /// Logs a formatted debug message.
        /// </summary>
        /// <param name="msg">Log message.</param>
        /// <param name="args">Arguments.</param>
        public void DebugFormat(string msg, params object[] args)
        {
            Logger.DebugFormat(BuildLogMsg(msg), args);
        }

        /// <summary>
        /// Logs an error log message.
        /// </summary>
        /// <param name="msg">Log message.</param>
        public void Error(string msg)
        {
            Logger.Error(BuildLogMsg(msg));
        }

        /// <summary>
        /// Logs a formatted error message.
        /// </summary>
        /// <param name="msg">Log message.</param>
        /// <param name="args">Arguments.</param>
        public void ErrorFormat(string msg, params object[] args)
        {
            Logger.ErrorFormat(BuildLogMsg(msg), args);
        }

        /// <summary>
        /// Logs an error message and an exception.
        /// </summary>
        /// <param name="msg">Log message.</param>
        /// <param name="e">Exception.</param>
        public void Error(string msg, Exception e)
        {
            Logger.Error(BuildLogMsg(msg), e);
        }

        /// <summary>
        /// Logs a formatted log message and an exception.
        /// </summary>
        /// <param name="msg">Formatted log message.</param>
        /// <param name="e">Exception.</param>
        /// <param name="args">Arguments.</param>
        public void ErrorFormat(string msg, Exception e, params object[] args)
        {
            Logger.Error(string.Format(BuildLogMsg(msg), args), e);
        }
    }
}
