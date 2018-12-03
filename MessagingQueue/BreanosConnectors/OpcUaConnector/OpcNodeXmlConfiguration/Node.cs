//-----------------------------------------------------------------------

// <copyright file="Node.cs" company="Breanos GmbH">
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
        /// <summary>
        /// Node base class. 
        /// A node is part of a tree-structure of nodes.
        /// Instantiable versions of Node are 
        /// SNode (singular nodes), 
        /// MNode (multi-nodes) and 
        /// VNodes (vario nodes)
        /// </summary>
        [XmlRoot]
        [XmlInclude(typeof(SNode))]
        [XmlInclude(typeof(MNode))]
        [XmlInclude(typeof(VNode))]
        public abstract class Node
        {
            /// <summary>
            /// This node's child nodes as enumerated under <Children>...</Children>
            /// </summary>
            [XmlArray]
            [XmlArrayItem(Type = typeof(SNode))]
            [XmlArrayItem(Type = typeof(MNode))]
            [XmlArrayItem(Type = typeof(VNode))]
            public Node[] Children { get; set; }
            /// <summary>
            /// Checks if this is a leaf node, i.e. if this node doesn't have any children
            /// </summary>
            public bool IsLeaf => (Children == null || Children.Length <= 0);
            /// <summary>
            /// Used for GetPaths(), this will separate this node's relative path from it's child nodes' relative paths
            /// </summary>
            [XmlAttribute]
            public string Separator { get; set; }
            [XmlAttribute]
            public string DeadbandType { get; set; }
            [XmlAttribute]
            public string DeadbandValue { get; set; }
            [XmlAttribute]
            public string Name { get; set; }
            [XmlIgnore]
            public NodeConfiguration Config { get; set; }

            public Node()
            {
                //if (Separator == null) Separator = ".";
            }

            public void InitializeConfig()
            {
                if (!string.IsNullOrEmpty(DeadbandType) && !string.IsNullOrEmpty(DeadbandValue))
                {
                    Config = new NodeConfiguration();
                    int t;
                    double v;
                    if (!string.IsNullOrEmpty(DeadbandType) && double.TryParse(DeadbandValue, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out v))
                    {
                        switch (DeadbandType)
                        {

                            case "Absolute":
                                t = 1;
                                break;
                            case "Percent":
                                t = 2;
                                break;
                            case "None":
                            default:
                                t = 0;
                                break;
                        }


                        Config.DeadbandSettings = new DeadbandSettings()
                        {
                            DeadbandType = t,
                            DeadbandValue = v
                        };
                    }
                    else
                    {
                        Config.DeadbandSettings = new DeadbandSettings()
                        {
                            DeadbandValue = 0,
                            DeadbandType = 0
                        };
                    }
                }

                if (!IsLeaf)
                {
                    foreach (var child in Children)
                    {
                        child.InitializeConfig();
                    }
                }
            }
            public abstract IEnumerable<string> GetPaths();

            public abstract IEnumerable<SNode> GetFlattenedStructure(string prePath = "", NodeConfiguration parentNodeConfiguration = null);
        }

        public class NodeConfiguration
        {
            public DeadbandSettings DeadbandSettings;
        }

        public class DeadbandSettings
        {
            public int DeadbandType;
            public double DeadbandValue;
        }
    }
}
