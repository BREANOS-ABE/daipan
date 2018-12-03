//-----------------------------------------------------------------------

// <copyright file="OpcConfiguration.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Monday, December 3, 2018 3:34:35 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BreanosConnectors
{
    namespace OpcUaConnector
    {
        [XmlRoot]
        [XmlInclude(typeof(SubscriptionConfiguration))]
        [XmlInclude(typeof(SNode))]
        [XmlInclude(typeof(MNode))]
        [XmlInclude(typeof(VNode))]
        public class OpcConfiguration
        {
            [XmlArray]
            [XmlArrayItem(typeof(SubscriptionConfiguration))]
            public SubscriptionConfiguration[] Subscriptions { get; set; }


        }


        [XmlRoot]
        [XmlInclude(typeof(SNode))]
        [XmlInclude(typeof(MNode))]
        [XmlInclude(typeof(VNode))]
        public class SubscriptionConfiguration
        {
            [XmlElement]
            public int PublishingInterval { get; set; }
            [XmlArray]
            [XmlArrayItem(Type = typeof(SNode))]
            [XmlArrayItem(Type = typeof(MNode))]
            [XmlArrayItem(Type = typeof(VNode))]
            public Node[] NodeCollection { get; set; }
        }
    }
}
