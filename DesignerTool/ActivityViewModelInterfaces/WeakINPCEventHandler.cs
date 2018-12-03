//-----------------------------------------------------------------------

// <copyright file="WeakINPCEventHandler.cs" company="Breanos GmbH">
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
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ActivityViewModelInterfaces
{
    //[DebuggerNonUserCode]
    public sealed class WeakINPCEventHandler 
    {
        private readonly WeakReference _targetReference;
        private readonly MethodInfo _method;

        public WeakINPCEventHandler(PropertyChangedEventHandler callback)
        {
            _method = callback.Method;
            _targetReference = new WeakReference(callback.Target, true);
        }

        //[DebuggerNonUserCode]
        public void Handler(object sender, PropertyChangedEventArgs e)
        {
            var target = _targetReference.Target;
            if (target != null)
            {
                var callback = (Action<object, PropertyChangedEventArgs>)Delegate.CreateDelegate(typeof(Action<object, PropertyChangedEventArgs>), target, _method, true);
                if (callback != null)
                {
                    callback(sender, e);
                }
            }
        }
    }
}
