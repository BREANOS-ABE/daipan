//-----------------------------------------------------------------------

// <copyright file="CoreService.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Tuesday, October 30, 2018 1:26:47 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AssistantInternalInterfaces;
using AssistantUtilities;
using BlackboardActor.Interfaces;
using BreanosConnectors.Kpu.Communication.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using NLog;

namespace CoreService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class CoreService : StatelessService, ICoreService
    {
        #region private members
        private IPresenterService _presenterService;
        private const string _presenterServiceUri = "fabric:/Assistant/PresenterService";
        private IExternalCommunicationService _exComService;
        private const string _exComServiceUri = "fabric:/Assistant/ExternalCommunicationService";
        private ISecurityService _securityService;
        private const string _securityServiceUri = "fabric:/Assistant/SecurityService";
        private Dictionary<string, string> _amqRoutes;
        private string _amqEndpoint, _amqUser, _amqPassword;
        private IConfigurationRoot _configuration;
        private const string classname = nameof(CoreService);
        private Dictionary<string, List<string>> _currentKpuPackageRequests;
        BreanosConnectors.ActiveMqConnector.Connector _activeMqConnector;
        private static BreanosLogger logger = new BreanosLogger(classname, ServiceEventSource.Current.Message);
        #endregion
        #region c'tor and initialization
        public CoreService(StatelessServiceContext context)
            : base(context)
        {
            logger.Trace();
            Initialize();
        }
        private async Task Initialize()
        {
            _presenterService = ServiceProxy.Create<IPresenterService>(new Uri(_presenterServiceUri));
            _exComService = ServiceProxy.Create<IExternalCommunicationService>(new Uri(_exComServiceUri));
            _securityService = ServiceProxy.Create<ISecurityService>(new Uri(_securityServiceUri));
            _currentKpuPackageRequests = new Dictionary<string, List<string>>();
            logger.Trace($"Initializing Presenter service");
            _presenterService.Initialize().Wait();
            logger.Trace($"Initializing Presenter service complete");
            try
            {
                _configuration = new ConfigurationBuilder().AddXmlFile("App.config").Build();
                _amqRoutes = new Dictionary<string, string>();
                _amqEndpoint = _configuration["Connection:Endpoint"];
                _amqUser = _configuration["Connection:User"];
                _amqPassword = _configuration["Connection:Password"];
                _amqRoutes["ToBlackboard"] = _configuration["Routes:ToBlackboard"];
                _amqRoutes["FromBlackboard"] = _configuration["Routes:FromBlackboard"];
                await SetupActiveMq();
            }
            catch (Exception e)
            {
                logger.Error($"Exception while reading app.config and getting amq routes: {e.ToString()}");
                return;
            }

            try
            {
                var proxy2 = ActorProxy.Create<IBlackboardActor>(new ActorId(42), "fabric:/Assistant");
                Task<bool> ret2 = proxy2.InitBlackboard();
                bool retVal3 = ret2.Result;

                Task rett;
                rett = proxy2.StartProcessing();
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
            }
            logger.Trace($"Initializing Blackboard Actor complete");

            InitializeTransformerActor();


            var registerForMessagesAtBlackboardRequest = new RoutingRequest()
            {
                Id = "Core",
                Path = _amqRoutes["FromBlackboard"],
                ContentTypes = new string[]
                {
                    BrokerCommands.PACKAGE,
                    BrokerCommands.KPU_REGISTRATION,
                    BrokerCommands.MODEL_UPDATE
                },
            };
            var registerPackage = BreanosConnectors.SerializationHelper.Pack(registerForMessagesAtBlackboardRequest);
            await _activeMqConnector.SendAsync(registerPackage, _amqRoutes["ToBlackboard"], BrokerCommands.CONFIGURE_ROUTES);
            logger.Trace($"Registration with Blackboard complete.");
        }

        private async void InitializeTransformerActor()
        {
            logger.Trace("InitializeTrnsformerActor");
            try
            {
                var transformatorProxy = ActorProxy.Create<ITransformatorActor>(new ActorId(17), "fabric:/Assistant");
                bool initok = await transformatorProxy.Init();
                
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
            }
        }

        private async Task SetupActiveMq()
        {
            logger.Trace();
            _activeMqConnector = new BreanosConnectors.ActiveMqConnector.Connector();
            var isConnected = await _activeMqConnector.ConnectAsync(_amqEndpoint, _amqUser, _amqPassword);
            _activeMqConnector.Message += _activeMqConnector_Message;
            var isListening = await _activeMqConnector.ListenAsync(_amqRoutes["FromBlackboard"]);
        }

        private async void SendKpuRegistrationRequestToSecurityService(KpuRegistrationRequest registrationRequest)
        {
            await _securityService.RegisterKpu(registrationRequest);
            logger.Trace($"SercurityService RegisterKpu has been called.");
        }

        private void _activeMqConnector_Message(object sender, BreanosConnectors.Interface.OnMessageEventArgs e)
        {
            logger.Trace();
            if (e == null)
            {
                logger.Error($"Received null message from ActiveMq");
                return;
            }
            if (e.Properties == null)
            {
                logger.Error($"Received message with null properties from ActiveMq");
            }
            if (!e.Properties.ContainsKey("ContentType"))
            {
                logger.Error($"Received a message from amq but did not contain ContentType");
                return;
            }
            logger.Trace($"Message from the AMQ: ContentType: {e.Properties["ContentType"]}; Content: {e.Content}");
            
            switch (e.Properties["ContentType"])
            {
                case BrokerCommands.KPU_REGISTRATION:
                    var isUnpackSuccessful = BreanosConnectors.SerializationHelper.TryUnpack(e.Content, out KpuRegistrationRequest registrationRequest);
                    if (isUnpackSuccessful) 
                    {
                        IncomingKpuRegistration(registrationRequest);
                        SendKpuRegistrationRequestToSecurityService(registrationRequest);
                    }
                    else
                        logger.Warn($"Could not unpack KpuRegistrationRequest");
                    break;
                case BrokerCommands.MODEL_UPDATE:
                    var IsUpdate = BreanosConnectors.SerializationHelper.TryUnpack(e.Content, out ModelUpdate[] updates);
                    if (IsUpdate)
                    {
                        UpdateModelValue(updates);
                    }
                    else
                    {
                        logger.Error($"Could not unpack modelupdate from kpu");
                    }
                    break;
                case BrokerCommands.PACKAGE:
                    var packageKpuId = (string)e.Properties["KpuId"];
                    if (_currentKpuPackageRequests.ContainsKey(packageKpuId) && _currentKpuPackageRequests[packageKpuId].Count > 0)
                    {
                        foreach (var connection in _currentKpuPackageRequests[packageKpuId])
                        {
                            SendKpuPackage(connection, packageKpuId, e.Content);
                        }
                        _currentKpuPackageRequests[packageKpuId].Clear();
                    }
                    break;
                default:
                    break;
            }
        }
        private async Task SendKpuPackage(string connectionId, string packageKpuId, string payload)
        {
            await _exComService.SendKpuPackage(connectionId, packageKpuId, payload);
        }


        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return this.CreateServiceRemotingInstanceListeners();
        }
        #endregion
        #region public methods       
        public async Task UpdateModelValue(ModelUpdate[] mus)
        {
            logger.Trace();
            try
            {
                await _presenterService.BatchUpdateValues(mus);
            }
            catch (Exception e)
            {
                logger.Error($"Exception caused on UpdateModelValue: {e.ToString()}");
            }
        }
        public async Task RequestKpuPackage(string connectionId, string id)
        {
            logger.Trace($"Requested for id {id}");
            if (!_currentKpuPackageRequests.ContainsKey(id))
            {
                logger.Error($"Could not request kpu package for {id} as that kpu is unknown");
            }
            _currentKpuPackageRequests[id].Add(connectionId);
            await _activeMqConnector.SendAsync("DummyText, do not remove", _amqRoutes["ToBlackboard"], BrokerCommands.PACKAGE_REQUEST, new[] { ("KpuId", (object)id) });
        }
      
        private async Task IncomingKpuRegistration(KpuRegistrationRequest registration)
        {
            logger.Trace();
            if (!_currentKpuPackageRequests.ContainsKey(registration.KpuId))
            {
                _currentKpuPackageRequests.Add(registration.KpuId, new List<string>());
            }
            PrintMenuXml(registration);
        }

        /// <summary>
        /// Helper method to print xml Menu into a device
        /// </summary>
        /// <param name="registration"></param>
        private void PrintMenuXml(KpuRegistrationRequest registration)
        {
            if (registration.MenuXmlString.Length != 0)
                logger.Info($"PrintMenuXml {registration.MenuXmlString}");
            else
                logger.Info($"PrintMenuXml returned empty string");
        }

        private async Task ProcessKpuPermissionRequest(string kpuId, KpuPermissionRequest request, KpuPermissionRequest parentPermissionRequest = null)
        {
            var permissionId = kpuId + '.' + request.PermissionIdentifier;
            if (parentPermissionRequest == null)
            {
                var result = await _securityService.CreatePermission(permissionId, null);
                if (result)
                {
                    logger.Info($"Registered base permission {permissionId} for {kpuId}");
                }
                else
                {
                    logger.Error($"Could not create base permission {permissionId} for {kpuId}");
                }
            }
            else
            {
                var parentPermissionId = kpuId + '.' + parentPermissionRequest.PermissionIdentifier;
                var result = await _securityService.CreatePermission(permissionId, parentPermissionId);
                if (result)
                {
                    logger.Info($"Registered child permission {permissionId} of {parentPermissionId} for {kpuId}");
                }
                else
                {
                    logger.Error($"Could not create child permission {permissionId} of {parentPermissionId} for {kpuId}");
                }
            }
            if (request.ChildPermissionRequests != null && request.ChildPermissionRequests.Length > 0)
            {
                foreach (var childRequest in request.ChildPermissionRequests)
                {
                    await ProcessKpuPermissionRequest(kpuId, childRequest, request);
                }
            }
        }
        private async Task IncomingKpuRegistration(string kpuId)
        {
            logger.Trace();
            if (!_currentKpuPackageRequests.ContainsKey(kpuId))
            {
                _currentKpuPackageRequests.Add(kpuId, new List<string>());
                logger.Trace($"Dictionary for current kpu package requests didn't contain key for kpu {kpuId}. Key created.");
            }
            await _presenterService.AppendKnownModel(kpuId);
        }
        public async Task RequestExecute(string id, string actionId, string[] parameters)
        {
            logger.Trace();
            ExecuteRequest request = new ExecuteRequest()
            {
                Action = actionId,
                KpuId = id,
                Parameters = parameters
            };
            var content = BreanosConnectors.SerializationHelper.Pack(request);
            await _activeMqConnector.SendAsync(content, _amqRoutes["ToBlackboard"], "ExecuteRequest");
        }
        #endregion


    }
}
