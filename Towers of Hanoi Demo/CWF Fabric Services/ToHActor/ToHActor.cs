//-----------------------------------------------------------------------

// <copyright file="ToHActor.cs" company="Breanos GmbH">
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
//using ToHActor.Interfaces;
using NLog;
using BreanosConnectors.ActiveMqConnector;
using BreanosConnectors.Kpu.Communication.Common;
using BreanosConnectors;
using Microsoft.Extensions.Configuration;
using BreanosConnectors.Kpu.Communication.Utilities;
using CWFStateless;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using CWF.Interfaces;

namespace ToHActor
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
    internal class ToHActor : Actor, IToHActor
    {
        private NLog.Logger logger = LogManager.GetCurrentClassLogger();

        //static public string KPU_ID { get; set; } = "Hanoi";

        CWF.Core.CWFEngine _engine;
        string KpuId { get; set; }

        Connector _modelUpdateConnector;
       
        private IConfigurationRoot _configuration;
        public IConfigurationRoot Configuration => _configuration;

        ModelUpdateLatestPropertyChangeBatcher _batcher;
        /// <summary>
        /// Initialisiert eine neue Instanz von "ToHActor".
        /// </summary>
        /// <param name="actorService">Der "Microsoft.ServiceFabric.Actors.Runtime.ActorService", der diese Akteurinstanz hosten wird.</param>
        /// <param name="actorId">Die "Microsoft.ServiceFabric.Actors.ActorId" für diese Akteurinstanz.</param>
        public ToHActor(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
            logger.Info("Message from ToHActor");

            _modelUpdateConnector = new Connector();
            _batcher = new ModelUpdateLatestPropertyChangeBatcher(SendMessageToServiceBus);          
        }

        public Task<string> GetKpuId()
        {
            return Task.FromResult<string>(KpuId);
        }

        public Task RestartKpu()
        {
            _engine?.Stop();
            _engine?.Run();

            return Task.CompletedTask;
        }
        public Task StartKpu()
        {
            _engine?.Run();
            return Task.CompletedTask;
        }
        public Task StopKpu()
        {
            _engine?.Stop();
            return Task.CompletedTask;
        }
        
        public void SendMessageToServiceBus(IEnumerable<ModelUpdate> model)
        {
            var array = model.ToArray();
            string packedStr = SerializationHelper.Pack(array);
            _modelUpdateConnector.SendAsync(packedStr, KPURegistration.QueueString, BrokerCommands.MODEL_UPDATE, new (string, object)[] { ("KpuId", KpuId) }).Wait();            
        }       

        private void _engine_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {                     
            ModelUpdate update = new ModelUpdate();
            update.TimestampUtc = DateTime.Now;
            update.ModelId = KpuId;
            update.Property = e.PropertyName;

            var value = sender.GetType().GetProperty(e.PropertyName).GetValue(sender);
            var expectedType = sender.GetType().GetProperty(e.PropertyName).PropertyType;
            var serializationMethod = typeof(BreanosConnectors.SerializationHelper).GetMethod("Serialize").MakeGenericMethod(expectedType);
            var serializedValue = (string)serializationMethod.Invoke(null, new[] { value });

            update.Value = serializedValue;

            _batcher.OnMessage(update);            
        }

        public Task<int> StartTowersOfHanoiKPU(string connectionString, string user, string password, string workflowsDir, string xsdDir, string activitiesDir, string fsmDir)
        {
            logger.Debug("StartTowersOfHanoiKPU");

            KpuId = "Hanoi";

            ActorEventSource.Current.ActorMessage(this, "StartTowersOfHanoiKPU new message");

            //var activitiesDir = ActorService.Context.CodePackageActivationContext.GetDataPackageObject("Activities");
            //ActorEventSource.Current.ActorMessage(this, $"activitiesDir= {activitiesDir.Path}");

            //var fsmDir = ActorService.Context.CodePackageActivationContext.GetDataPackageObject("FSM");
            //ActorEventSource.Current.ActorMessage(this, $"fsmDir= {fsmDir.Path}");

            //var workflowsDir = ActorService.Context.CodePackageActivationContext.GetDataPackageObject("Workflows");
            //ActorEventSource.Current.ActorMessage(this, $"workflowsDir= {workflowsDir.Path}");

            //var xsdDir = ActorService.Context.CodePackageActivationContext.GetDataPackageObject("XSD");
            //ActorEventSource.Current.ActorMessage(this, $"xsdDir= {xsdDir.Path}");

            //var KPUDir = ActorService.Context.CodePackageActivationContext.GetDataPackageObject("KPU");
            //ActorEventSource.Current.ActorMessage(this, $"KPUDir= {KPUDir.Path}");

            //var TempDir = ActorService.Context.CodePackageActivationContext.GetDataPackageObject("Temp");
            //ActorEventSource.Current.ActorMessage(this, $"TempDir= {TempDir.Path}");            
           
            _modelUpdateConnector.ConnectAsync(connectionString, user, password).Wait();

            ICwfService cwfStateless = ServiceProxy.Create<ICwfService>(new Uri("fabric:/CWF.Fabric.Services/CWFStateless"));
            Task<int> i = cwfStateless.RegisterKPU(KpuId);
            int j = i.Result;
            
            /*_engine = new CWF.Core.CWFEngine(workflowsDir, xsdDir + "\\Workflow.xsd", activitiesDir, fsmDir);

            _engine.PropertyChanged += _engine_PropertyChanged;
            _engine.Run();*/
            //_engine.StartWorkflow(10);

            return Task.FromResult(0);
        }

        public Task<int> StartKpuActor(string kpuId, string workflowsDir, string xsdDir, string activitiesDir, string fsmDir, string kpuDir, string connectionString, string user, string password)
        {
            logger.Debug("StartKpuActor");

            KpuId = kpuId;

            ActorEventSource.Current.ActorMessage(this, "StartKpu new message");

            _modelUpdateConnector.ConnectAsync(connectionString, user, password).Wait();

            ICwfService cwfStateless = ServiceProxy.Create<ICwfService>(new Uri("fabric:/CWF.Fabric.Services/CWFStateless"));
            Task<int> i = cwfStateless.RegisterKPU(KpuId);
            int j = i.Result;

            _engine = new CWF.Core.CWFEngine(workflowsDir, xsdDir + "\\Workflow.xsd", activitiesDir, fsmDir);

            _engine.PropertyChanged += _engine_PropertyChanged;
            _engine.Run();
                //_engine.StartWorkflow(10);
            return Task.FromResult<int>(1);
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
