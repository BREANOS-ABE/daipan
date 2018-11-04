﻿//-----------------------------------------------------------------------

// <copyright file="If.cs" company="Breanos GmbH">
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
    /// If flowchart node.
    /// </summary>
    public class If : Node
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
           
            sb.AppendLine("--------START-------" + GetType().ToString() + "--------START-------");
            sb.AppendLine("IfActivityId: " + Id.ToString());
            sb.AppendLine(base.ToString());

            sb.AppendLine("DoNodes");
            foreach (Node n in DoNodes)
            {
                sb.AppendLine(n.ToString());
            }

            sb.AppendLine("ElseNodes");
            foreach (Node n in ElseNodes)
            {
                sb.AppendLine(n.ToString());
            }
            sb.AppendLine("--------STOP-------" + GetType().ToString() + "--------STOP-------");

            return sb.ToString();
        }
        
        /// <summary>
        /// Do Nodes.
        /// </summary>
        public Node[] DoNodes { get; private set; }

        /// <summary>
        /// Else nodes.
        /// </summary>
        public Node[] ElseNodes { get; private set; }

        /// <summary>
        /// Creates a new If flowchart node.
        /// </summary>
        /// <param name="id">Id.</param>
        /// <param name="parentId">Parent Id.</param>
        /// <param name="ifId">If Id.</param>
        /// <param name="doNodes">Do nodes.</param>
        /// <param name="elseNodes">Else nodes.</param>
        public If(int id, int parentId, IEnumerable<Node> doNodes, IEnumerable<Node> elseNodes)
            :base(id, parentId)
        {           
            if (doNodes != null) DoNodes = doNodes.ToArray();
            if (elseNodes != null) ElseNodes = elseNodes.ToArray();
        }
    }
}