//-----------------------------------------------------------------------

// <copyright file="OpcXmlNodeLoader.cs" company="Breanos GmbH">
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
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace BreanosConnectors
{
    namespace OpcUaConnector
    {
        public static class OpcXmlNodeLoader
        {
            public static OpcConfiguration LoadNodes(string filename)
            {
                XmlSerializer xs = new XmlSerializer(typeof(OpcConfiguration), new[] { typeof(SubscriptionConfiguration), typeof(SNode), typeof(MNode), typeof(VNode) });
                using (var fs = new FileStream(filename, FileMode.Open))
                {
                    var root = xs.Deserialize(fs) as OpcConfiguration;
                    foreach (var subDesc in root.Subscriptions)
                    {
                        foreach (var nc in subDesc.NodeCollection)
                        {
                            nc.InitializeConfig();
                        }
                    }

                    return root;
                }
            }
        }
    }
}
