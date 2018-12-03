//-----------------------------------------------------------------------

// <copyright file="Connector.cs" company="Breanos GmbH">
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
using System.Threading.Tasks;
using Opc.Ua;
using Opc.Ua.Client;
namespace BreanosConnectors
{
    namespace OpcUaConnector
    {
        public enum OpcState
        {
            Disconnected,
            ConnectionLost,
            Connecting,
            ConnectedAndListening,
            Connected,
            Unknown
        }

        public class Connector
        {
            public event OnMonitoredItemChanged MonitoredItemChanged;
            public event OnOpcStateChanged OpcStateChanged;

            private Session _session;
            private Subscription _subscription;
            private SessionReconnectHandler _reconnectionHandler;
            private Dictionary<string, MonitoredItem> _items;
            private ConnectorLogging _logger = new ConnectorLogging(nameof(Connector));
            private ConnectorConfiguration _config;
            #region State Machine
            public class OpcStateChangedEventArgs : EventArgs
            {
                public OpcState LastState { get; set; }
                public OpcState State { get; set; }
            }
            public delegate void OnOpcStateChanged(object sender, OpcStateChangedEventArgs e);
            private OpcState _state;
            public OpcState State { get { return _state; } private set { var last = _state; _state = value; if (last != value) OnStateChanged(last); } }
            private void OnStateChanged(OpcState lastState)
            {
                _logger.Trace($"{lastState.ToString()}->{State.ToString()}");
                OpcStateChanged?.Invoke(this, new OpcStateChangedEventArgs() { LastState = lastState, State = this.State });
            }
            #endregion

            public Connector(ConnectorConfiguration config = null)
            {
                _logger.Trace();
                _config = config ?? ConnectorConfiguration.DefaultConfiguration();
                _items = new Dictionary<string, MonitoredItem>();
            }
            public async Task ConnectAsync(string url, string sessionName = "BreanosOpcConnectorSession")
            {
                _logger.Trace(ConnectorLogging.Process((nameof(url), url), (nameof(sessionName), sessionName)));
                while (State != OpcState.Connected)
                {
                    try
                    {
                        _session = await Session.Create(CreateConfig(), new ConfiguredEndpoint(null, new EndpointDescription(url)), true, sessionName, (uint)_config.SessionTimeout, null, null);
                        State = OpcState.Connected;
                    }
                    catch (Exception e)
                    {
                        _logger.Error($"Exception while creating a session with an OPC server: {e.ToString()}");
                        State = OpcState.Disconnected;
                        await Task.Delay(_config.ReconnectAttemptInterval);
                    }
                }
                _session.KeepAliveInterval = _config.KeepAliveInterval;
                _session.KeepAlive += _session_KeepAlive;
                _session.PublishError += _session_PublishError;
            }
            public void CloseConnection()
            {
                try
                {
                    _session?.Close();
                    _session?.Dispose();
                    _subscription.DeleteItems();
                    _subscription.Delete(true);
                    _subscription.Dispose();
                }
                catch (Exception e)
                {
                    _logger.Warn($"Exception while closing connection to OPC server: {e.ToString()}");
                }
                finally
                {
                    State = OpcState.Disconnected;
                    _session = null;
                    _subscription = null;
                }
            }
            private void _session_PublishError(Session session, PublishErrorEventArgs e)
            {
                _logger.Trace();
            }

            private void _session_KeepAlive(Session session, KeepAliveEventArgs e)
            {
                _logger.Trace();
                if (e != null && e.Status != null)
                {
                    if (!Object.ReferenceEquals(session, _session))
                    {
                        return;
                    }
                    if (ServiceResult.IsBad(e.Status))
                    {
                        _logger.Warn($"Connection to OPC server lost on keepalive interval");
                        State = OpcState.ConnectionLost;
                        _session.KeepAlive -= _session_KeepAlive;
                        ReestablishConnection();
                    }
                }
            }

