//-----------------------------------------------------------------------

// <copyright file="ExternalCommunicationService.cs" company="Breanos GmbH">
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
using System.Fabric;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AssistantInternalInterfaces;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Remoting;
using System.Text;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Collections.Concurrent;
using NLog;
using AssistantUtilities;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;
using BreanosConnectors.Kpu.Communication.Common;

namespace ExternalCommunicationService
{

    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class ExternalCommunicationService : StatelessService, IExternalCommunicationService
    {
        #region Private fields / references
        private const string classname = nameof(ExternalCommunicationService);
        private static BreanosLogger logger = new BreanosLogger(classname, ServiceEventSource.Current.Message);
        private IWebHost _host;
        private ISecurityService _securityService;
        private const string _securityServiceUri = "fabric:/Assistant/SecurityService";
        private ICoreService _coreService;
        private const string _coreServiceUri = "fabric:/Assistant/CoreService";
        private IPresenterService _presenterService;
        private const string _presenterServiceUri = "fabric:/Assistant/PresenterService";
        private const string _filePathPrefix = "C:/assistant/fileRepository/";
        private const string _markupPathPrefix = "C:/assistant/markupRepository/";
        private object _subscriptionLock = new object();
        #endregion
        #region Quasi-Singleton accessor
        private static ExternalCommunicationService _instance;
        public static ExternalCommunicationService Instance { get => _instance; private set => _instance = value; }
        #endregion
        #region Construction & Initialization
        public ExternalCommunicationService(StatelessServiceContext context)
            : base(context)
        {
            logger.Trace();
            _instance = this;
            Initialize();
        }
        private void Initialize()
        {
            
            _securityService = ServiceProxy.Create<ISecurityService>(new Uri(_securityServiceUri));
            _coreService = ServiceProxy.Create<ICoreService>(new Uri(_coreServiceUri));
            _presenterService = ServiceProxy.Create<IPresenterService>(new Uri(_presenterServiceUri));
            var ipAddress = $"http://{Startup.GetIpAddr() ?? ""}:{Startup.port}";
            _host = WebHost.CreateDefaultBuilder().UseStartup<Startup>().UseUrls(ipAddress).Build();
            _host.Start();
            logger.Trace("Initialization complete");
        }
        #endregion
        #region Service methods
        public async Task CreateSession(string connectionId)
        {
            logger.Trace($"ConnectionId ={connectionId}");

            await _securityService?.CreateSession(connectionId);
        }
        public async Task LinkUserToSession(string connectionId, string user)
        {
            logger.Trace($"ConnectionId ={connectionId}; User={user}");

            await _securityService?.SetSessionUser(connectionId, user);
        }
        public async Task<bool> Login(string connectionId, string user, string password)
        {
            var service = Startup.GetService<IClientProxy>();
            var isSuccess = await _securityService.Login(connectionId, user, password);
            if (isSuccess)
            {
                await LinkUserToSession(connectionId, user);
            }
            return isSuccess;
        }
        public async Task RequestExecute(string connectionId, string controllerId, string actionId, string[] parameters)
        {
            var service = Startup.GetService<IClientProxy>();
            var sb = new StringBuilder();
            sb.Append(controllerId).Append('.').Append(actionId).Append('(');
            if (parameters != null && parameters.Length > 0)
                sb.AppendJoin(", ", parameters);
            sb.Append(')');
            logger.Trace(sb.ToString());
            var permissionId = $"{controllerId}.{actionId}";
            if (await _securityService?.CheckPermission(connectionId, permissionId))
            {
                await _coreService.RequestExecute(controllerId, actionId, parameters);
            }
            else
            {
                logger.Trace("User did not have permission for action");
                await service.SendMessage(connectionId, "You do not have permission for that action", "Ok");
            }
        }

        public async Task ParseManagementRequest(string connectionId, string actionId, string[] parameters)
        {
            logger.Trace();
            var service = Startup.GetService<IClientProxy>();
            var isPermitted = false;
            if (isPermitted)
            {
                switch (actionId)
                {
                    default:
                        break;
                }
            }
            else
            {
                
                await service.SendMessage(connectionId, $"You are not permitted to initiate {actionId}");
            }
        }
        public async Task ReceiveViewRequest(string connectionId, string viewId)
        {
            if (!await _securityService.CheckPermission(connectionId, viewId + ".subscribe"))
            {
                logger.Warn($"User with connectionId {connectionId} requested view {viewId} but did not have permission to register");
                return;
            }
            await _coreService.RequestKpuPackage(connectionId, viewId);
        }
        public async Task ReceiveSubscribeRequest(string connectionId, string viewId)
        {
            logger.Trace();
            if (!await _securityService.CheckPermission(connectionId, viewId + ".subscribe"))
            {
                logger.Warn($"User with connectionId {connectionId} requested subscription for {viewId} but did not have permission to subscribe");
                return;
            }
            await _presenterService.SubscribeConnectionToModel(connectionId, viewId);
        }
        public async Task ReceiveUnsubscribeRequest(string connectionId, string viewId)
        {
            logger.Trace();
            if (!await _securityService.CheckPermission(connectionId, viewId + ".subscribe"))
            {
                logger.Warn($"User with connectionId {connectionId} requested subscription for {viewId} but did not have permission to subscribe");
                return;
            }
            await _presenterService.UnsubscribeConnectionFromModel(connectionId, viewId);
        }
        public async Task SendKpuPackage(string connectionId, string kpuId, string data)
        {
            var service = Startup.GetService<IClientProxy>();
            await service.SendPackage(connectionId, kpuId, data);
        }
       
        public async Task OnModelBatchUpdate(string[] connectionIds, ModelUpdate[] updates)
        {
            logger.Trace($"called with {updates.Count()} updates");

            var service = Startup.GetService<IClientProxy>();
            IEnumerable<string> subs = new List<string>();
            await service.UpdateBatchClient(new TargetedBatchUpdate()
            {
                Subscriptions = connectionIds,
                Updates = updates
            });
        }

        public async Task OnClientDisconnected(string connectionId)
        {
            logger.Trace(connectionId);
        }
        public async Task CreateUser(string connectionId, string name, string password, string location)
        {
            logger.Trace($"connectionId={connectionId}, name={name}, password=*****, location={location}");
            await _securityService.CreateUser(connectionId, name, password, location);
        }
       
        public async Task<string> GetUserMenu(string connectionId)
        {
            return await _securityService.GetUserMenu(connectionId);
        }
        #endregion
        #region sfa service internal stuff
        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners() => this.CreateServiceRemotingInstanceListeners();


        #endregion
    }
}
