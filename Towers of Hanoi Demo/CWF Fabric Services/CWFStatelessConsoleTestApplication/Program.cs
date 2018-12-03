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

using CWF.Interfaces;
//using CWFActor.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System;
using System.Threading.Tasks;
//using ToHActor.Interfaces;

namespace CWFStatelessConsoleTestApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            ICwfService helloWorldClient = ServiceProxy.Create<ICwfService>(new Uri("fabric:/CWF.Fabric.Services/CWFStateless"));
            Task<int> ii = helloWorldClient.StartTowersOfHanoi();
            int i = ii.Result;

            //Task<IToHActor> test = helloWorldClient.GetActorFromKpuId("Hanoi");
            //IToHActor i1 = test.Result;

            //Task<IToHActor> test2 = helloWorldClient.GetActorFromKpuId("Hanoi");
            //IToHActor i2 = test2.Result;

            //Task<string> message = helloWorldClient.HelloWorldAsync();
            //System.Console.WriteLine(message.Result);

            //ActorId actorId = ActorId.CreateRandom();

            //ICWFActor actorClient = ActorProxy.Create<ICWFActor>(actorId, new Uri("fabric:/CWF.Fabric.Services/CWFActorService"));
            //Task<string> message2 = actorClient.HelloWorldAsync();
            //System.Console.WriteLine(message2.Result);
          
           /* IToHActor actorClient2 = ActorProxy.Create<IToHActor>(actorId, new Uri("fabric:/CWF.Fabric.Services/ToHActorService"));
            Task<int> retVal = actorClient2.StartTowersOfHanoiKPU(string.Empty, string.Empty, string.Empty, string.Empty);
            System.Console.WriteLine(retVal.Result);
            System.Console.ReadLine();*/
        }
    }
}
