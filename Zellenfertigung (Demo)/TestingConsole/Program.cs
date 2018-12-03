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

using GetGantryJob.Interfaces;
using InitializeWFParams.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using System;
using System.Threading.Tasks;

namespace TestingConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            IInitializeWFParams actor = ActorProxy.Create<IInitializeWFParams>(ActorId.CreateRandom(), new Uri("fabric:/Zellenfertigung_Demo/InitializeWFParamsActorService"));
            Task<string> retval = actor.GetHelloWorldInitAsync();
            Console.WriteLine($"Hello World! {retval.Result}");

            IGetGantryJob getGantryJobActor = ActorProxy.Create<IGetGantryJob>(ActorId.CreateRandom(), new Uri("fabric:/Zellenfertigung_Demo/GetGantryJobActorService"));
            Task<string> GantryJobRetVal = getGantryJobActor.GetHelloWorldGantryJobAsync(retval.Result);
            Console.WriteLine($"Hello World! {GantryJobRetVal.Result}");

            //IMyService helloWorldClient = ServiceProxy.Create<IMyService>(new Uri("fabric:/Zellenfertigung_Demo/Stateless1"));

            //var message = helloWorldClient.HelloWorldAsync();
            //Console.WriteLine($"Hello World von StatelessService {message.Result }");
            Console.ReadKey();
        }
    }
}
