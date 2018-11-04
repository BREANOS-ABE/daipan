//-----------------------------------------------------------------------

// <copyright file="GraphEvent.cs" company="Breanos GmbH">
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

namespace CWF.Core.ExecutionGraph
{
    /// <summary>
    /// Graph event.
    /// </summary>
    public class GraphEvent
    {
        /// <summary>
        /// Nodes.
        /// </summary>
        public Node[] Nodes { get; private set; }

        /// <summary>
        /// Creates a new graph event.
        /// </summary>
        /// <param name="nodes">Nodes.</param>
        public GraphEvent(IEnumerable<Node> nodes)
        {
            if (nodes != null) Nodes = nodes.ToArray();
        }
    }
}
