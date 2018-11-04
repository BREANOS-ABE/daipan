//-----------------------------------------------------------------------

// <copyright file="PostHanoiStateToActiveMQActivity.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Monday, November 5, 2018 12:22:54 AM</date>
// </copyright>

//-----------------------------------------------------------------------


using System;
using System.Xml.Linq;
using BreanosConnectors;
using CWF.Core;
using CWF.Core.ExecutionGraph;
using HanoiLibrary;
using NLog;
using static CWF.Core.Parser;

namespace CWF.Tasks.PostHanoiStateToActiveMQActivity
{
    public class PostHanoiStateToActiveMQActivity : CWF.Core.StatefulActivity<HanoiWorkflowState>
    {
        private static NLog.Logger logger = LogManager.GetCurrentClassLogger();
        public PostHanoiStateToActiveMQActivity(ActivityMemento activityMememnto) : base(activityMememnto)
        {
        }

        public override void Run(System.Threading.CancellationToken token, object state, object parameterDto = null)
        {
            if (!token.IsCancellationRequested)
            {
                var s = state as HanoiLibrary.HanoiWorkflowState;
                StateToken = s;

                StateToken.PropertyChanged += StateToken_PropertyChanged;
                StateToken.DiskBaseWidth = 30;

                string serialized = SerializationHelper.Pack(s);
                Core.Logger.InfoFormat($"PostHanoiStateToActiveMQActivity sending...{serialized}");
                Workflow.Engine.Amqc.SendAsync(serialized, Workflow.Engine.TopicName, typeof(string).AssemblyQualifiedName);
            }
            else
            {
                Core.Logger.InfoFormat($"PostHanoiStateToActiveMQActivity has been canceled by user");
            }
            FinishActivity();
        }

        private void StateToken_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            
        }
    }
}
