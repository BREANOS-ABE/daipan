//-----------------------------------------------------------------------

// <copyright file="HanoiWorkflowState.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Monday, November 5, 2018 12:22:54 AM</date>
// </copyright>

//-----------------------------------------------------------------------


using CWF.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace HanoiLibrary
{
    public class HanoiWorkflowState : GenericState
    {
        public HanoiWorkflowState()
        {

        }

        protected override void SomePropertyChanged(string prop)
        {
            base.SomePropertyChanged(prop);
        }

        private void OnChanged(string prop)
        {
            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
            SomePropertyChanged(prop);
        }
        private int _diskBaseWidth;

        public int DiskBaseWidth
        {
            get { return _diskBaseWidth; }
            set
            {
                _diskBaseWidth = value; OnChanged(nameof(DiskBaseWidth));
            }
        }

        private int _round;

        public int Round
        {
            get { return _round; }
            set { _round = value; OnChanged(nameof(Round)); }
        }

        private int _numberDisks;

        public int NumberDisks
        {
            get { return _numberDisks; }
            set { _numberDisks = value; OnChanged(nameof(NumberDisks)); }
        }
        private List<HanoiDisk> _stack1;

        public List<HanoiDisk> Stack1
        {
            get { return _stack1; }
            set { _stack1 = value;/* _stack1.CollectionChanged += Stack1_CollectionChanged;*/ OnChanged(nameof(Stack1)); }
        }

        private void Stack1_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnChanged(nameof(Stack1));
        }


        private List<HanoiDisk> _stack2;

        public List<HanoiDisk> Stack2
        {
            get { return _stack2; }
            set { _stack2 = value; /*_stack2.CollectionChanged += Stack2_CollectionChanged;*/ OnChanged(nameof(Stack2)); }
        }

        private void Stack2_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnChanged(nameof(Stack1));
        }



        private List<HanoiDisk> _stack3;

        public List<HanoiDisk> Stack3
        {
            get { return _stack3; }
            set { _stack3 = value; /*_stack3.CollectionChanged += Stack3_CollectionChanged;*/ OnChanged(nameof(Stack3)); }
        }

        private void Stack3_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnChanged(nameof(Stack1));
        }

    }
}
