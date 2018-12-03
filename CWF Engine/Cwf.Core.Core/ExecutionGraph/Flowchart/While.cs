//-----------------------------------------------------------------------

// <copyright file="While.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Monday, December 3, 2018 3:34:35 PM</date>
// </copyright>

//-----------------------------------------------------------------------

//-----------------------------------------------------------------------

// <copyright file="While.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Tuesday, October 30, 2018 2:54:51 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CWF.Core.ExecutionGraph.Flowchart
{
    /// <summary>
    /// While flowchart node.
    /// </summary>
    public class While : Node
    {
        /// <summary>
        /// While Id.
        /// </summary>
        public int WhileId { get; private set; }
        /// <summary>
        /// Nodes.
        /// </summary>
        public Node[] Nodes { get; private set; }

        /// <summary>
        /// Creates a new While flowchart node.
        /// </summary>
        /// <param name="id">Id.</param>
        /// <param name="parentId">Parent Id.</param>
        /// <param name="whileId">While Id.</param>
        /// <param name="nodes">Nodes.</param>
        public While(int id, int parentId, int whileId, IEnumerable<Node> nodes) : base(id, parentId)
        {
            WhileId = whileId;
            if (nodes != null) Nodes = nodes.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(GetType().ToString());
            sb.AppendLine("WhileId" + WhileId.ToString());
            sb.AppendLine(base.ToString());

            foreach (Node n in Nodes)
            {
                sb.AppendLine(n.ToString());
            }
            return sb.ToString();
        }
    }
}
