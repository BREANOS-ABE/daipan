//-----------------------------------------------------------------------

// <copyright file="CWFStateless.cs" company="Breanos GmbH">
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
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using CWF.Interfaces;
//using ToHActor.Interfaces;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors;
using NLog;
using Microsoft.Extensions.Configuration;
using Microsoft.ServiceFabric.Actors.Query;

namespace CWFStateless
{
    /// <summary>
    /// Eine Instanz dieser Klasse wird von der Service Fabric-Laufzeit für jede Dienstinstanz erstellt.
    /// </summary>
    internal sealed class CWFStateless : StatelessService, ICwfService
    {
        private NLog.Logger logger = LogManager.GetCurrentClassLogger();

        KPURegistration _kpuRegisterer;

        private IConfigurationRoot _configuration;
        public IConfigurationRoot Configuration => _configuration;

        public CWFStateless(StatelessServiceContext context)
            : base(context)
        {
           // InitializeKPURegisterer();
            //StartTowersOfHanoi();
        }

        private void _kpuRegisterer_PropertyChanged(object sender, KpuStateEventArg e)
        {
            logger.Trace($"_kpuRegisterer_PropertyChanged Commando:{e.Commando}, KpuId:{e.KpuId}");

            if (e.Commando.CompareTo("restart") == 0)
            {
                RestartKpu(e.KpuId);
                //_engine.Stop();

                //_engine.Run();
            }
            else if (e.Commando.CompareTo("start") == 0)
            {
                logger.Debug($"In Run:{e.Commando}, KpuId:{e.KpuId}");
                //_engine.Run();
                StartKpu(e.KpuId);
            }
            else if (e.Commando.CompareTo("stop") == 0)
            {
                logger.Debug($"In Stop:{e.Commando}, KpuId:{e.KpuId}");
                StopKpu(e.KpuId);
                //_engine.Stop();
            }
        }

        public void ReadConfigurationFile()
        {
            try
            {
                _configuration = new ConfigurationBuilder()

                    .AddXmlFile("app.config")
                    .Build();
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
            }
        }

        string ActivitiesDir { get; set; }
        string FsmDir { get; set; }
        string WorkflowsDir { get; set; }
        string XsdDir { get; set; }
        string TempDir { get; set; }
        string KPUDir { get; set; }      
        string Endpoint { get; set; }
        string User { get; set; }
        string Password { get; set; }

        private void InitializeKPURegisterer()
        {
            var activitiesDir = Context.CodePackageActivationContext.GetDataPackageObject("Activities");
            ActivitiesDir = activitiesDir.Path;
            ServiceEventSource.Current.ServiceMessage(this.Context, $"activitiesDir= {activitiesDir.Path}");

            var fsmDir = Context.CodePackageActivationContext.GetDataPackageObject("FSM");
            FsmDir = fsmDir.Path;
            ServiceEventSource.Current.ServiceMessage(this.Context, $"fsmDir= {fsmDir.Path}");

            var workflowsDir = Context.CodePackageActivationContext.GetDataPackageObject("Workflows");
            WorkflowsDir = workflowsDir.Path;
            ServiceEventSource.Current.ServiceMessage(this.Context, $"workflowsDir= {workflowsDir.Path}");

            var xsdDir = Context.CodePackageActivationContext.GetDataPackageObject("XSD");
            XsdDir = xsdDir.Path;
            ServiceEventSource.Current.ServiceMessage(this.Context, $"xsdDir= {xsdDir.Path}");

            var kPUDir = Context.CodePackageActivationContext.GetDataPackageObject("KPU");
            KPUDir = kPUDir.Path;
            ServiceEventSource.Current.ServiceMessage(this.Context, $"KPUDir= {kPUDir.Path}");

            var tempDir = Context.CodePackageActivationContext.GetDataPackageObject("Temp");
            TempDir = tempDir.Path;
            ServiceEventSource.Current.ServiceMessage(this.Context, $"TempDir= {tempDir.Path}");

            ReadConfigurationFile();
            Endpoint = Configuration["connection:Endpoint"];
            User = Configuration["connection:User"];
            Password = Configuration["connection:Password"];

            _kpuRegisterer = new KPURegistration(KPUDir, TempDir, Endpoint, User, Password, ActivitiesDir, FsmDir, WorkflowsDir, XsdDir);
            _kpuRegisterer.PropertyChanged += _kpuRegisterer_PropertyChanged;

            logger.Info($"Nach InitDefaults {_kpuRegisterer.ConnectionString},{_kpuRegisterer.User},{_kpuRegisterer.Password}");           
        }

