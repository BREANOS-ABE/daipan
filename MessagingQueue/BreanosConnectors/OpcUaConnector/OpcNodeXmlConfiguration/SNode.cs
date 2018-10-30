//-----------------------------------------------------------------------

// <copyright file="SNode.cs" company="Breanos GmbH">
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
        /// Represents a singular node
        /// </summary>
        [XmlRoot(ElementName = "SNode")]
        public class SNode : Node
        {
            /// <summary>
            /// The path of this node
            /// </summary>
            [XmlAttribute]
            public string Path { get; set; }
            public override IEnumerable<string> GetPaths()
            {
                List<string> e = new List<string>();
                if (IsLeaf) e.Add(Path);
                else
                {
                    foreach (var child in Children)
                    {
                        e.AddRange(child.GetPaths());
                    }
                    e = e.Select(p => $"{Path}{(Separator??".")}{p}").ToList();
                }
                return e;
            }

            public override IEnumerable<SNode> GetFlattenedStructure(string prePath, NodeConfiguration parentNodeConfiguration)
            {
                List<SNode> e = new List<SNode>();
                var newPath = string.IsNullOrEmpty(prePath) ? Path : prePath + (Separator??".") + Path;
                if (IsLeaf) e.Add(new SNode() { Path = newPath, Name=this.Name, Config = this.Config != null ? this.Config : parentNodeConfiguration, DeadbandType = this.DeadbandType, DeadbandValue = this.DeadbandValue, Separator = this.Separator });
                else
                {
                    foreach (var child in Children)
                    {
                        e.AddRange(child.GetFlattenedStructure(newPath, Config != null ? Config : parentNodeConfiguration));
                    }
                }
                return e;
            }
        }
    }
}
