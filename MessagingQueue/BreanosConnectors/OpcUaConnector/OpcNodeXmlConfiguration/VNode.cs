//-----------------------------------------------------------------------

// <copyright file="VNode.cs" company="Breanos GmbH">
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BreanosConnectors
{
    namespace OpcUaConnector
    {
        /// <summary>
        /// Represents a group of nodes with differing Paths but same set of children
        /// </summary>
        [XmlRoot(ElementName = "VNode")]
        public class VNode : Node
        {
            [XmlArray]
            [XmlArrayItem(ElementName = "Path")]
            public string[] Paths { get; set; }

            private IEnumerable<string> GetSNodeRepresenatation()
            {
                var l = new List<string>(Paths);
                return l;
            }

            public override IEnumerable<string> GetPaths()
            {
                var a = new List<string>();
                var b = GetSNodeRepresenatation();
                if (IsLeaf) a.AddRange(GetSNodeRepresenatation());
                else
                {
                    var childPaths = new List<string>();
                    foreach (var child in Children)
                    {
                        childPaths.AddRange(child.GetPaths());
                    }
                    a = (from self in b
                         from other in childPaths
                         select ($"{self}{Separator??"."}{other}")).ToList();
                }
                return a;
            }

            public override IEnumerable<SNode> GetFlattenedStructure(string prePath, NodeConfiguration parentNodeConfiguration)
            {
                IEnumerable<string> localPrepaths;
                if (string.IsNullOrEmpty(prePath))
                {
                    localPrepaths = GetSNodeRepresenatation();
                }
                else
                {
                    localPrepaths = GetSNodeRepresenatation().Select(l => prePath + (Separator??".") + l);
                }
                var a = new List<SNode>();
                if (IsLeaf)
                {
                    a.AddRange(localPrepaths.Select(x => new SNode() { DeadbandType = DeadbandType, DeadbandValue = DeadbandValue, Path = x, Name=x, Separator = Separator, Config = this.Config != null ? this.Config : parentNodeConfiguration }));
                }
                else
                {
                    foreach (var lpp in localPrepaths)
                    {
                        foreach (var child in Children)
                        {
                            a.AddRange(child.GetFlattenedStructure(lpp, this.Config != null ? this.Config : parentNodeConfiguration));
                        }
                    }
                }
                return a;
            }

        }
    }
}
