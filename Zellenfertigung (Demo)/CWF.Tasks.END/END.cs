//-----------------------------------------------------------------------

// <copyright file="END.cs" company="Breanos GmbH">
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

namespace CWF.Tasks.END
{
  public class END : CWF.Core.StatefulActivity<FertigungszelleWorkflowState>
  {
    private static NLog.Logger logger = LogManager.GetCurrentClassLogger();
    public END(ActivityMemento activityMememnto) : base(activityMememnto)
    {
    }

    public override void Run(System.Threading.CancellationToken token, object state, object parameterDto = null)
    {
      if (!token.IsCancellationRequested)
      {
        Console.WriteLine("END: ");
        Console.WriteLine("");

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
