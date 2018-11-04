﻿//-----------------------------------------------------------------------

// <copyright file="Case.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Monday, November 5, 2018 12:22:54 AM</date>
// </copyright>

//-----------------------------------------------------------------------


using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CWF.Core.ExecutionGraph.Flowchart
{
    /// <summary>
    /// Case.
    /// </summary>
    public class Case
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("Case:");
            sb.AppendLine("Value:");
            sb.AppendLine(Value);
            foreach (Node n in Nodes)
            {
                sb.AppendLine(n.ToString());
            }          
            return sb.ToString();
        }
        /// <summary>
        /// Case value.
        /// </summary>
        public string Value { get; private set; }
        /// <summary>
        /// Case nodes.
        /// </summary>
        public Node[] Nodes { get; private set; }

        /// <summary>
        /// Creates a new case.
        /// </summary>
        /// <param name="val">Case value.</param>
        /// <param name="nodes">Case nodes.</param>
        public Case(string val, IEnumerable<Node> nodes)
        {
            Value = val;
            if (nodes != null) Nodes = nodes.ToArray();
        }       
    }
}