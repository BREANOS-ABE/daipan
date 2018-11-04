//-----------------------------------------------------------------------

// <copyright file="Switch.cs" company="Breanos GmbH">
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
    /// Switch flowchart node.
    /// </summary>
    public class Switch : Node
    {
        /// <summary>
        /// Switch id.
        /// </summary>
        public int SwitchId { get; private set; }
        /// <summary>
        /// Cases.
        /// </summary>
        public Case[] Cases { get; private set; }
        /// <summary>
        /// Default case.
        /// </summary>
        public Node[] Default { get; private set; }

        /// <summary>
        /// Creates a new Switch flowchart node.
        /// </summary>
        /// <param name="id">Id.</param>
        /// <param name="parentId">Parent Id.</param>
        /// <param name="switchId">Switch Id.</param>
        /// <param name="cases">Cases.</param>
        /// <param name="default">Default case.</param>
        public Switch(int id, int parentId, int switchId, IEnumerable<Case> cases, IEnumerable<Node> @default) : base(id, parentId)
        {
            SwitchId = switchId;
            if (cases != null) Cases = cases.ToArray();
            if (@default != null) Default = @default.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(GetType().ToString());
            sb.AppendLine("SwitchId" + SwitchId.ToString());
            sb.AppendLine(base.ToString());
            sb.AppendLine(Cases.ToString());
            sb.AppendLine(Default.ToString());
            return sb.ToString();
        }
    }
}
