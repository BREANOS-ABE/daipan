//-----------------------------------------------------------------------

// <copyright file="ServiceBusMessages.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Monday, December 3, 2018 3:34:35 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using AssistantUtilities;
using BIKSClassLibrary;
using BlackboardClassLibrary.Commands;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace BlackboardClassLibrary.KnowledgeSources
{
    /// <summary>
    /// KU zum verarbeiten von ServiceBus Nachrichten
    /// </summary>
    public class ServiceBusMessages : KnowledgeSourceBase
    {

        /// <summary>
        /// Event für asynchrone Methode
        /// </summary>
        ManualResetEvent CompletedResetEvent = new ManualResetEvent(false);

        /// <summary>
        /// Zum Empfang von Service Bus Nachrichten
        /// </summary>
        private SubscriptionClient Client { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public class OnMessageEventArgs : EventArgs
        {
            /// <summary>
            /// 
            /// </summary>
            public Type ContentType { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public object Content { get; set; }
        }

        /// <summary>
        /// Knowledge Type
        /// </summary>
        public override KnowledgeSourceType KSType => KnowledgeSourceType.ServiceBusHandler;

        /// <summary>
        /// High priority
        /// </summary>
        public override KnowledgeSourcePriority Priority => KnowledgeSourcePriority.High;

        /// <summary>
        /// Topic Name
        /// </summary>
        private string TopicName { get; set; } = "MachineUnitUp";


        /// <summary>
        /// Supscription Name
        /// </summary>
        private string SubscriptionName { get; set; } = "SubscriptionTopicMachineUnitUp";

        /// <summary>
        /// 
        /// </summary>
        protected MessagingFactory MessagingFactory { get; set; }

        /// <summary>
        /// 
        /// </summary>
        protected NamespaceManager NamespaceManager { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void OnMessageDelegate(object sender, OnMessageEventArgs e);

        /// <summary>
        /// 
        /// </summary>
        public event OnMessageDelegate Message;

        /// <summary>
        /// Logger instance
        /// </summary>
        //private static Logger logger = LogManager.GetCurrentClassLogger();        
        static BreanosLogger logger;

        public ServiceBusMessages()
        {
            if (logger == null) logger = BreanosLoggerFactory.DuplicateGet(Blackboard.Blackboard.BlackboardLoggerKey, nameof(ServiceBusMessages));
        }
        /// <summary>
        /// Service Bus Helper
        /// </summary>
        ~ServiceBusMessages()
        {
            CompletedResetEvent.Reset();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ex"></param>
        public void LogErrors(object sender, ExceptionReceivedEventArgs ex)
        {
            logger.Error(sender.ToString(), ex.ToString());
        }

        /// <summary>
        /// Service Bus Helper
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="t"></param>
        /// <param name="o"></param>
        /// <returns></returns>
        private bool TryDeserialization(Stream stream, Type t, out object o)
        {
            try
            {
                DataContractSerializer serializer = new DataContractSerializer(t);
                XmlDictionaryReader xmlReader = XmlDictionaryReader.CreateBinaryReader(stream, XmlDictionaryReaderQuotas.Max);
                o = serializer.ReadObject(xmlReader);
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                o = null;
                return false;
            }
            return (o != null);
        }

        /// <summary>
        /// ReceiveMessagesFromSubscription installs the receive handler.
        /// </summary>
        /// <param name="topicName"></param>
        /// <param name="subscriptionFilter"></param>
        public void ReceiveMessagesFromSubscription(string topicName, string subscriptionFilter)
        {
            try
            {
                Task listener = Task.Factory.StartNew(() =>
                {
                    // You only need to set up the below once. 

                    Client = SubscriptionClient.Create(TopicName, SubscriptionName);

                    var options = new OnMessageOptions();

                    options.AutoComplete = false;

                    options.ExceptionReceived += LogErrors;
                    try
                    {
                        Client.OnMessage((message) =>
                        {
                            try
                            {
                                Type messageType = Type.GetType(message.ContentType);
                                Stream stream = message.GetBody<Stream>();

                                if (TryDeserialization(stream, messageType, out object o))
                                {
                                    Message?.Invoke(this, new OnMessageEventArgs()
                                    {
                                        Content = o,
                                        ContentType = messageType
                                    });
                                }
                                message.Complete(); // Remove message from subscription.
                            }
                            catch (Exception ex)
                            {
                                logger.Trace(ex.Message);
                                message.Abandon(); // Failed. Leave the message for retry or max deliveries is exceeded and it dead letters.
                            }

                        }, options);
                    }
                    catch (Exception exe)
                    {
                        logger.Error(exe.Message);
                    }

                    CompletedResetEvent.WaitOne();
                });
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
            }
        }

        /// <summary>
        /// Service Bus helper
        /// </summary>
        /// <returns></returns>
        private bool CreateSubscription()
        {
            try
            {
                MessagingFactory = MessagingFactory.Create();

                NamespaceManager = NamespaceManager.Create();
            
                if (!NamespaceManager.SubscriptionExists(TopicName, SubscriptionName))
                {
                    logger.Trace("Subscription does not exist, create one.");
                    NamespaceManager.CreateSubscription(TopicName, SubscriptionName);
                }
                Client = MessagingFactory.CreateSubscriptionClient(TopicName, SubscriptionName, ReceiveMode.PeekLock);
                logger.Debug("Service Bus is ready");
            }
            catch (Exception e)
            {
                logger.Trace("Init exception caught {0}", e.ToString());
                return false;
            }
          
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void HandleServiceBusMessage(object sender, OnMessageEventArgs e)
        {
            logger.Debug("HandleServiceBusMessage called");
            BlackboardObject blackboardObject = (BlackboardObject)Convert.ChangeType(e.Content, e.ContentType);

            logger.Trace("ClientId: " + blackboardObject.ClientId);
            logger.Trace("ServiceName: " + blackboardObject.ServiceName);
            logger.Trace("BlackboardData: " + blackboardObject.BlackboardData);

            InitSFAObject initSFAObject = new InitSFAObject(blackboardObject);
            blackboard.AddObject(null, ObjectType.InitService, initSFAObject);                           
        }

        /// <summary>
        /// To initialize the service bus
        /// </summary>
        public bool InitServiceBus()
        {
            if (!CreateSubscription())
            {
                logger.Debug("CreateSubscription returned with false");
                return false;
            }

            Message += HandleServiceBusMessage;
            ReceiveMessagesFromSubscription(TopicName, SubscriptionName);
            logger.Debug("Init Service Bus returns with true");
            return true;
        }

        /// <summary>
        /// Overload configure method
        /// </summary>
        /// <param name="board"></param>
        public override void Configure(Blackboard.Blackboard board)
        {            
            base.Configure(board);
            InitServiceBus();    
        }
    }
}
