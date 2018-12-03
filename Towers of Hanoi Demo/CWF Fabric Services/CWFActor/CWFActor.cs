//-----------------------------------------------------------------------

// <copyright file="CWFActor.cs" company="Breanos GmbH">
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
using CWFActor.Interfaces;

namespace CWFActor
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
    internal class CWFActor : Actor, ICWFActor
    {
        /// <summary>
        /// Initialisiert eine neue Instanz von "CWFActor".
        /// </summary>
        /// <param name="actorService">Der "Microsoft.ServiceFabric.Actors.Runtime.ActorService", der diese Akteurinstanz hosten wird.</param>
        /// <param name="actorId">Die "Microsoft.ServiceFabric.Actors.ActorId" für diese Akteurinstanz.</param>
        public CWFActor(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        public Task<string> HelloWorldAsync()
        {
            return Task.FromResult("Hello from Actor!");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task<int> StartWorkflowEngine()
        {
            CWF.Core.CWFEngine engine = new CWF.Core.CWFEngine(string.Empty);
            engine.Run();
            return Task.FromResult(33);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task<int> StartWorkflowEngine2()
        {
            //CWF.Core.CWFEngine engine = new CWF.Core.CWFEngine(string.Empty);
            //engine.Run();
            

            var activitiesDir = ActorService.Context.CodePackageActivationContext.GetDataPackageObject("Activities");
            ActorEventSource.Current.ActorMessage(this, $"activitiesDir= {activitiesDir.Path}");

            var fsmDir = ActorService.Context.CodePackageActivationContext.GetDataPackageObject("FSM");
            ActorEventSource.Current.ActorMessage(this, $"fsmDir= {fsmDir.Path}");

            var workflowsDir = ActorService.Context.CodePackageActivationContext.GetDataPackageObject("Workflows");
            ActorEventSource.Current.ActorMessage(this, $"workflowsDir= {workflowsDir.Path}");

            var xsdDir = ActorService.Context.CodePackageActivationContext.GetDataPackageObject("XSD");
            ActorEventSource.Current.ActorMessage(this, $"xsdDir= {xsdDir.Path}");

            CWF.Core.CWFEngine engine = new CWF.Core.CWFEngine(workflowsDir.Path, xsdDir.Path+"\\Workflow.xsd", activitiesDir.Path, fsmDir.Path);
            engine.Run();
            engine.StartWorkflow(6);

            return Task.FromResult(34);
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
    }
}
