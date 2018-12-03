//-----------------------------------------------------------------------

// <copyright file="Program.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Monday, December 3, 2018 3:34:35 PM</date>
// </copyright>

//-----------------------------------------------------------------------

//-----------------------------------------------------------------------

// <copyright file="Program.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Tuesday, October 30, 2018 2:54:51 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using NLog;
using System;
using CWF.Core;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;


namespace BIF.Demo.App
{
    class Program
    {
        /// <summary>
        /// This is the current logger instance
        /// </summary>
        private static NLog.Logger logger = LogManager.GetCurrentClassLogger();

        static private System.Threading.Thread myThread;

      


        static void Main(string[] args)
        {                   
            ThreadStart threadDelegate = new ThreadStart(Work.DoWork);
            Thread newThread = new Thread(threadDelegate);
            newThread.Start();

               
        }

        class Work
        {
            private static async Task HandleTimer(CWFEngine engine, System.Timers.Timer timer)
            {
                Random random = new Random(Guid.NewGuid().GetHashCode());
                int randomNumber = random.Next(5, 15);

                timer.Interval = randomNumber * 1000;
                System.Console.WriteLine($"Timer initialized with {randomNumber} * 1000");

                engine.Stop();
                engine.Run();
               
            }

            public static async void DoWork()
            {
                logger.Debug("Starting Workflow Engine");
                CWFEngine bif = new CWFEngine("C:\\Cwf\\Cwf.xml", 50);

                Random random = new Random(Guid.NewGuid().GetHashCode());
                int randomNumber = random.Next(5, 10);

                System.Console.WriteLine($"Timer initialized with {randomNumber} * 1000");
                System.Timers.Timer timer = new System.Timers.Timer(1000 * randomNumber);
                
                timer.Elapsed += async (sender, e) => await HandleTimer(bif, timer);
                timer.Start();

                bif.Run();
                
                /*try
                {
                    bif.StartWorkflow(10);
                    Task.Delay(2000).Wait();
                    bif.Stop();
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e.ToString());
                }*/
             
                Console.ReadLine();                
            }          
        }
    }
}
