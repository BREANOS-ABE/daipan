//-----------------------------------------------------------------------

// <copyright file="PresenterService.cs" company="Breanos GmbH">
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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using AssistantInternalInterfaces;
using System.Runtime.CompilerServices;
using NLog;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using AssistantUtilities;
using BreanosConnectors.Kpu.Communication.Common;

namespace PresenterService
{
    class CurrentModelUpdateCache : Dictionary<string,ModelUpdate>
    {
        public string ModelId { get; set; }
        private Dictionary<string,ModelUpdate> thisAsBase { get { return (this as Dictionary<string, ModelUpdate>); } }
        public new ModelUpdate this[string index]
        {
            get
            {
                if (ContainsKey(index)) return thisAsBase[index];
                else return null;
            }
            set
            {
                if (!ContainsKey(index)) thisAsBase[index] = value;
                else if (thisAsBase[index].TimestampUtc < value.TimestampUtc) thisAsBase[index] = value;
            }
        }
    }
    class ConnectionSubscriptionCollection : List<ConnectionSubscription>
    {
        public bool Contains(string connectionId)
        {
            return this.Where(c => c.ConnectionId == connectionId).Any();
        }
        public bool Remove(string connectionId)
        {
            return (this.Contains(connectionId))?(this.Remove(this.FirstOrDefault(c => c.ConnectionId == connectionId))):(false);
        }
    }
    class ConnectionSubscription
    {
        public string ConnectionId { get; set; }
        public string ModelId { get; set; }
    }

    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class PresenterService : StatelessService, IPresenterService
    {
        private const string classname = nameof(PresenterService);
        private static BreanosLogger logger = new BreanosLogger(classname, ServiceEventSource.Current.Message);
        private IExternalCommunicationService _exComService;
        private const string _exComServiceUri = "fabric:/Assistant/ExternalCommunicationService";
        private Dictionary<string,ConnectionSubscriptionCollection> _modelSubscriptions;
        private Dictionary<string, CurrentModelUpdateCache> _currentModelUpdates;
        private Dictionary<string, FullBatcher<ModelUpdate>> _batchers;
        private const int _batchUpdateDelay = 200;
        #region c'tor & service
        public PresenterService(StatelessServiceContext context)
            : base(context)
        {
            logger.Trace();
            _modelSubscriptions = new Dictionary<string, ConnectionSubscriptionCollection>();
            _currentModelUpdates = new Dictionary<string, CurrentModelUpdateCache>();
            _batchers = new Dictionary<string, FullBatcher<ModelUpdate>>();
            _exComService = ServiceProxy.Create<IExternalCommunicationService>(new Uri(_exComServiceUri));
        }
        public async Task Initialize()
        {           
            logger.Trace();
        }
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners() => this.CreateServiceRemotingInstanceListeners();

        #endregion
        #region Public methods

        public async Task SubscribeConnectionToModel(string connectionId, string modelId)
        {
            logger.Trace();
            if (_modelSubscriptions.ContainsKey(modelId))
            {
                if (_modelSubscriptions[modelId].Contains(connectionId))
                {
                    logger.Warn($"Tried to subscribe {connectionId} to {modelId} but subscription already existed");
                }
                else
                {
                    _modelSubscriptions[modelId].Add(new ConnectionSubscription()
                    {
                        ConnectionId = connectionId,
                        ModelId = modelId
                    });
                    logger.Trace($"Added {connectionId} to subscriptions for {modelId}");
                }
            }
            else
            {
                await AppendKnownModel(modelId);
                _modelSubscriptions[modelId].Add(new ConnectionSubscription()
                {
                    ConnectionId = connectionId,
                    ModelId = modelId
                });
                logger.Trace($"Added {connectionId} to subscriptions for an as of yet unknown model {modelId}");
            }
        }

        public async Task UnsubscribeConnectionFromModel(string connectionId, string modelId)
        {
            logger.Trace();
            if (_modelSubscriptions.ContainsKey(modelId))
            {
                if (_modelSubscriptions[modelId].Contains(connectionId))
                {
                    _modelSubscriptions[modelId].Remove(connectionId);
                    logger.Trace($"Removed {connectionId} to subscriptions for {modelId}");
                }
                else
                {
                    logger.Warn($"Tried to unsubscribe {connectionId} from {modelId} but subscription didn't exist");
                }
            }
            else
            {
                logger.Warn($"Tried to unsubscribe {connectionId} to {modelId} but that model is unknown");
            }
        }

        public async Task UnsubscribeConnection(string connectionId)
        {
            int i = 0;
            foreach (var kv in _modelSubscriptions)
            {
                if (kv.Value.Contains(connectionId))
                {
                    kv.Value.Remove(connectionId);
                    i++;
                }
            }
            logger.Trace($"Unsubscribed {connectionId} from {i} models");
        }

        public async Task AppendKnownModel(string modelId)
        {
            logger.Trace();
            if (!_modelSubscriptions.ContainsKey(modelId))
            {
                _modelSubscriptions.Add(modelId, new ConnectionSubscriptionCollection());
                _currentModelUpdates.Add(modelId, new CurrentModelUpdateCache());
                _batchers.Add(modelId, new FullBatcher<ModelUpdate>((batch) =>
                {
                    if (_modelSubscriptions[modelId].Any())
                        _exComService.OnModelBatchUpdate(_modelSubscriptions[modelId].Select(ms=>ms.ConnectionId).ToArray(), batch.ToArray());
                }, _batchUpdateDelay));
                logger.Trace($"Added {modelId} to the list of known models");
            }
            else
            {
                logger.Warn($"Attempted to append known models with {modelId} but the model was already known");
            }
        }

        public async Task RemoveKnownModel(string modelId)
        {
            logger.Trace();
            if (_modelSubscriptions.ContainsKey(modelId))
            {
                _modelSubscriptions.Remove(modelId);
                _currentModelUpdates.Remove(modelId);
                _batchers.Remove(modelId);
                logger.Trace($"Removed {modelId} from the list of known models");
            }
            else
            {
                logger.Warn($"Attempted to remove {modelId} from the list of known models but the model was not known");
            }
        }

        public async Task BatchUpdateValues(ModelUpdate[] updates)
        {
            foreach (var update in updates)
            {
                if (_currentModelUpdates.ContainsKey(update.ModelId))
                {
                    _currentModelUpdates[update.ModelId][update.Property] = update;
                    _batchers[update.ModelId].OnMessage(update);
                }
            }
        }
        #endregion
        #region Private methods

        #endregion
    }
}
