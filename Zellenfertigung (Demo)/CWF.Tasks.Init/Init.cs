//-----------------------------------------------------------------------

// <copyright file="Init.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Monday, December 3, 2018 3:34:35 PM</date>
// </copyright>

//-----------------------------------------------------------------------

//-----------------------------------------------------------------------

// <copyright file="RunHanoiRoundActivity.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Tuesday, October 30, 2018 2:54:51 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using FertigungszelleLibaryStandard;
using NLog;
using System;
using static CWF.Core.Parser;

namespace CWF.Tasks.Init
{
    public class Init : CWF.Core.StatefulActivity<FertigungszelleWorkflowState>
    {
        private static NLog.Logger logger = LogManager.GetCurrentClassLogger();
        public Init(ActivityMemento activityMememnto) : base(activityMememnto)
        {
        }

        public override void Run(System.Threading.CancellationToken token, object state, object parameterDto = null)
        {
            if (!token.IsCancellationRequested)
            {
                var s = state as FertigungszelleWorkflowState;
                StateToken = s;

                Console.WriteLine("Setting Parameters...");

                StateToken.Activityerror = false;
                StateToken.Machineerror = false;

                var parameters = parameterDto as WorkPieceCount;
                StateToken.Workpiececount = parameters.WorkPieceCountNumber;

                Console.WriteLine($"Parameters have been set. Max WorkpieceCounter is: {StateToken.Workpiececount} ");

                //IInitializeWFParams actor = ActorProxy.Create<IInitializeWFParams>(ActorId.CreateRandom(), new Uri("fabric:/Zellenfertigung_Demo/InitializeWFParamsActorService"));
                //Task<string> retval = actor.GetHelloWorldInitAsync();
                //s.Test = retval.Result;
                //Console.WriteLine($"Hello World! {retval.Result}");
            }
            else
            {
                Core.Logger.InfoFormat($"Fertigungszelle has been canceled by user");
            }
            FinishActivity();
            //FinishActivityAsSuccess(false);
        }

        private void StateToken_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {

        }
    }
}
    
