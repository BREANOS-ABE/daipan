//-----------------------------------------------------------------------

// <copyright file="BIKSEnvironment.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Monday, December 3, 2018 3:34:35 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIKSClassLibrary
{
    /// <summary>
    /// Test helper class for Workflow activities. Not used in Blackboard implementation.
    /// </summary>
    public class BIKSEnvironment
    {
        /// <summary>
        /// Herein are all our messages queued, which are received from ServiceBus. Also helper class from 
        /// Workflow activities implementation.
        /// </summary>
        public ConcurrentQueue<object> ItemList { get; set; } = new ConcurrentQueue<object>();
        
        /// <summary>
        /// ctor of the BIKSEnvironment test helper class 
        /// </summary>
        public BIKSEnvironment()
        {

        }
    }
}
