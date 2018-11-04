//-----------------------------------------------------------------------

// <copyright file="CheckHanoiNotFinishedActivity.cs" company="Breanos GmbH">
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
using CWF.Core;
using CWF.Core.ExecutionGraph;
using NLog;
using static CWF.Core.Parser;

namespace CWF.Tasks.CheckHanoiNotFinishedActivity
{
    public class CheckHanoiNotFinishedActivity : CWF.Core.StatefulActivity<HanoiLibrary.HanoiWorkflowState>
    {
        private static NLog.Logger logger = LogManager.GetCurrentClassLogger();
        public CheckHanoiNotFinishedActivity(ActivityMemento activityMemento) : base(activityMemento)
        {
        }

        public override void Run(System.Threading.CancellationToken token, object state, object parameterDto = null)
        {
            bool isFinished = false;
            if (!token.IsCancellationRequested)
            {
                isFinished = IsFinished(state as HanoiLibrary.HanoiWorkflowState);
            }
            else
            {
                Core.Logger.InfoFormat($"CheckHanoiNotFinishedActivity has been canceled by user");
            }

            Core.Logger.InfoFormat($"CheckHanoiNotFinishedActivity {isFinished}");
            FinishActivityAsSuccess(isFinished);
        }

        private bool IsFinished(HanoiLibrary.HanoiWorkflowState state)
        {
            if (state.NumberDisks % 2 == 0)
            {
                return state.Stack2.Count != state.NumberDisks;

            }
            else
                return state.Stack3.Count != state.NumberDisks;
        }
    }
}
