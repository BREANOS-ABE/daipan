//-----------------------------------------------------------------------

// <copyright file="WorkFlowStateToken.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Monday, December 3, 2018 3:34:35 PM</date>
// </copyright>

//-----------------------------------------------------------------------

//-----------------------------------------------------------------------

// <copyright file="WorkFlowStateToken.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Tuesday, October 30, 2018 2:54:51 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using System;
using System.ComponentModel;

namespace DemoStateMachine
{
    public class DemoWorkflowState : INotifyPropertyChanged
    {
        private string _amqConnectionString;

        public string AmqConnectionString
        {
            get { return _amqConnectionString; }
            set { _amqConnectionString = value; OnChanged(nameof(AmqConnectionString)); }
        }

        private string _amqUser;

        public string AmqUser
        {
            get { return _amqUser; }
            set { _amqUser = value; OnChanged(nameof(AmqUser)); }
        }

        private string _amqPassword;

        public string AmqPassword
        {
            get { return _amqPassword; }
            set { _amqPassword = value; OnChanged(nameof(AmqPassword)); }
        }

        private bool _isConnected;

        public bool IsConnected
        {
            get { return _isConnected; }
            set { _isConnected = value; OnChanged(nameof(IsConnected)); }
        }

        private string _nextActivity;

        public string NextActivity
        {
            get { return _nextActivity; }
            set { _nextActivity = value; OnChanged(nameof(NextActivity)); }
        }

        public DemoWorkflowState()
        {
            InitializeStateTokenToDefault();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        private void InitializeStateTokenToDefault()
        {
            AmqConnectionString = "";
            AmqUser = "";
            AmqPassword = "";
            IsConnected = false;
            NextActivity = "";
        }


        public override string ToString()
        {
            return $"{nameof(DemoWorkflowState)}: {{{nameof(AmqConnectionString)}={AmqConnectionString}; {nameof(AmqUser)}={AmqUser}; {nameof(AmqPassword)}={AmqPassword}; {nameof(IsConnected)}={IsConnected}; {nameof(NextActivity)}={NextActivity}; }}";
        }
    }
}
