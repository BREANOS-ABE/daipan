//-----------------------------------------------------------------------

// <copyright file="FertigungszelleWorkflowState.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Monday, December 3, 2018 3:34:35 PM</date>
// </copyright>

//-----------------------------------------------------------------------

//-----------------------------------------------------------------------

// <copyright file="HanoiWorkflowState.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Tuesday, October 30, 2018 2:54:51 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using System.ComponentModel;

namespace FertigungszelleLibaryStandard
{
  public class FertigungszelleWorkflowState : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;
    public FertigungszelleWorkflowState()
    {
    }

    private void OnChanged(string prop)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    private int _workpieceid;

    public int Workpieceid
    {
      get { return _workpieceid; }
      set
      {
        _workpieceid = value; OnChanged(nameof(_workpieceid));
      }
    }

    private int _workpiececount;

    public int Workpiececount
    {
      get { return _workpiececount; }
      set
      {
        _workpiececount = value; OnChanged(nameof(_workpiececount));
      }
    }

    private bool _activityerror;

    public bool Activityerror
    {
      get { return _activityerror; }
      set
      {
        _activityerror = value; OnChanged(nameof(_activityerror));
      }
    }

    private bool _machineerror;

    public bool Machineerror
    {
      get { return _machineerror; }
      set
      {
        _machineerror = value; OnChanged(nameof(_machineerror));
      }
    }

    public bool IsFinished => (Machineerror == false) && (Activityerror == false);

    public bool IsError => (Activityerror == true) || (Machineerror == true);

    public bool IsRepeat => (Workpiececount > 0) && (Machineerror == false);


    private int _machinenumber;

    public int Machinenumber
    {
      get { return _machinenumber; }
      set
      {
        _machinenumber = value; OnChanged(nameof(_machinenumber));
      }
    }

    private int _randomNr1;

    public int RandomNr1
    {
      get { return _randomNr1; }
      set
      {
        _randomNr1 = value; OnChanged(nameof(_randomNr1));
      }
    }

    private int _randomNr2;

    public int RandomNr2
    {
      get { return _randomNr2; }
      set
      {
        _randomNr2 = value; OnChanged(nameof(_randomNr2));
      }
    }

    private int _randomNr3;

    public int RandomNr3
    {
      get { return _randomNr3; }
      set
      {
        _randomNr3 = value; OnChanged(nameof(_randomNr3));
      }
    }
        private string _test;
        public string Test
        {
            get { return _test; }
            set
            {
                _test = value; OnChanged(nameof(_test));
            }
        }
  }
}
