//-----------------------------------------------------------------------

// <copyright file="IBlackboardActor.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Tuesday, October 30, 2018 1:26:47 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BIKSClassLibrary;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Remoting.FabricTransport;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport;

//[assembly: FabricTransportServiceRemotingProvider(RemotingListener = RemotingListener.V2Listener, RemotingClient = RemotingClient.V2Client)]
[assembly: FabricTransportActorRemotingProvider(RemotingListener = RemotingListener.V2Listener, RemotingClient = RemotingClient.V2Client)]
namespace BlackboardActor.Interfaces
{
    /// <summary>
    /// Diese Schnittstelle definiert die Methoden, die ein Akteur bereitstellt.
    /// Clients verwenden diese Schnittstelle für die Interaktion mit dem Akteur, der sie implementiert.
    /// </summary>
    public interface IBlackboardActor : IActor
    {
        Task<bool> Init(int id, string action, BlackboardData arguments);
        Task<bool> InitBlackboard();

        /// <summary>
        /// Starts processing of the blackboard messages
        /// </summary>
        /// <returns></returns>
        Task<int> StartProcessing();
    }
        
}
