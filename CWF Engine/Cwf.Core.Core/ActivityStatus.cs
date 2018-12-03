//-----------------------------------------------------------------------

// <copyright file="ActivityStatus.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Monday, December 3, 2018 3:34:35 PM</date>
// </copyright>

//-----------------------------------------------------------------------

//-----------------------------------------------------------------------

// <copyright file="ActivityStatus.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Tuesday, October 30, 2018 2:54:51 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using CWF.Core.ExecutionGraph;

namespace CWF.Core
{
    /// <summary>
    /// Task status.
    /// </summary>
    public class ActivityStatus
    {
        /// <summary>
        /// Status.
        /// </summary>
        public Status Status { get; set; }
        /// <summary>
        /// Der Zustand.
        /// </summary>
        public object NewState { get; set; }
        /// <summary>
        /// If and While condition.
        /// </summary>
        public bool Condition { get; set; }
        /// <summary>
        /// Switch/Case value.
        /// </summary>
        public string SwitchValue { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ActivityName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int ActivityId { get; set; }       
        /// <summary>
        /// 
        /// </summary>
        public object NextParameter { get; set; }
        /// <summary>
        /// Creates a new TaskStatus. This constructor is designed for sequential tasks.
        /// </summary>
        /// <param name="status">Status.</param>
        public ActivityStatus(Activity activity, Status status)
        {
            ActivityName = activity.Name;
            ActivityId = activity.Id;            
            Status = status;
        }

        /// <summary>
        /// Creates a new TaskStatus. This constructor is designed for If/While flowchart tasks.
        /// </summary>
        /// <param name="status">Status.</param>
        /// <param name="condition">Condition value.</param>
        public ActivityStatus(Activity activity, Status status, bool condition) : this(activity, status)
        {
            Condition = condition;
        }

        /// <summary>
        /// Creates a new TaskStatus. This constructor is designed for Switch flowchart tasks.
        /// </summary>
        /// <param name="status">Status.</param>
        /// <param name="switchValue">Switch value.</param>
        public ActivityStatus(Activity activity, Status status, string switchValue) : this(activity, status)
        {
            SwitchValue = switchValue;
        }

        /// <summary>
        /// Creates a new TaskStatus. This constructor is designed for If/While and Switch flowchart tasks.
        /// </summary>
        /// <param name="status">Status.</param>
        /// <param name="condition">Condition value.</param>
        /// <param name="switchValue">Switch value.</param>
        public ActivityStatus(Activity activity, Status status, bool condition, string switchValue) : this(activity, status)
        {
            Condition = condition;
            SwitchValue = switchValue;
        }
    }
}
