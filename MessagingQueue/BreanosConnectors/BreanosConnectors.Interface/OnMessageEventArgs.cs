//-----------------------------------------------------------------------

// <copyright file="OnMessageEventArgs.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Tuesday, October 30, 2018 1:26:47 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using System.Collections.Generic;

namespace BreanosConnectors.Interface
{
    public class OnMessageEventArgs
    {
        /// <summary>
        /// The content / payload of the message that was sent over the Messaging Queue
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// A row of keys with values (properties) of the message content that 
        /// can be filtered for in the ListenTo method of IMqConnector 
        /// and set in the properties of its Send method.
        /// Basically metadata that don't belong to the message itself but are 
        /// important for processing.
        /// </summary>
        public IDictionary<string, object> Properties { get; set; }
    }
}
