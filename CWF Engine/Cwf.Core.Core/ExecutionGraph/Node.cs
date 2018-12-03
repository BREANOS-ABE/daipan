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

//-----------------------------------------------------------------------

// <copyright file="Node.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Tuesday, October 30, 2018 2:54:51 PM</date>
// </copyright>

//-----------------------------------------------------------------------

namespace CWF.Core.ExecutionGraph
{
    /// <summary>
    /// Node.
    /// </summary>
    public class Node
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return GetType().Name + " |Activity Id: " + Id.ToString() + " |ParentActivityId:" + ParentId.ToString();
        }
        /// <summary>
        /// Node Id.
        /// </summary>
        public int Id { get; private set; }
        /// <summary>
        /// Node parent Id.
        /// </summary>
        public int ParentId { get; private set; }

        /// <summary>
        /// Creates a new node.
        /// </summary>
        /// <param name="id">Node id.</param>
        /// <param name="parentId">Node parent id.</param>
        public Node(int id, int parentId)
        {
            Id = id;
            ParentId = parentId;
        }       
    }
}
