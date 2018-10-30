//-----------------------------------------------------------------------

// <copyright file="BreanosServiceBusMessages.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Tuesday, October 30, 2018 1:26:47 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using AssistantUtilities;
using BlackboardClassLibraryCore;
using BreanosConnectors.Interface;
using Microsoft.Extensions.Configuration;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BreanosConnectors.Kpu.Communication.Common;


namespace BlackboardClassLibrary.KnowledgeSources
{
    class BreanosServiceBusMessages : KnowledgeSourceBase
    {
        /// <summary>
        /// Logger instance
        /// </summary>
        private static BreanosLogger logger;

        //activemq connector
        BreanosConnectors.ActiveMqConnector.Connector _amqc = new BreanosConnectors.ActiveMqConnector.Connector();

        private static string TOPIC_FOR_STATUS_UPDATE = string.Empty;
        private static string TOPIC_FOR_ANSWER = string.Empty;
       
        /// <summary>
        /// Topic name, set in ctor
        /// </summary>
        public string TopicName { get; set; }

        /// <summary>
        /// Set the subscription name for this
        /// </summary>
        public string SubscriptionName { get; set; }

        string JsonDirectory { get; set; }

        public void OldRouterCode(string content, string contentType)
        {
            try
            {
                if (contentType.CompareTo("MU1") == 0)
                {
                    logger.Trace($"Sending message to {TOPIC_FOR_STATUS_UPDATE}");
                    _amqc.SendAsync(content, TOPIC_FOR_STATUS_UPDATE, contentType, null).Wait();
                }
                else if (contentType.CompareTo("DU1") == 0)
                {
                    logger.Trace($"Sending message to {TOPIC_FOR_ANSWER}");
                    _amqc.SendAsync(content, TOPIC_FOR_ANSWER, contentType, null).Wait();
                }
                else
                {
                    logger.Trace("Type not valid!: " + contentType);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error:" + ex.ToString());
            }
        }

        /// <summary>
        /// Endpoint URL for Service Bus
        /// </summary>
        private void Amqc_LineTopicMessage(object sender, OnMessageEventArgs e)
        {
            logger.Trace(sender.ToString() + "Amqc_LineTopicMessage called");
            //OldRouterCode(e.Content, e.Properties["ContentType"] as string);

            //Changes for Configurator DTO.
            var contentType = e.Properties["ContentType"] as string;

            if (contentType.CompareTo(BrokerCommands.CONFIGURE_ROUTES) == 0)
            {
                var isOk = BreanosConnectors.SerializationHelper.TryUnpack(e.Content, out RoutingRequest registrationRequest);
                if (!isOk)
                {
                    logger.Warn($"Unpack not successfull in Amqc_LineTopicMessage");
                }
                if (_router == null)
                {
                    logger.Debug($"_router was initialized in Amqc_LineTopicMessage");
                    _router = new ConfigurableContentTypeRouter();//new ContentTypeRouter(JsonDirectory);
                }

                (_router as ConfigurableContentTypeRouter).Config(registrationRequest);
            }
            RouterCode(e.Content, e.Properties["ContentType"] as string, e.Properties);
        }

        /// <summary>
        /// 
        /// </summary>
        public void InitOldRouter()
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

            TOPIC_FOR_STATUS_UPDATE = Configuration["Topics:TOPIC_FOR_STATUS_UPDATE"];
            TOPIC_FOR_ANSWER = Configuration["Topics:TOPIC_FOR_ANSWER"];       
        }

        private void RouterCode(string content, string v, IDictionary<string, object> properties)
        {
            try
            {
                string[] queues = _router?.GetQueuesFromContentType(v);

                if (queues == null && _router == null)
                    logger.Debug($"_router has not been initialized");

                if (queues != null)
                {
                    foreach (string queue in queues)
                    {
                        logger.Trace($"Sending message to {queue}");

                        (string, object)[] convParameters = null;
                        if (properties != null && properties.Count != 0)
                        {
                            convParameters = properties.Select(x => (x.Key, x.Value)).ToArray();
                        }
                        else
                        {
                            logger.Trace($"Error property variable is null!");
                        }

                        _amqc.SendAsync(content, queue, v, convParameters).Wait();
                    }
                }
                else
                {
                    logger.Debug("Type not valid!: " + v);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error:" + ex.ToString());
            }
        }

        /// <summary>
        /// Filter Str for ServiceBus
        /// </summary>
        public string FilterStr { get; set; }

        //public string EndpointUrl { get; private set; } = @"activemq:tcp://192.168.30.125:61616";

        ContentTypeRouterBase _router;

        /// <summary>
        /// Breanos Service Bus Connector
        /// </summary>
        //private ServiceBusConnector sbc = new ServiceBusConnector();

        /// <summary>
        /// public ctor
        /// </summary>
        /// <param name="topicName"></param>
        /// <param name="subscriptionName"></param>
        /// <param name="filterString"></param>
        public BreanosServiceBusMessages(string topicName, string subscriptionName, string filterString, string jsonDirectory)
        {
            if (logger == null) logger = BreanosLoggerFactory.DuplicateGet(Blackboard.Blackboard.BlackboardLoggerKey, nameof(BreanosServiceBusMessages));
            TopicName = topicName;
            SubscriptionName = subscriptionName;
           
            FilterStr = filterString;
            JsonDirectory = jsonDirectory;

            InitOldRouter();             
            
            logger.Error("Nach GetQueuesFromContentType");
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
        /// Overload configure method
        /// </summary>
        /// <param name="board"></param>
        public override void Configure(Blackboard.Blackboard board)
        {
            base.Configure(board);
            InitServiceBus().Wait();                  
        }

        private IConfigurationRoot _configuration;
        public IConfigurationRoot Configuration => _configuration;

        /// <summary>
        /// To initialize the service bus
        /// </summary>
        public async Task<bool> InitServiceBus()
        {         
            var Endpoint = Configuration["connection:Endpoint"];
            var User = Configuration["connection:User"];
            var Password = Configuration["connection:Password"];
            if (Endpoint is null || User is null || Password is null)
            {
                logger.Debug("One of configuration manager expected values is null");
                return false;
            }
                          
            //LineTopic
            Task<bool> connectionEstablished = _amqc.ConnectAsync(Endpoint, User, Password);
            bool ok = await connectionEstablished;
            if (!ok)
            {
                logger.Error("Could not establish connection!");
            }

            _amqc.Message += Amqc_LineTopicMessage;

            Task<bool> listenEstablished = _amqc.ListenAsync(TopicName, SubscriptionName/*, filterString*/);
            bool testok = await listenEstablished;
            if (!testok)
            {
                logger.Error("Could not establish listen!");
            }          
            return true;
        }           
    }
}
