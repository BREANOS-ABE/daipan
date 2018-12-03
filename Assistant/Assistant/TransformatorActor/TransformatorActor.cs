//-----------------------------------------------------------------------

// <copyright file="TransformatorActor.cs" company="Breanos GmbH">
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
using BlackboardActor.Interfaces;
using AssistantUtilities;

namespace TransformatorActor
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
    internal class TransformatorActor : Actor, ITransformatorActor
    {
        private static BreanosLogger logger;

        public const string TransformatorLoggerKey = "TransformatorLogger";
        /// <summary>
        /// Initialisiert eine neue Instanz von "TransformatorActor".
        /// </summary>
        /// <param name="actorService">Der "Microsoft.ServiceFabric.Actors.Runtime.ActorService", der diese Akteurinstanz hosten wird.</param>
        /// <param name="actorId">Die "Microsoft.ServiceFabric.Actors.ActorId" für diese Akteurinstanz.</param>
        public TransformatorActor(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
            logger = BreanosLoggerFactory.CreateGet(TransformatorActor.TransformatorLoggerKey, nameof(BlackboardActor), s => ActorEventSource.Current.Message(s));
        }

        public Task<bool> Init()
        {
            logger.Debug("Hello from Init");
            return Task.FromResult<bool>(true);
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

        /// <summary>
        /// TODO: Ersetzen Sie die Methode durch Ihre eigene Akteurmethode.
        /// </summary>
        /// <returns></returns>
        Task<int> ITransformatorActor.GetCountAsync(CancellationToken cancellationToken)
        {
            return this.StateManager.GetStateAsync<int>("count", cancellationToken);
        }

        /// <summary>
        /// TODO: Ersetzen Sie die Methode durch Ihre eigene Akteurmethode.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        Task ITransformatorActor.SetCountAsync(int count, CancellationToken cancellationToken)
        {
            // Es ist nicht garantiert, dass Anforderungen in der entsprechenden Reihenfolge oder höchstens ein Mal verarbeitet werden.
            // Die Aktualisierungsfunktion überprüft hier, ob die eingehende Anzahl größer als die aktuelle Anzahl ist, um die Reihenfolge beizubehalten.
            return this.StateManager.AddOrUpdateStateAsync("count", count, (key, value) => count > value ? count : value, cancellationToken);
        }
    }
}
