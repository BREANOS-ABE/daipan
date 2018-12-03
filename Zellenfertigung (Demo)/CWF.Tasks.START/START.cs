//-----------------------------------------------------------------------

// <copyright file="START.cs" company="Breanos GmbH">
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

namespace CWF.Tasks.START
{
  public class START : CWF.Core.StatefulActivity<FertigungszelleWorkflowState>
  {
    private static NLog.Logger logger = LogManager.GetCurrentClassLogger();
    public START(ActivityMemento activityMememnto) : base(activityMememnto)
    {
    }

    public override void Run(System.Threading.CancellationToken token, object state, object parameterDto = null)
    {
      if (!token.IsCancellationRequested)
      {
        var s = state as FertigungszelleWorkflowState;
        StateToken = s;

        StateToken.PropertyChanged += StateToken_PropertyChanged;

        Random rnd = new Random(Guid.NewGuid().GetHashCode());

        StateToken.Activityerror = false;
        StateToken.Machineerror = false;
        StateToken.Machinenumber = 1;

        if (StateToken.Workpiececount == 0)
        {
          var parameters = parameterDto as WorkPieceCount;
          StateToken.Workpiececount = parameters.WorkPieceCountNumber;
        }

        if (StateToken.RandomNr1 == 0)
        {
          StateToken.RandomNr1 = rnd.Next(0, StateToken.Workpiececount + 1);
          StateToken.RandomNr2 = rnd.Next(1, 4);
          StateToken.RandomNr3 = rnd.Next(0, StateToken.Workpiececount + 1);
        }

        Console.WriteLine("START: ");

        //System.Threading.Thread.Sleep(1500);

        //string serialized = SerializationHelper.Pack(s);
        //Core.Logger.InfoFormat($"PostHanoiStateToActiveMQActivity sending...{serialized}");
        //Workflow.Engine.Amqc.SendAsync(serialized, Workflow.Engine.TopicName, typeof(string).AssemblyQualifiedName);

        //FinishActivityAsSuccess(true);
        //return;
      }
      else
      {
        Core.Logger.InfoFormat($"SetupHanoiGameActivity has been canceled by user");
      }
      FinishActivity();
      //FinishActivityAsSuccess(false);
    }

    private void StateToken_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {

    }
  }
}
