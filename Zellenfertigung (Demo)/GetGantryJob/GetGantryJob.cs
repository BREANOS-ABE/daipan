//-----------------------------------------------------------------------

// <copyright file="GetGantryJob.cs" company="Breanos GmbH">
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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using GetGantryJob.Interfaces;
using BreanosConnectors.ActiveMqConnector;
using DataClasses;

namespace GetGantryJob
{
    /// <remarks>
    /// Diese Klasse stellt einen Akteur dar.
    /// Jede "ActorID" ist einer Instanz dieser Klasse zugeordnet.
    /// Das Attribut "StatePersistence" bestimmt die Persistenz und Replikation des Akteurzustands:
    ///  – Permanent: Der Zustand wird auf den Datenträger geschrieben und repliziert.
    ///  – Flüchtig: Der Zustand wird nur im Arbeitsspeicher gespeichert und repliziert.
    ///  – Keine: Der Zustand wird nur im Arbeitsspeicher gespeichert und nicht repliziert.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    internal class GetGantryJob : Actor, IGetGantryJob
    {
        /// <summary>
        /// Initialisiert eine neue Instanz von "GetGantryJob".
        /// </summary>
        /// <param name="actorService">Der "Microsoft.ServiceFabric.Actors.Runtime.ActorService", der diese Akteurinstanz hosten wird.</param>
        /// <param name="actorId">Die "Microsoft.ServiceFabric.Actors.ActorId" für diese Akteurinstanz.</param>
        public GetGantryJob(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        /// <summary>
        /// Diese Methode wird bei jeder Aktivierung eines Akteurs aufgerufen.
        /// Ein Akteur wird erstmals aktiviert, wenn eine seiner Methoden aufgerufen wird.
        /// </summary>
        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");

            // Der "StateManager" ist der Speicher des privaten Zustands dieses Akteurs.
            // In "StateManager" gespeicherte Daten werden für hohe Verfügbarkeit für Akteure repliziert, die flüchtigen oder permanenten Zustandsspeicher verwenden.
            // Ein beliebiges serialisierbares Objekt kann in "StateManager" gespeichert werden.
            // Weitere Informationen finden Sie unter https://aka.ms/servicefabricactorsstateserialization.

            return this.StateManager.TryAddStateAsync("count", 0);
        }

        public Task<string> GetHelloWorldGantryJobAsync(string input)
        {
            return Task.FromResult($"Hello from GetGantryJob Actor. I received the following Parameter: {input}");
        }

        public Task<string> CheckAndGetJob()
        {
            Connector connector = new Connector();
            connector.ConnectAsync("activemq:tcp://BRE-DEV02.breanos.local:61616", "admin", "admin");
            connector.Message += Connector_Message;
            connector.ListenAsync("TestQueue").Wait();
            return Task.FromResult("Test");
        }

        private void Connector_Message(object sender, BreanosConnectors.Interface.OnMessageEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
