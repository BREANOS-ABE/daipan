//-----------------------------------------------------------------------

// <copyright file="HanoiModel.cs" company="Breanos GmbH">
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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HanoiWpf
{
    public class HanoiModel : INotifyPropertyChanged
    {
        //convention: _stack[0] is bottom; _stack[last] is top;
        //int[][] _stacks;

        public ObservableCollection<int>[] Stacks { get; private set; }

        int _stacksize;
        public event EventHandler Finished;
        public event PropertyChangedEventHandler PropertyChanged;
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public IEnumerable<int> Stack1 { get { return Stacks[0].Reverse();} }
        public IEnumerable<int> Stack2 { get { return Stacks[1].Reverse();} }
        public IEnumerable<int> Stack3 { get { return Stacks[2].Reverse(); } }

        
        public int DiskStepSize => 20;
        public int[] DiskOrigins { get; private set; }
        public int[] PoleLefts { get; private set; }

        private int diskMaxWidth;
        public int DiskStackWidth => diskMaxWidth;
        private int leftMargin = 0;
        public HanoiModel(int stacksize)
        {
            Stacks = new ObservableCollection<int>[3];
            Stacks[0] = new ObservableCollection<int>();
            Stacks[1] = new ObservableCollection<int>();
            Stacks[2] = new ObservableCollection<int>();
        }
        //step 1
        void PutSmallestTwoRightMod3()
        {
            int smallestDiskIndex = GetStackIndexWithSmallestDiskOnTop();
            int targetIndex = (smallestDiskIndex + 2) % 3;
            var disk = PopStack(smallestDiskIndex);
            RaiseStackChanged(smallestDiskIndex);
            if (!PutStack(targetIndex, disk))
            {
                throw new Exception("Whyyyyy");
            }
            RaiseStackChanged(targetIndex);
            
        }

        void RaiseStackChanged(int i)
        {
            
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"Stacks[{i}]"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"Stack{i+1}"));
            
        }
        //step 2
        void PutSecondSmallestOntoOnlyPossible()
        {
            int secondSmallestDiskIndex = GetStackIndexWithSecondSmallestDiskOnTop();
            int disk = PopStack(secondSmallestDiskIndex);
            RaiseStackChanged(secondSmallestDiskIndex);
            if (!PutStack((secondSmallestDiskIndex+1)%3,disk) && !PutStack((secondSmallestDiskIndex + 2) % 3,disk))
            {
                throw new Exception("neyyyyyy");
            }
            RaiseStackChanged((secondSmallestDiskIndex + 1) % 3);
            RaiseStackChanged((secondSmallestDiskIndex + 2) % 3);
        }

        bool isStep1 = true;

        public void RunRound()
        {
            if (IsFinished())
            {
                Finished?.Invoke(this, null);
                return;
                
            }
            if (isStep1)
            {
                PutSmallestTwoRightMod3();
                isStep1 = false;
            } else
            {
                PutSecondSmallestOntoOnlyPossible();
                isStep1 = true;
            }

        }
        public async Task RunUntilFinished(CancellationToken token)
        {

            while (!IsFinished())
            {
                if (token.IsCancellationRequested) return;
                RunRound();
                await Task.Delay(50);
            }
        }
        Task runningRounds;
        CancellationTokenSource cts;
        public void RestartCommand(int stacksize)
        {
            if (runningRounds != null)
            {
                cts.Cancel();
            }
            cts = new CancellationTokenSource();
            isStep1 = true;
            _stacksize = stacksize;
            diskMaxWidth = stacksize * DiskStepSize + 40;
            var pole1Left = diskMaxWidth / 2 + leftMargin;
            var pole2Left = 3 * (diskMaxWidth / 2) + (2 * leftMargin);
            var pole3Left = 5 * (diskMaxWidth / 2) + (3 * leftMargin);
            PoleLefts = new int[]
            {
                pole1Left,
                pole2Left,
                pole3Left
            };

            Stacks = new ObservableCollection<int>[3];
            Stacks[0] = new ObservableCollection<int>();
            Stacks[1] = new ObservableCollection<int>();
            Stacks[2] = new ObservableCollection<int>();
            DiskOrigins = new int[3];
            DiskOrigins[0] = PoleLefts[0] - (diskMaxWidth / 2) + 5;
            if (stacksize % 2 == 0)
            {
                DiskOrigins[1] = PoleLefts[2] - (diskMaxWidth / 2) + 5;
                DiskOrigins[2] = PoleLefts[1] - (diskMaxWidth / 2) + 5;
            }
            else
            {
                DiskOrigins[2] = PoleLefts[2] - (diskMaxWidth / 2) + 5;
                DiskOrigins[1] = PoleLefts[1] - (diskMaxWidth / 2) + 5;
            }
            for (int i = 0; i < _stacksize; i++)
            {
                Stacks[0].Add((_stacksize - i) * DiskStepSize);
            }
            RaiseStackChanged(0);
            RaiseStackChanged(1);
            RaiseStackChanged(2);
            runningRounds = RunUntilFinished(cts.Token);
        }
        bool IsFinished()
        {
            if (_stacksize % 2 == 0)
            {
            return Stacks[1].Count == _stacksize && Stacks[1][_stacksize - 1] != 0;

            } else
            return Stacks[2].Count == _stacksize && Stacks[2][_stacksize - 1] != 0;
        }

        int[] GetTopmostDisks()
        {
            return new int[]
            {
                GetTopmostDiskSizeOfStack(0),
                GetTopmostDiskSizeOfStack(1),
                GetTopmostDiskSizeOfStack(2)
            };
        }

        int PopStack(int stackIndex)
        {

            for (int i = _stacksize - 1; i >= 0; i--)
            {
                if (Stacks[stackIndex].Count == (i+1))
                {
                    var ret = Stacks[stackIndex][i];
                    Stacks[stackIndex].RemoveAt(i);
                    return ret;
                }
            }
            return 0;
        }

        bool PutStack(int stackIndex, int value)
        {
            if (Stacks[stackIndex].Count >= _stacksize) return false;

            for (int i = _stacksize - 1; i >= 0; i--)
            {
                if (Stacks[stackIndex].Count == 0) //at the bottom
                {
                    Stacks[stackIndex].Add(value);
                    return true;
                }
                //else if (_stacks[stackIndex][i - 1] > 0 && _stacks[stackIndex][i - 1] > value)
                else if (Stacks[stackIndex].Count == i && Stacks[stackIndex][i - 1] > value)
                {
                    Stacks[stackIndex].Add(value);
                    return true;
                }
                else if (Stacks[stackIndex].Count < i)
                {
                    continue;
                }
                else break;
            }
            return false;
        }

        int GetStackIndexWithSecondSmallestDiskOnTop()
        {
            return GetStackIndexWithNthSmallestDiskOnTop(2);
        }
        int GetStackIndexWithSmallestDiskOnTop()
        {
            return GetStackIndexWithNthSmallestDiskOnTop(1);
        }
        int GetStackIndexWithNthSmallestDiskOnTop(int n)
        {
            var topDisks = GetTopmostDisks();
            var ordered = topDisks.OrderBy(i => i).Where(i => i != 0);
            var diskSize = ordered.Skip(n-1).First();
            var diskIndex = Array.IndexOf(topDisks, diskSize);
            return diskIndex;
        }
        int GetTopmostDiskSizeOfStack(int stackIndex)
        {

            for (int i = _stacksize - 1; i >= 0; i--)
            {
                if (Stacks[stackIndex].Count == (i+1))
                {
                    return Stacks[stackIndex][i];
                }
            }
            return 0;
        }
    }
}
