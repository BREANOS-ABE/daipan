//-----------------------------------------------------------------------

// <copyright file="PostHanoiStateActivity.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Monday, December 3, 2018 3:34:35 PM</date>
// </copyright>

//-----------------------------------------------------------------------

//-----------------------------------------------------------------------

// <copyright file="PostHanoiStateActivity.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Tuesday, October 30, 2018 2:54:51 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CWF.Core;
using CWF.Core.ExecutionGraph;
using static CWF.Core.Parser;

namespace CWF.Tasks.PostHanoiStateActivity
{
    public class PostHanoiStateActivity : CWF.Core.StatefulActivity<HanoiLibrary.HanoiWorkflowState>
    {
        public PostHanoiStateActivity(ActivityMemento activityMemento) : base(activityMemento)
        {
        }

        public override void Run(System.Threading.CancellationToken token, object state, object parameterDto = null)
        {
            if (!token.IsCancellationRequested)
            {
                var s = state as HanoiLibrary.HanoiWorkflowState;

                var stack1 = s.Stack1.ToArray()/*.Select(disk => $"{disk.Guid} : {disk.DiskSize}")*/;
                var stack2 = s.Stack2.ToArray()/*.Select(disk => $"{disk.Guid} : {disk.DiskSize}")*/;
                var stack3 = s.Stack3.ToArray()/*.Select(disk => $"{disk.Guid} : {disk.DiskSize}")*/;
                var oldTop = Console.CursorTop;
                var oldLeft = Console.CursorLeft;
                Console.WriteLine($"Towers of Hanoi current state round {s.Round}");
                Console.WriteLine($"Towers:");
                var blank = new string(' ', Console.WindowWidth - 1);
                for (int i = 0; i < s.NumberDisks + 1; i++)
                {
                    Console.Write(blank);
                }
                Console.CursorTop = oldTop + 2;
                Console.CursorLeft = oldLeft;
                int towerWidthPlusMargin1 = s.NumberDisks * 2 - 1 + 2;
                int currentCursorTop = Console.CursorTop;
                WriteTower(stack1, s.DiskBaseWidth, s.NumberDisks, Console.CursorLeft + 1 + (towerWidthPlusMargin1 * 0), currentCursorTop);
                if (s.NumberDisks % 2 != 0)
                {
                    WriteTower(stack2, s.DiskBaseWidth, s.NumberDisks, Console.CursorLeft + 1 + (towerWidthPlusMargin1 * 1), currentCursorTop);
                    WriteTower(stack3, s.DiskBaseWidth, s.NumberDisks, Console.CursorLeft + 1 + (towerWidthPlusMargin1 * 2), currentCursorTop);
                }
                else
                {
                    WriteTower(stack3, s.DiskBaseWidth, s.NumberDisks, Console.CursorLeft + 1 + (towerWidthPlusMargin1 * 1), currentCursorTop);
                    WriteTower(stack2, s.DiskBaseWidth, s.NumberDisks, Console.CursorLeft + 1 + (towerWidthPlusMargin1 * 2), currentCursorTop);
                }

                Console.CursorTop = oldTop;
                Console.CursorLeft = oldLeft;
            }
            else
            {
                Core.Logger.InfoFormat($"PostHanoiStateActivity has been canceled by user");
            }
            FinishActivity();
        }

        private void WriteTower(HanoiLibrary.HanoiDisk[] tower, int diskWidthMultiplicator, int numberDisks, int left, int top)
        {
            int colors = 16;
            
            int towerHeight = tower.Length;
            int width = numberDisks * 2 - 1;
            int horizontalCenter = left + width / 2;
            for (int i = 0; i < numberDisks; i++)
            {
                    Console.CursorTop = top + i;
                if (i < (numberDisks - towerHeight))
                {
                    Console.CursorLeft = horizontalCenter;
                    Console.Write("|");
                    //draw stick
                }
                else
                {
                    var towerIndex = i - (numberDisks-towerHeight);
                    var diskSize = tower[towerIndex].DiskSize / diskWidthMultiplicator;
                    Console.CursorLeft = horizontalCenter - (diskSize-1);
                    diskSize = diskSize * 2 - 1;
                    var color = (ConsoleColor)(((ulong)tower[towerIndex].Guid.GetHashCode()) % (ulong)(colors-1) +1);
                    Console.ForegroundColor = color;
                    Console.Write(new string('=', diskSize));
                    Console.ForegroundColor = ConsoleColor.White;
                }

            }
            Console.WriteLine();

        }
    }
}
