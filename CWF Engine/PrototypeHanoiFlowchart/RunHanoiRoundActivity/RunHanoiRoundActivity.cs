//-----------------------------------------------------------------------

// <copyright file="RunHanoiRoundActivity.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Monday, November 5, 2018 12:22:54 AM</date>
// </copyright>

//-----------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CWF.Core;
using CWF.Core.ExecutionGraph;
using HanoiLibrary;
using static CWF.Core.Parser;

namespace CWF.Tasks.RunHanoiRoundActivity
{
    public class RunHanoiRoundActivity : CWF.Core.StatefulActivity<HanoiLibrary.HanoiWorkflowState>
    {
        public RunHanoiRoundActivity(ActivityMemento activityMemento) : base(activityMemento)
        {
        }
        public HanoiLibrary.HanoiWorkflowState Model { get; set; }
        public override void Run(System.Threading.CancellationToken token, object state, object parameterDto = null)
        {
            if (!token.IsCancellationRequested)
            {
                var hws = state as HanoiLibrary.HanoiWorkflowState;
                if (hws != null)
                {
                    Model = hws;
                    hws.Round++;
                    var stacks = GetStacksFromModel();
                    if ((hws.Round % 2) != 0)
                        PutSmallestTwoRightMod3(stacks);
                    else
                        PutSecondSmallestOntoOnlyPossible(stacks);
                    PutStacksOntoModel(stacks);
                    //System.Threading.Tasks.Task.Delay(10).Wait();
                    FinishActivityAsSuccess(true);
                    return;
                }
            }
            else
            {
                Core.Logger.InfoFormat($"RunHanoiRoundActivity has been canceled by user");
            }
            FinishActivityAsError(false);            
        }
        private List<HanoiDisk>[] GetStacksFromModel()
        {
            var stacks = new List<HanoiDisk>[]
            {
                Model.Stack1.AsEnumerable().Reverse().ToList(),
                Model.Stack2.AsEnumerable().Reverse().ToList(),
                Model.Stack3.AsEnumerable().Reverse().ToList()
            };
            return stacks;
        }
        private void PutStacksOntoModel(List<HanoiDisk>[] stacks)
        {
            stacks[0].Reverse();
            stacks[1].Reverse();
            stacks[2].Reverse();
            Model.Stack1 = new List<HanoiDisk>(stacks[0]);
            Model.Stack2 = new List<HanoiDisk>(stacks[1]);
            Model.Stack3 = new List<HanoiDisk>(stacks[2]);
        }
        void PutSmallestTwoRightMod3(List<HanoiDisk>[] stacks)
        {
            int smallestDiskIndex = GetStackIndexWithSmallestDiskOnTop(stacks);
            int targetIndex = (smallestDiskIndex + 2) % 3;
            var disk = PopStack(smallestDiskIndex, stacks);
            //RaiseStackChanged(smallestDiskIndex);
            if (!PutStack(targetIndex, disk, stacks))
            {
                throw new Exception("Whyyyyy");
            }
            //RaiseStackChanged(targetIndex);

        }
        void PutSecondSmallestOntoOnlyPossible(List<HanoiDisk>[] stacks)
        {
            int secondSmallestDiskIndex = GetStackIndexWithSecondSmallestDiskOnTop(stacks);
            var disk = PopStack(secondSmallestDiskIndex, stacks);
            //RaiseStackChanged(secondSmallestDiskIndex);
            if (!PutStack((secondSmallestDiskIndex + 1) % 3, disk, stacks) && !PutStack((secondSmallestDiskIndex + 2) % 3, disk, stacks))
            {
                throw new Exception("neyyyyyy");
            }
            //RaiseStackChanged((secondSmallestDiskIndex + 1) % 3);
            //RaiseStackChanged((secondSmallestDiskIndex + 2) % 3);
        }

        HanoiDisk PopStack(int stackIndex, List<HanoiDisk>[] stacks)
        {

            for (int i = Model.NumberDisks - 1; i >= 0; i--)
            {
                if (stacks[stackIndex].Count == (i + 1))
                {
                    var ret = stacks[stackIndex][i];
                    stacks[stackIndex].RemoveAt(i);
                    return ret;
                }
            }
            return null;
        }

        bool PutStack(int stackIndex, HanoiDisk putDisk, List<HanoiDisk>[] stacks)
        {
            if (stacks[stackIndex].Count >= Model.NumberDisks) return false;

            for (int i = Model.NumberDisks - 1; i >= 0; i--)
            {
                if (stacks[stackIndex].Count == 0) //at the bottom
                {
                    stacks[stackIndex].Add(putDisk);
                    return true;
                }
                //else if (_stacks[stackIndex][i - 1] > 0 && _stacks[stackIndex][i - 1] > value)
                else if (stacks[stackIndex].Count == i && stacks[stackIndex][i - 1].DiskSize > putDisk.DiskSize)
                {
                    stacks[stackIndex].Add(putDisk);
                    return true;
                }
                else if (stacks[stackIndex].Count < i)
                {
                    continue;
                }
                else break;
            }
            return false;
        }
        int GetStackIndexWithSecondSmallestDiskOnTop(List<HanoiDisk>[] stacks)
        {
            return GetStackIndexWithNthSmallestDiskOnTop(2, stacks);
        }
        int GetStackIndexWithSmallestDiskOnTop(List<HanoiDisk>[] stacks)
        {
            return GetStackIndexWithNthSmallestDiskOnTop(1, stacks);
        }
        int GetStackIndexWithNthSmallestDiskOnTop(int n, List<HanoiDisk>[] stacks)
        {
            var topDisks = GetTopmostDisks(stacks);
            var ordered = topDisks.OrderBy(i => { if (i != null) return i.DiskSize; else return 0; }).Where(i => i != null);
            var diskSize = ordered.Skip(n - 1).First();
            var diskIndex = Array.IndexOf(topDisks, diskSize);
            return diskIndex;
        }
        HanoiDisk[] GetTopmostDisks(List<HanoiDisk>[] stacks)
        {
            return new HanoiDisk[]
            {
                GetTopmostDiskSizeOfStack(0, stacks),
                GetTopmostDiskSizeOfStack(1, stacks),
                GetTopmostDiskSizeOfStack(2,stacks)
            };
        }
        HanoiDisk GetTopmostDiskSizeOfStack(int stackIndex, List<HanoiDisk>[] stacks)
        {

            for (int i = Model.NumberDisks - 1; i >= 0; i--)
            {
                if (stacks[stackIndex].Count == (i + 1))
                {
                    return stacks[stackIndex][i];
                }
            }
            return null;
        }
    }
}
