//-----------------------------------------------------------------------

// <copyright file="ServiceBusConnector.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Monday, December 3, 2018 3:34:35 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace BreanosServiceBusConnector
{
    /// <summary>
    /// Adapter for the Microsoft Service Bus 1.1
    /// This class wraps the functionality provided via NamespaceManager and MessagingFactory for
    /// receiving from and sending to queues and topics on a service bus.
    /// Messages can be subscribed to with filters and will be forwarded to the subscriber 
    /// as objects of type System.IO.Stream. Additionally, meta information from the 
    /// Brokered Messages is also forwarded so the receiver or another party can then concern themselves
    /// with deserializing the message.
    /// </summary>
    public class ServiceBusConnector
    {
        #region properties
        protected MessagingFactory MessagingFactory { get; set; }
        protected NamespaceManager NamespaceManager { get; set; }
        private SubscriptionClient TopicReceiver { get; set; }
        private QueueClient QueueReceiver { get; set; }
        #endregion
        #region events
        public delegate void OnMessage(object sender, OnMessageEventArgs e);
        public delegate void OnLogMessage(object sender, string message, ServiceBusConnectorLogLevel level);
        public event OnMessage Message;
        public event OnLogMessage Log;
        #endregion
        #region private fields
        private MessageSender _sender;
        private Task _messagePumpTask;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _shouldPumpRun;
        #endregion
        #region c'tor
        public ServiceBusConnector()
        {

        }
        #endregion
        #region public methods
        /// <summary>
        /// Builds the MessagingFactory and NamespaceManager for the ServiceBus
        /// which are used for managing the service bus itself and creating senders and receivers
        /// </summary>
        /// <param name="serviceNamespace"></param>
        /// <param name="serverFullyQualifiedDomainName"></param>
        /// <param name="httpPort"></param>
        /// <param name="tcpPort"></param>
        public async Task<bool> Connect(string serviceNamespace, string serverFullyQualifiedDomainName, int httpPort, int tcpPort)
        {
            Trace($"Namespace: {serviceNamespace}, FQDN: {serverFullyQualifiedDomainName}, httpPort: {httpPort}, tcpPort: {tcpPort}");
            if (MessagingFactory != null && NamespaceManager != null) return true; //only one connection. could use a disconnect method

            ServiceBusConnectionStringBuilder builder = new ServiceBusConnectionStringBuilder();
            builder.ManagementPort = httpPort;
            builder.RuntimePort = tcpPort;
            builder.Endpoints.Add(new UriBuilder()
            {
                Scheme = "sb",
                Host = serverFullyQualifiedDomainName,
                Path = serviceNamespace
            }.Uri);
            builder.StsEndpoints.Add(new UriBuilder()
            {
                Scheme = "https",
                Host = serverFullyQualifiedDomainName,
                Port = httpPort,
                Path = serviceNamespace
            }.Uri);
            var serverAddress = builder.ToString();
            return await Connect(serverAddress);
        }
        /// <summary>
        /// Builds the MessagingFactory and NamespaceManager for the ServiceBus
        /// which are used for managing the service bus itself and creating senders and receivers
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public async Task<bool> Connect(string connectionString)
        {
            Trace($"connectionString: {connectionString}");
            while (MessagingFactory == null || NamespaceManager == null)
            {
                try
                {
                    MessagingFactory = MessagingFactory.CreateFromConnectionString(connectionString);
                    NamespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);
                    return true;
                }
                catch (Exception e)
                {
                    Error(e.ToString());
                    MessagingFactory = null;
                    NamespaceManager = null;
                    await Task.Delay(500);
                }
            }
            return false;
        }
        /// <summary>
        /// Enables listening to a specific path.
        /// Note: if no subscription is defined, the path will be assumed to be for a queue, otherwise topic.
        /// Concerning the filter,
        /// Message properties are user-defined key-value pairs contained in message.Properties. 
        /// For the SBMP thick client, the values are restricted to 
        /// byte, sbyte, char, short, ushort, int, uint, long, ulong, float, double, decimal, bool, Guid, string, Uri, DateTime, DateTimeOffset, and TimeSpan
        /// </summary>
        /// <see cref="https://markheath.net/post/filtered-azure-service-bus-subscriptions/"/>
        /// <seealso cref="https://docs.microsoft.com/en-us/rest/api/servicebus/message-headers-and-properties/"/>
        /// <seealso cref="https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-messaging-sql-filter/"/>
        /// <seealso cref="https://codehollow.com/2016/03/azure-servicebus-filters/"/>
        /// <seealso cref="https://stackoverflow.com/questions/39282348/sqlfilter-based-on-system-properties-in-azure-service-bus-subscription/"/>
        /// <param name="path">The path to the messages' location on the service bus, e.g. "LineMessageQueue" or "MessagesGoHere"</param>
        /// <param name="subscription">your topic subscription's name</param>
        /// <param name="filter">An sql-like-syntax string denoting meta-information stored inside the BrokeredMessage to be filtered on. to filter a numeric value (int, double, etc), use comparators like =, &lt;, &gt;, ... to compare strings, use LIKE and % as wildcard. enclose the string with single quotes (')</param>
        /// <returns></returns>
        public async Task ListenTo(string path, string subscription = null, string filter = null)
        {
            Trace($"path = {path ?? "null"}, subscription = {subscription ?? "null"}, filter = {filter ?? "null"}");
            bool isQueue = subscription == null;
            await EnsureReceiverExists(path, subscription, filter);
            try
            {
                _cancellationTokenSource = new CancellationTokenSource();
                _shouldPumpRun = true;
                if (isQueue) _messagePumpTask = QueueMessagePump();
                else _messagePumpTask = TopicMessagePump();
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// Message sending method. Attempts to send an object onto the servicebus.
        /// </summary>
        /// <param name="o">The string to send. </param>
        /// <param name="path">The path to the messages' location on the service bus</param>
        /// <param name="contentType">the value for the brokered message's ContentType property</param>
        /// <param name="properties">a collection of keys with values, useful for filtering messages on the receiving end</param>
        /// <returns></returns>
        public async Task Send(string o, string path, string contentType, IDictionary<string, object> properties = null)
        {
            Trace($"path: {path}, predefined contentType {contentType}");
            try
            {
                BrokeredMessage bm = new BrokeredMessage(o);
                bm.ContentType = contentType;
                if (properties != null)
                {
                    foreach (var kv in properties)
                    {
                        bm.Properties.Add(kv);
                    }
                }
                await EnsureSend(bm, path);
            }
            catch (Exception e)
            {
                Error($"Send with predefined contentType caused an exception: {e.ToString()}");
            }

        }
        /// <summary>
        /// Stops the Listening Loop, either after its current iteration or, if forceStop is set, before.
        /// </summary>
        /// <param name="forceStop"></param>
        public void StopListening(bool forceStop = false)
        {
            Trace($"forceStop: {forceStop}");
            if (forceStop) _cancellationTokenSource.Cancel();
            _shouldPumpRun = false;
        }
        #endregion
        #region private methods
        private void LogMessage(ServiceBusConnectorLogLevel level, string message = "", [CallerMemberName]string method = "")
        {
            Log?.Invoke(this, $"{method}() -> {message ?? ""}", level);
        }
        private void Error(string message = "", [CallerMemberName]string method = "")
        {
            LogMessage(ServiceBusConnectorLogLevel.Error, message, method);
        }
        private void Trace(string message = "", [CallerMemberName]string method = "")
        {
            LogMessage(ServiceBusConnectorLogLevel.Trace, message, method);
        }
        private void Info(string message = "", [CallerMemberName]string method = "")
        {
            LogMessage(ServiceBusConnectorLogLevel.Info, message, method);
        }
        private async Task EnsureSend(BrokeredMessage message, string path)
        {
            Trace();
            while (true)
            {
                try
                {
                    //first check if sender exists but for a different destination
                    if (_sender != null && !_sender.Path.Equals(path))
                    {
                        try
                        {
                            _sender.Close();
                        }
                        catch (Exception e)
                        {
                            Error(e.ToString(), nameof(EnsureSend));
                            _sender.Abort();
                            Info("Called sender abort", nameof(EnsureSend));
                        }
                        finally
                        {
                            _sender = null;
                        }
                    }
                    //second check if sender is null or closed
                    if (_sender == null || _sender.IsClosed)
                    {
                        Trace("Creating sender", nameof(EnsureSend));
                        _sender = MessagingFactory.CreateMessageSender(path);
                    }

                    Trace("Trying to send now");
                    //try to send the message and break from the endless loop
                    await _sender.SendAsync(message);
                    break;
                }
                catch (Exception e)
                {
                    //kill the messenger if it acted up.
                    Error($"There was an exception while trying to send a message: {e.ToString()}");
                    _sender.Abort();
                    Info("Called sender abort", nameof(EnsureSend));
                    _sender = null;
                }
            }
        }
        private async Task EnsureReceiverExists(string path, string subscription, string filter = null)
        {
            Trace();
            try
            {
                switch (string.IsNullOrEmpty(subscription))
                {
                    case true:
                        if (!await NamespaceManager.QueueExistsAsync(path))
                        {
                            await NamespaceManager.CreateQueueAsync(path);
                        }
                        QueueReceiver = MessagingFactory.CreateQueueClient(path, ReceiveMode.ReceiveAndDelete);

                        break;
                    case false:
                        if (!await NamespaceManager.TopicExistsAsync(path))
                        {
                            await NamespaceManager.CreateTopicAsync(path);
                        }
                        //recreate subscription if necessary
                        if (await NamespaceManager.SubscriptionExistsAsync(path, subscription))
                        {
                            await NamespaceManager.DeleteSubscriptionAsync(path, subscription);
                        }
                        if (!await NamespaceManager.SubscriptionExistsAsync(path, subscription))
                        {
                            if (!string.IsNullOrEmpty(filter))
                            {
                                SqlFilter sf = new SqlFilter(filter);
                                await NamespaceManager.CreateSubscriptionAsync(path, subscription, sf);
                            }
                            else
                            {
                                await NamespaceManager.CreateSubscriptionAsync(path, subscription);
                            }
                        }
                        TopicReceiver = MessagingFactory.CreateSubscriptionClient(path, subscription, ReceiveMode.ReceiveAndDelete);
                        break;
                }
            }
            catch (Exception e)
            {
                Error(e.ToString());
            }
        }
        private async Task TopicMessagePump()
        {
            Trace();
            if (TopicReceiver == null)
            {
                Info($"TopicReceiver was not set up correctly. Message pump cannot be started. Please look for previous error messages from {nameof(EnsureReceiverExists)} and check the path, subscription, filter and your rights to management for the Service Bus");
                return;
            }
            while (_shouldPumpRun)
            {
                try
                {
                    if (_cancellationTokenSource.Token.IsCancellationRequested) throw new TaskCanceledException();
                    await TopicReceiver.ReceiveBatchAsync(20, TimeSpan.FromMilliseconds(200))?.ContinueWith(ForwardMessage);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception e)
                {
                    Error($"TopicMessagePump execution caused an exception: {e.ToString()}");
                }
            }
        }
        private async Task QueueMessagePump()
        {
            Trace();
            if (QueueReceiver == null)
            {
                Info($"QueueReceiver was not set up correctly. Message pump cannot be started. Please look for previous error messages from {nameof(EnsureReceiverExists)} and check the path, and your rights to management for the Service Bus");
                return;
            }
            while (_shouldPumpRun)
            {
                try
                {
                    if (_cancellationTokenSource.Token.IsCancellationRequested) throw new TaskCanceledException();
                    await QueueReceiver.ReceiveBatchAsync(20, TimeSpan.FromMilliseconds(200))?.ContinueWith(ForwardMessage);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception e)
                {
                    Error($"QueueMessagePump execution caused an exception: {e.ToString()}");
                }
            }
        }
        private async Task ForwardMessage(Task<IEnumerable<BrokeredMessage>> messagesTask)
        {
            var messages = await messagesTask;
            if (messages != null && messages.Count() > 0)
                Trace($"{messages.Count()} recieved");
            foreach (var message in messages)
            {
                if (message != null)
                {
                    string body = null;
                    try
                    {
                        body = message.GetBody<string>();
                    }
                    catch (Exception e)
                    {
                        Error($"GetBody caused an exception: {e.ToString()}");
                    }
                    try
                    {
                        Message?.Invoke(this, new OnMessageEventArgs()
                        {
                            ContentType = message.ContentType,
                            Content = body,
                            Properties = message.Properties
                        });
                    }
                    catch (Exception e)
                    {
                        Error($"Message.Invoke caused an exception: {e.ToString()}");
                    }
                }
            }
        }
        #endregion
    }
}
