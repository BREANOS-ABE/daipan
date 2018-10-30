//-----------------------------------------------------------------------

// <copyright file="ConnectorConfiguration.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Tuesday, October 30, 2018 1:26:47 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;

namespace BreanosConnectors
{
    namespace OpcUaConnector
    {
        public class ConnectorConfiguration
        {
            public int PublishingInterval { get; set; }
            public int KeepAliveInterval { get; set; }
            public int ReconnectAttemptInterval { get; set; }
            public int SessionTimeout { get; set; }
            public int OperationTimeout { get; set; }
            public static ConnectorConfiguration DefaultConfiguration()
            {
                return new ConnectorConfiguration()
                {
                    KeepAliveInterval = 5000,
                    PublishingInterval = 1000,
                    ReconnectAttemptInterval = 10000,
                    SessionTimeout = 3600000,
                    OperationTimeout = 10000
                };
            }
        }
    }
}
