//-----------------------------------------------------------------------

// <copyright file="SetupHanoiGameActivity.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Monday, November 5, 2018 12:22:54 AM</date>
// </copyright>

//-----------------------------------------------------------------------


using System.Collections.Generic;
using CWF.Core.ExecutionGraph;
using HanoiLibrary;
using static CWF.Core.Parser;

namespace CWF.Tasks.SetupHanoiGameActivity
{
    public class SetupHanoiGameActivity : Core.StatefulActivity<HanoiWorkflowState>
    {
        public SetupHanoiGameActivity(ActivityMemento activityMemento) : base(activityMemento)
        {
        }

        public override void Run(System.Threading.CancellationToken token, object state, object parameterDto = null)
        {
            if (!token.IsCancellationRequested)
            {
                var parameters = parameterDto as HanoiSetupConfiguration;
                var s = state as HanoiWorkflowState;
                if (s == null) s = new HanoiWorkflowState();
                s.DiskBaseWidth = 20;
                s.Round = 0;
                s.Stack1 = new List<HanoiDisk>();
                s.Stack2 = new List<HanoiDisk>();
                s.Stack3 = new List<HanoiDisk>();
                StateToken = s;
                s.NumberDisks = parameters.NumberDisks;
                for (int i = 0; i < s.NumberDisks; i++)
                {
                    s.Stack1.Add(new HanoiDisk() { DiskSize = (i + 1) * s.DiskBaseWidth });
                }
                FinishActivityAsSuccess(true);
                return;
            }
            else
            {
                Core.Logger.InfoFormat($"SetupHanoiGameActivity has been canceled by user");
            }
            FinishActivityAsSuccess(false);
        }
    }
}
