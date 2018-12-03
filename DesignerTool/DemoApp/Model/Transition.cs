//-----------------------------------------------------------------------

// <copyright file="Transition.cs" company="Breanos GmbH">
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
using System.Reflection;
using System.Text;

namespace DesignerTool.Model
{
    public class Transition<T> : Transition
    {
        private Func<T, bool> _checkMethod;
        public Activity<T> ActualSourceActivity { get; set; }
        public Activity<T> ActualTargetActivity { get; set; }
        public override Activity SourceActivity
        {
            get
            {
                return ActualSourceActivity;
            }
            set
            {
                if (value is Activity<T>) ActualSourceActivity = (Activity<T>)value;
            }
        }
        public override Activity TargetActivity 
        {
            get
            {
                return ActualTargetActivity;
            }
            set
            {
                if (value is Activity<T>) ActualTargetActivity = (Activity<T>)value;
            }
        }
        private string SourceActivityName => ((ActualSourceActivity != null && ActualSourceActivity.Definition != null) ? (ActualSourceActivity.Definition.Name) : ("null"));
        private string TargetActivityName => ((ActualTargetActivity != null && ActualTargetActivity.Definition != null) ? (ActualTargetActivity.Definition.Name) : ("null"));
        private string ConditionText => ((Definition != null) ? (Definition.ConditionText ?? "true") : "null");
        public Transition(TransitionDefinition definition) : base(definition)
        {

        }
        public override bool Check(object o)
        {
            if (o is T)
            {
                var t = (T)o;
                return (_checkMethod != null) ? (_checkMethod(t)) : true;
            }
            else
            {
                throw new Exception($"An attempt was made to call Check on Transition {this}");
            }
        }

        public override void SetCheckMethod(Type stateType, MethodInfo info)
        {
            var genericDelegateType = typeof(Func<,>).MakeGenericType(stateType, typeof(bool));
            _checkMethod = (Func<T, bool>)Delegate.CreateDelegate(genericDelegateType, info);
        }
        public override string ToString()
        {
            return $"T({ConditionText})_{{{SourceActivityName}->{TargetActivityName}}}";
        }

    }
    public abstract class Transition
    {
        public const string TransitionConditionMethodNamePrefix = "_Check_T";
        public virtual Activity SourceActivity { get; set; }
        public virtual Activity TargetActivity { get; set; }
        public  TransitionDefinition Definition { get; set; }
        public Transition(TransitionDefinition definition)
        {
            Definition = definition;
        }
        public abstract void SetCheckMethod(Type stateType, System.Reflection.MethodInfo info);
        public abstract bool Check(object o);
    }
}
