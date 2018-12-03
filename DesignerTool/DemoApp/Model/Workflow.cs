//-----------------------------------------------------------------------

// <copyright file="Workflow.cs" company="Breanos GmbH">
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

namespace DesignerTool.Model
{
    public class Workflow<T> : Workflow
    {
        public T ActualState { get; set; }
        public override object State { get
            {
                return ActualState;
            }
            set
            {
                if (value is T) ActualState = (T)value;
            }
        }
        public Activity<T>[] ActualActivities { get; set; }
        public override Activity[] Activities
        {
            get
            {
                return ActualActivities;
            }
            set
            {
                ActualActivities = new List<Activity<T>>(value.Select(t => t as Activity<T>)).ToArray();
            }
        }
        public Transition<T>[] ActualTransitions { get; set; }
        public override Transition[] Transitions
        {
            get
            {
                return ActualTransitions;
            }
            set
            {
                ActualTransitions = new List<Transition<T>>(value.Select(t => t as Transition<T>)).ToArray();
            }
        }
        public override Transition InitialTransition { get { return (ActualTransitions != null) ? (ActualTransitions.Where(t => t.ActualSourceActivity == null && t.ActualTargetActivity != null).FirstOrDefault()):null; } }
    }
    public abstract class Workflow
    {
        public string Name { get; set; }
        public abstract object State { get; set; }
        public Setting[] Settings { get; set; }
        public abstract Activity[] Activities { get; set; }
        public abstract Transition[] Transitions { get; set; }
        public abstract Transition InitialTransition { get; }

    }
}