        public Task<int> StartTowersOfHanoi()
        {            
            ActorId actorId = ActorId.CreateRandom();
            IToHActor actorClient2 = ActorProxy.Create<IToHActor>(actorId, new Uri("fabric:/CWF.Fabric.Services/ToHActorService"));
            Task<int> retVal = actorClient2.StartTowersOfHanoiKPU(Endpoint, User, Password, WorkflowsDir, XsdDir, ActivitiesDir, FsmDir);
            int i = retVal.Result;
            return (Task.FromResult(1));
        }

        protected override System.Threading.Tasks.Task OnOpenAsync(System.Threading.CancellationToken cancellationToken)
        {
            InitializeKPURegisterer();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Optionale Außerkraftsetzung, um Listener (z. B. TCP, HTTP) für dieses Dienstreplikat zum Verarbeiten von Client- oder Benutzeranforderungen zu erstellen.
        /// </summary>
        /// <returns>Eine Sammlung von Listenern.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return this.CreateServiceRemotingInstanceListeners();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="kpuId"></param>
        /// <returns></returns>
        public Task<int> RegisterKPU(string kpuId)
        {
            _kpuRegisterer.RegisterKPU(kpuId, Endpoint, "LineTopic_1");
            return Task.FromResult<int>(1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="kpuIdSearchFor"></param>
        void StartKpu(string kpuIdSearchFor)
        {
            Task<IToHActor> tohActor = GetActorFromKpuId(kpuIdSearchFor);
            if (tohActor == null)
            {
                IToHActor tohActor2 = ActorProxy.Create<IToHActor>(ActorId.CreateRandom(), new Uri("fabric:/CWF.Fabric.Services/ToHActorService"));
                Task<int> ii = tohActor2.StartKpuActor(kpuIdSearchFor, WorkflowsDir, XsdDir, ActivitiesDir, FsmDir, KPUDir, Endpoint, User, Password);
                int i = ii.Result;
                return;
            }

            IToHActor actor = tohActor?.Result;
            actor.StartKpu();
        }
      
        /// <summary>
        /// 
        /// </summary>
        /// <param name="kpuIdSearchFor"></param>
        void StopKpu(string kpuIdSearchFor)
        {
            Task<IToHActor> tohActor = GetActorFromKpuId(kpuIdSearchFor);
           
            IToHActor actor = tohActor?.Result;
            actor?.StopKpu();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="kpuIdSearchFor"></param>
        void RestartKpu(string kpuIdSearchFor)
        {
            Task<IToHActor> tohActor = GetActorFromKpuId(kpuIdSearchFor);
            IToHActor actor = tohActor.Result;
            actor.RestartKpu();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="kpuIdSearchFor"></param>
        /// <returns></returns>
        public Task<IToHActor> GetActorFromKpuId(string kpuIdSearchFor)
        {
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken cancellationToken = source.Token;

            int partitionKey = 0;

            IActorService actorServiceProxy = ActorServiceProxy.Create(new Uri("fabric:/CWF.Fabric.Services/ToHActorService"), 0);

            ContinuationToken continuationToken = null;
            List<ActorInformation> activeActors = new List<ActorInformation>();

            do
            {
                Task<PagedResult<ActorInformation>> pageTask = actorServiceProxy.GetActorsAsync(continuationToken, cancellationToken);

                PagedResult<ActorInformation> page = pageTask.Result;
                activeActors.AddRange(page.Items.Where(x => x.IsActive));

                continuationToken = page.ContinuationToken;
            }
            while (continuationToken != null);

            foreach (ActorInformation info in activeActors)
            {
                var proxy = ActorProxy.Create<IToHActor>(info.ActorId, "fabric:/CWF.Fabric.Services");
                Task<string> kpuId = proxy.GetKpuId();
                var kpuid = kpuId.Result;
                if (kpuid.CompareTo(kpuIdSearchFor) == 0)
                {
                    return Task.FromResult<IToHActor>(proxy);
                }
            }
            return null;
        }       
    }
}
