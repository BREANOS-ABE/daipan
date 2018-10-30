//-----------------------------------------------------------------------

// <copyright file="Connector.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Tuesday, October 30, 2018 1:26:47 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using Apache.NMS;
using BreanosConnectors.Interface_FW;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using Apache.NMS.Util;
using System.Collections.Generic;

namespace BreanosConnectors
{

    namespace ActiveMqConnector_FW
    {
        /// <summary>
        /// Implementation of the IMqConnector interface for use with an ActiveMQ Server.
        /// <para>Usage:</para>
        /// <para>c'tor -> ConnectAsync -> SendAsync</para>
        /// <para>c'tor -> ConnectAsync -> Message += my_handler -> ListenAsync</para>
        /// <para>c'tor -> ConnectionStateChanged += my_stateChangeHandler</para>
        /// </summary>
        public class Connector : IMqConnector
        {
            public delegate void OnConnectionStateChanged(object sender, ConnectionStateChangedEventArgs e);
            public event OnConnectionStateChanged ConnectionStateChanged;
            public event OnMessage Message;
            public int ReconnectWaitTimeMs { get; set; }
            private ISession _session;
            private IMessageConsumer _consumer;
            private string _connectionUri, _connectionUser, _connectionPassword, _listenToPath, _listenToSubscription, _listenToFilter;
            private bool _isInternalListeningHooked;
            private bool _isConnectionGood;
            private Task _reconnectionTask;
            private object _reconnectionTaskLock = new object();
            private CancellationTokenSource _ctsReconnect;
            private ConnectorLogging _logger = new ConnectorLogging(nameof(Connector));
            public Connector()
            {
                ReconnectWaitTimeMs = 5000; //default value. Wait for 5s between reconnection attempts
            }
            /// <summary>
            /// Attempts to establish a connection with a given ActiveMQ server defined by the parameters.
            /// The following sets of parameters are supported (examples for values will be shown in square brackets):
            /// <para>ConnectAsync(uri ["activemq:tcp://localhost:61616"], username ["user123"], password ["12345"])</para>
            /// <para>ConnectAsync(settingsFilePath ["AmqConnectionSettings.xml"])</para>
            /// </summary>
            /// <param name="parameters"></param>
            /// <returns></returns>
            public async Task<bool> ConnectAsync(params string[] parameters)
            {
                lock (_reconnectionTaskLock)
                {
                    if (_reconnectionTask != null)
                    {
                        _logger.Error($"Cannot initialize connecting as there is a reconnection attempt underway.");
                        return false;
                    }
                }
                return await ConnectAsyncInternal(parameters);
            }
            /// <summary>
            /// Initializes listening to a specific path on the Active MQ server.
            /// Please note, that, if not otherwise specified, it will be assumed that the path should be a queue.
            /// As such, if you want to specifically listen to a queue, prepend the path parameter with "queue://" otherwise with "topic://"
            /// </summary>
            /// <param name="path">The path to the message exchange</param>
            /// <param name="subscription">Not used by Active MQ. The subscription will be named automatically by the active mq server</param>
            /// <param name="filter">An SQL-like string to filter for values set on the meta properties of a message. This maps directly to the "properties" parameter (including "ContentType")  of SendAsync</param>
            /// <returns></returns>
            public async Task<bool> ListenAsync(string path, string subscription = null, string filter = null)
            {
                _logger.Trace(ConnectorLogging.Process((nameof(path), path), (nameof(subscription), subscription), (nameof(filter), filter)));
                if (_isInternalListeningHooked)
                {
                    _logger.Error($"Currently already listening to some path. Please call {nameof(StopListening)} before calling {nameof(ListenAsync)} again");
                    return false;
                }
                if (!_isConnectionGood)
                {
                    _logger.Error($"Currently not connected. Please wait for connection to be established before listening");
                    return false;
                }
                if (_session == null)
                {
                    _logger.Error($"Cannot listen to path if session has not been established");
                    return false;
                }
                if (string.IsNullOrEmpty(path))
                {
                    _logger.Error($"Bad Argument: {nameof(path)} was null");
                    return false;
                }
                PrependWithPathDefault(ref path);
                try
                {
                    _listenToPath = path;
                    _listenToSubscription = subscription;
                    _listenToFilter = filter;
                    IDestination destination;
                    //todo: add code to verify, path is not for queue when subscription is not null and vice versa
                    destination = SessionUtil.GetDestination(_session, path);
                    ITopic topicDestination = SessionUtil.GetTopic(_session, path);
                    if (string.IsNullOrEmpty(filter))
                    {
                        if (path.StartsWith("topic://"))
                        {
                            _logger.Trace($"Creating durable consumer for {topicDestination.ToString()}");
                            _consumer = _session.CreateDurableConsumer(topicDestination, subscription, filter, false);
                        }
                        else if (path.StartsWith("queue://"))
                        {
                            _logger.Trace($"Creating consumer for {destination.ToString()}");
                            _consumer = _session.CreateConsumer(destination);
                        }
                        else
                        {
                            _logger.Error($"Could not start listening because a path of {path} cannot be handled");
                            return false;
                        }
                    }
                    else
                    {
                        if (path.StartsWith("topic://"))
                        {
                            _logger.Trace($"Creating durable consumer for {topicDestination.ToString()}");
                            _consumer = _session.CreateDurableConsumer(topicDestination, subscription, filter, false);
                        }
                        else if (path.StartsWith("queue://"))
                        {
                            _logger.Trace($"Creating consumer for {destination.ToString()}");
                            _consumer = _session.CreateConsumer(destination, filter);
                        }
                        else
                        {
                            _logger.Error($"Could not start listening because a path of {path} cannot be handled");
                            return false;
                        }
                    }
                    _consumer.Listener += OnMessageReceived;
                    _isInternalListeningHooked = true;
                    _logger.Info($"Initialization for listening to {path} successful.");
                    return true;
                }
                catch (Exception e)
                {
                    _logger.Error($"Exception while starting listener: {e.ToString()}");
                }
                return false;

            }
            /// <summary>
            /// Attempts to send a message onto the connected ActiveMQ server to be distributed to the appropriate listeners
            /// Please note that, equally to the ListenAsync method, you should prepend the path with "topic://" or "queue://" to ensure the message is sent to the correct entity.
            /// </summary>
            /// <param name="payload">The message content, e.g. a serialized DTO</param>
            /// <param name="path">The topic / queue to send the message to. Please specifiy WITH a prepended "topic://" or "queue://". Otherwise, queue will be assumed</param>
            /// <param name="contentType">One of the properties(see next parameter) to be used for discerning the type of the DTO. E.g. set to AssemblyQualifiedName of the DTO's class</param>
            /// <param name="properties">Further metadata properties of the message to be sent. Please note that you cannot set the property "ContentType" here as it will be ignored. Please use the proper property for that purpose.</param>
            /// <param name="retries">How often should the connector retry on failure</param>
            /// <param name="retryDelayMs">How long should the connector wait before each retry</param>
            /// <returns></returns>
            public async Task<bool> SendAsync(string payload, string path, string contentType, (string, object)[] properties = null, int retries = 1, int retryDelayMs = 5000)
            {
                var propertiesExpanded = ((properties != null) ? (string.Join(", ", properties)) : ("null"));
                _logger.Trace(ConnectorLogging.Process((nameof(payload), payload), (nameof(path), path), (nameof(contentType), contentType), (nameof(properties), "( " + propertiesExpanded + " )"), (nameof(retries), retries), (nameof(retryDelayMs), retryDelayMs)));
                if (_session == null)
                {
                    _logger.Error($"Cannot send message if session has not been established");
                    return false;
                }
                PrependWithPathDefault(ref path);
                IDestination destination = null;
                try
                {
                    destination = SessionUtil.GetDestination(_session, path);
                    if (destination == null)
                    {
                        _logger.Error($"Destination was null while trying to send a message: {path}. Destination might not exist on the Active MQ");
                        return false;
                    }
                    if (!_isConnectionGood)
                    {
                        _logger.Error($"Currently not connected. Please wait for connection to be established sending messages");
                        return false;
                    }
                }
                catch (Exception e)
                {
                    _logger.Error($"Exception while getting destination: {e.ToString()}");
                }
                
                var currentAttempt = 0;
                while (currentAttempt++ <= retries)
                {
                    try
                    {
                        _logger.Trace($"Creating producer for destination {destination.ToString()}");
                        using (var producer = _session.CreateProducer(destination))
                        {
                            producer.DeliveryMode = MsgDeliveryMode.Persistent;
                            ITextMessage message = null;
                            message = producer.CreateTextMessage(payload);
                            message.Properties["ContentType"] = contentType;
                            if (properties != null)
                                foreach (var item in properties)
                                {
                                    if (item.Item1.Equals("ContentType")) continue;
                                    message.Properties[item.Item1] = item.Item2;
                                }
                            producer.Send(message);
                            _logger.Trace($"Message with contentType {contentType} sent to {destination.ToString()}");
                            return true; //everything went smoothly. we can stop retrying
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.Error($"Exception while sending message with contentType {contentType} to {path}; retrying in {retryDelayMs / 1000} seconds: {e.ToString()}");
                        await Task.Delay(retryDelayMs);
                    }
                }
                return false;
            }
            /// <summary>
            /// Attempts to return a listing of all available queues on the ActiveMQ server
            /// </summary>
            /// <returns></returns>
            public async Task<string[]> ListAllQueues()
            {
                string advisoryTopic = "ActiveMQ.Advisory.Queue";
                var dest = _session.GetTopic(advisoryTopic);
                List<string> queues = new List<string>();
                Task.Run(() =>
                {
                    using (IMessageConsumer consumer = _session.CreateConsumer(dest))
                    {
                        IMessage advisory;
                        while ((advisory = consumer.Receive(TimeSpan.FromMilliseconds(500))) != null)
                        {
                            Apache.NMS.ActiveMQ.Commands.ActiveMQMessage amqMsg = advisory as Apache.NMS.ActiveMQ.Commands.ActiveMQMessage;
                            if (amqMsg.DataStructure != null)
                            {
                                Apache.NMS.ActiveMQ.Commands.DestinationInfo info = amqMsg.DataStructure as Apache.NMS.ActiveMQ.Commands.DestinationInfo;
                                if (info != null)
                                {
                                    queues.Add(info.Destination.ToString());
                                }
                            }
                        }
                    }
                }).Wait();
                return queues.ToArray();
            }
            /// <summary>
            /// Attempts to return a listing of all available topics on the ActiveMQ server
            /// </summary>
            /// <returns></returns>
            public async Task<string[]> ListAllTopics()
            {
                string advisoryTopic = "ActiveMQ.Advisory.Topic";
                var dest = _session.GetTopic(advisoryTopic);
                List<string> queues = new List<string>();
                await Task.Run(() =>
                {
                    using (IMessageConsumer consumer = _session.CreateConsumer(dest))
                    {
                        IMessage advisory;
                        while ((advisory = consumer.Receive(TimeSpan.FromMilliseconds(500))) != null)
                        {
                            Apache.NMS.ActiveMQ.Commands.ActiveMQMessage amqMsg = advisory as Apache.NMS.ActiveMQ.Commands.ActiveMQMessage;
                            if (amqMsg.DataStructure != null)
                            {
                                Apache.NMS.ActiveMQ.Commands.DestinationInfo info = amqMsg.DataStructure as Apache.NMS.ActiveMQ.Commands.DestinationInfo;
                                if (info != null)
                                {
                                    queues.Add(info.Destination.ToString());
                                }
                            }
                        }
                    }
                });
                return queues.ToArray();
            }
            public async Task<OnMessageEventArgs> PullMessage(string path)
            {
                PrependWithPathDefault(ref path);
                var dest = SessionUtil.GetDestination(_session, path);
                OnMessageEventArgs e = null;
                Task.Run(() =>
                {
                    using (IMessageConsumer consumer = _session.CreateConsumer(dest))
                    {
                        IMessage message;
                        
                        if ((message = consumer.Receive(TimeSpan.FromMilliseconds(500))) != null)
                        {
                            var textMessage = message as ITextMessage;
                            e = new OnMessageEventArgs()
                            {
                                Content = textMessage.Text,
                                Properties = ConvertToDictionary(textMessage.Properties)
                            };
                            
                        }

                    }
                }).Wait();
                return e;
            }

            /// <summary>
            /// Disables the listening event hook
            /// </summary>
            public void StopListening()
            {
                _logger.Trace();
                try
                {
                    _consumer?.Close();
                    _consumer?.Dispose();
                }
                catch (Exception e)
                {
                    _logger.Warn($"Exception while stopping listener: {e.ToString()}");
                }

                _isInternalListeningHooked = false;
                _consumer = null;
                _logger.Info("stopped listening");
            }
            /// <summary>
            /// Stops the reconnection attempts if they're currently running in the background
            /// </summary>
            public async Task CancelReconnectionAttempts(int maxWaitMs)
            {
                _logger.Trace();
                _ctsReconnect?.Cancel();
                _logger.Trace($"cancellation sent");
                int currentWait = 0;
                while (_reconnectionTask != null)
                    try
                    {
                        currentWait += 100;
                        await Task.Delay(100);
                        if (currentWait > maxWaitMs) throw new TimeoutException("Waiting for reconnection task to finish took too long");
                    }
                    catch (Exception e)
                    {
                        throw;
                    }
            }
            private void OnMessageReceived(IMessage message)
            {
                _logger.Trace();
                if (message != null)
                {
                    var textmessage = message as Apache.NMS.ITextMessage;
                    if (textmessage != null)
                    {
                        try
                        {
                            var text = textmessage.Text;
                            if (!string.IsNullOrEmpty(text))
                            {
                                var properties = ConvertToDictionary(textmessage.Properties);
                                Message?.Invoke(this, new OnMessageEventArgs()
                                {
                                    Content = text,
                                    Properties = properties
                                });
                                _logger.Trace($"Message event invoked");
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.Error($"Exception while processing message: {e.ToString()}");
                        }
                    }
                }
            }
            private Dictionary<string,object> ConvertToDictionary(IPrimitiveMap map)
            {
                var properties = new Dictionary<string, object>();
                foreach (var key in map.Keys)
                {
                    properties[(string)key] = map[(string)key];
                }
                return properties;
            }
            private void PrependWithPathDefault(ref string path)
            {
                _logger.Trace(ConnectorLogging.Process((nameof(path), path)));
                if (!path.Contains("://"))
                {
                    _logger.Info($"Path type (queue or topic) not specified. Queue will be used");
                    path = "queue://" + path;
                }
            }
            private async Task<bool> TryReconnectAndReattach()
            {
                bool _isSuccessfullyReconnected = false;
                if (!string.IsNullOrEmpty(_connectionUri) &&
                            !string.IsNullOrEmpty(_connectionUser) &&
                            !string.IsNullOrEmpty(_connectionPassword))
                {
                    try
                    {
                        _logger.Info($"Attempting to reconnect");
                        _isSuccessfullyReconnected = await ConnectAsyncInternal(_connectionUri, _connectionUser, _connectionPassword);
                    }
                    catch (Exception e)
                    {
                        _logger.Error($"Exception on attempting to reconnect: {e.ToString()}");
                        return false;
                    }
                }
                if (!_isSuccessfullyReconnected) return false;
                if (!string.IsNullOrEmpty(_listenToPath))
                {
                    try
                    {
                        if (_isInternalListeningHooked)
                            StopListening();
                        return await ListenAsync(_listenToPath, null, _listenToFilter);
                    }
                    catch (Exception e)
                    {
                        _logger.Error($"Exception on reattaching listener: {e.ToString()}");
                        return false;
                    }
                }
                return true;
            }
            private void InitiateReconnectionLogic()
            {
                _logger.Trace();
                //check if reconnection logic is already running
                bool isAlreadyRunning = false;
                lock (_reconnectionTaskLock)
                {
                    if (_reconnectionTask != null)
                    {
                        _logger.Warn($"Could not initialize reconnection logic. Already running");
                        return;
                    }
                }

                //start it if not already running
                lock (_reconnectionTaskLock)
                {
                    if (_reconnectionTask == null)
                        _reconnectionTask = Task.Run(async () =>
                        {
                            _isConnectionGood = false;
                            ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(false));
                            while (true)
                            {
                                if (_ctsReconnect.IsCancellationRequested)
                                {
                                    _reconnectionTask = null;
                                    return;
                                }
                                if (await TryReconnectAndReattach())
                                {
                                    _isConnectionGood = true;
                                    ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(true));
                                    lock (_reconnectionTaskLock)
                                    {
                                        _reconnectionTask = null;
                                    }
                                    return;
                                }
                                else
                                {
                                    Task.Delay(ReconnectWaitTimeMs).Wait();
                                }
                            }
                        }, (_ctsReconnect = new CancellationTokenSource()).Token);
                }
            }
            private void Connection_ExceptionListener(Exception exception)
            {
                _logger.Trace();
                InitiateReconnectionLogic();
            }
            private async Task<bool> ConnectAsyncInternal(params string[] parameters)
            {
                if (parameters == null || parameters.Length != 3)
                {
                    string logMessage = $"Could not connect to Active MQ, received a bad set of parameters. ";
                    if (parameters == null) logMessage += "No parameters present";
                    else logMessage += $"{parameters.Length} parameters passed, but 3 were expected";
                    _logger.Error(logMessage);
                    return false;
                }
                _logger.Trace(ConnectorLogging.Process(("parameters", "( " + string.Join(", ", parameters) + " )")));
                try
                {
                    _connectionUri = parameters[0];
                    var failoverUri = _connectionUri.Replace($"activemq:", "failover:");
                    _connectionUser = parameters[1];
                    _connectionPassword = parameters[2];
                    var factory = new Apache.NMS.NMSConnectionFactory(_connectionUri);
                    var connection = factory.CreateConnection(_connectionUser, _connectionPassword); //authenticate
                    _logger.Trace($"Connection created");
                    connection.ExceptionListener += Connection_ExceptionListener;
                    connection.ConnectionInterruptedListener += Connection_ConnectionInterruptedListener;
                    connection.ConnectionResumedListener += Connection_ConnectionResumedListener;
                    
                    DisposeCurrentSession();
                    _session = connection.CreateSession();
                    _logger.Trace($"Session created");
                    connection.Start();
                    if (connection.IsStarted && _session != null)
                    {
                        _logger.Info($"Connection established");
                        _isConnectionGood = true;
                        ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(true));
                        return true;
                    }
                }
                catch (Exception e)
                {
                    _logger.Error($"Exception on connect: {e.ToString()}");
                }
                _logger.Error($"Could not establish connection. Please check prior error messages from {nameof(ConnectAsyncInternal)}");
                return false;
            }
            private void DisposeCurrentSession()
            {
                try
                {
                    _session?.Dispose();
                }
                catch (Exception e)
                {
                    _logger.Trace($"Disposing the old session caused an exception: {e.ToString()}");
                }
                finally
                {
                    _session = null;
                }
            }
            private void Connection_ConnectionResumedListener()
            {
                _logger.Warn();
                ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(true));
            }
            private void Connection_ConnectionInterruptedListener()
            {
                _logger.Warn();
                ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(false));
            }
        }
    }
}