            private void ReestablishConnection()
            {
                _logger.Trace();
                _reconnectionHandler = new SessionReconnectHandler();
                State = OpcState.Connecting;
                _reconnectionHandler.BeginReconnect(_session, _config.ReconnectAttemptInterval, OnSessionReconnectComplete);
            }
            private void OnSessionReconnectComplete(object sender, EventArgs e)
            {
                _logger.Trace();
                if (_reconnectionHandler == null)
                {
                    _logger.Error($"ReconnectComplete called but reconnect handler was null");
                    State = OpcState.Disconnected;
                    return;
                }
                if (!Object.ReferenceEquals(sender, _reconnectionHandler))
                {
                    _logger.Info($"Current reconnectHandler did not match the sender of this method");
                    return;
                }
                _session = _reconnectionHandler.Session;
                _session.KeepAliveInterval = _config.KeepAliveInterval;
                _session.KeepAlive += _session_KeepAlive;
                _session.PublishError += _session_PublishError;
                _reconnectionHandler.Dispose();
                _reconnectionHandler = null;
                State = OpcState.ConnectedAndListening;
            }
            private bool RefreshSessionSubscriptionInterconnection()
            {
                _logger.Trace();
                try
                {
                    if (_session == null) return false;
                    if (_subscription != null && _subscription.Session != _session)
                    {
                        _subscription.Delete(true);
                        _subscription = null;
                    }
                    if (_subscription == null)
                    {
                        _subscription = new Subscription(_session.DefaultSubscription)
                        {
                            PublishingInterval = 5000
                        };
                    }
                    return true;
                }
                catch (Exception e)
                {
                    _logger.Error($"Exception while ensuring session/subscription compatibility: {e.ToString()}");
                    return false;
                }
            }
            public void InitiateListeningSubscription()
            {
                _logger.Trace();
                _subscription = new Subscription(_session.DefaultSubscription)
                {
                    PublishingInterval = _config.PublishingInterval
                };
            }
            public (ResponseHeader responseHeader, StatusCodeCollection statusCodes, DiagnosticInfoCollection diagnosticInfos)
                Write(params (string, object)[] values)
            {
                _logger.Trace();
                WriteValueCollection wvc = new WriteValueCollection();
                try
                {
                    foreach (var value in values)
                    {
                        wvc.Add(new WriteValue()
                        {
                            NodeId = value.Item1,
                            AttributeId = Attributes.Value,
                            IndexRange = null,
                            Value = new DataValue()
                            {
                                Value = value.Item2
                            }
                        });
                    }
                }
                catch (Exception e)
                {
                    _logger.Fatal($"Exception while creating a WriteValueCollection: {e.ToString()}");
                }

                try
                {
                    var responseHeader = _session.Write(null, wvc, out var statusCodes, out var diagnosticInfos);
                    return (responseHeader, statusCodes, diagnosticInfos);
                }
                catch (Exception e)
                {
                    _logger.Fatal($"Exception while writing a WriteValueCollection to an OPC server: {e.ToString()}");
                }
                return (null, null, null);
            }
            public void ListenNodesFromXml(string path)
            {
                OpcConfiguration config = null;
                try
                {
                    config = OpcXmlNodeLoader.LoadNodes(path);
                }
                catch (Exception e)
                {
                    _logger.Error($"Error on loading nodes from xml file for listening: {e.ToString()}");
                    return;
                }
                List<SNode> flattenedStructure = new List<SNode>();
                foreach (var sub in config.Subscriptions)
                {
                    foreach (var node in sub.NodeCollection)
                    {
                        flattenedStructure.AddRange(node.GetFlattenedStructure());
                    }
                }
                var nodeIds = flattenedStructure.Select(node => node.Path);
                Listen(nodeIds.ToArray());
            }
            public void Listen(params string[] nodeIds)
            {
                _logger.Trace();
                var nodeItems = nodeIds.Select(nid => new MonitoredItem(_subscription.DefaultItem)
                {
                    StartNodeId = nid,
                    DisplayName = nid
                });
                foreach (var nodeItem in nodeItems)
                {
                    nodeItem.Notification += NodeItem_Notification;
                    _subscription.AddItem(nodeItem);
                }
                UpdateSubscription(_subscription);
            }

            private void NodeItem_Notification(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs e)
            {
                MonitoredItemChanged?.Invoke(this, new OnMonitoredItemChangedEventArgs()
                {
                    Node = monitoredItem.StartNodeId.ToString(),
                    Value = (e.NotificationValue as MonitoredItemNotification).Value.Value,
                    ValueType = (e.NotificationValue as MonitoredItemNotification).Value.Value.GetType()
                });
            }

            private ApplicationConfiguration CreateConfig()
            {
                _logger.Trace();
                var cfg = new ApplicationConfiguration()
                {
                    ApplicationName = "OPCClient",
                    ApplicationType = ApplicationType.Client,
                    SecurityConfiguration = new SecurityConfiguration { ApplicationCertificate = new CertificateIdentifier { StoreType = @"Windows", StorePath = @"LocalMachine\My", SubjectName = Utils.Format(@"CN={0}", "localhost") }, TrustedPeerCertificates = new CertificateTrustList { StoreType = @"Windows", StorePath = @"CurrentUser\TrustedPeople", }, NonceLength = 32, AutoAcceptUntrustedCertificates = true },
                    TransportConfigurations = new TransportConfigurationCollection(),
                    TransportQuotas = new TransportQuotas { OperationTimeout = _config.OperationTimeout },
                    ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = _config.SessionTimeout },
                    CertificateValidator = new CertificateValidator()
                };
                cfg.Validate(ApplicationType.Client);
                if (cfg.SecurityConfiguration.AutoAcceptUntrustedCertificates)
                {
                    cfg.CertificateValidator.CertificateValidation += (s, e) => { e.Accept = (e.Error.StatusCode == StatusCodes.BadCertificateUntrusted); };
                }
                return cfg;
            }
            private bool UpdateSubscription(Subscription sub)
            {
                _logger.Trace();
                if (_session.SubscriptionCount <= 0 || !_session.Subscriptions.Contains(sub) || !sub.Created)
                {
                    if (!_session.AddSubscription(sub))
                    {
                        _logger.Error($"Could not add a subscription to the session");
                    }
                    try
                    {
                        sub.Create();
                    }
                    catch (Exception e)
                    {
                        _logger.Error($"Exception while updating subscription: {e.ToString()}");
                        return false;
                    }
                }
                else
                {
                    try
                    {
                        sub.ApplyChanges();
                    }
                    catch (Exception e)
                    {
                        _logger.Error($"Exception while applying changes to a subscription: {e.ToString()}");
                        return false;
                    }
                }
                return true;
            }
        }
    }
}
