//-----------------------------------------------------------------------

// <copyright file="BreanosServiceBusMessages.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Monday, December 3, 2018 3:34:35 PM</date>
// </copyright>

//-----------------------------------------------------------------------


using AssistantInternalInterfaces;
using AssistantUtilities;
using BreanosServiceBusConnector;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackboardClassLibrary.KnowledgeSources
{
    class BreanosServiceBusMessages : KnowledgeSourceBase
    {
        /// <summary>
        /// Logger instance
        /// </summary>
        //private static Logger logger = LogManager.GetCurrentClassLogger();
        private static BreanosLogger logger;

        //private ICoreService _CoreService;
        //private const string _coreServiceUri = "fabric:/Assistant/CoreService";

        private static string TOPIC_FOR_STATUS_UPDATE = "DataMartTopic_1";
        private static string TOPIC_FOR_ANSWER = "MachineUnitTopic_1";
       
        /// <summary>
        /// Topic name, set in ctor
        /// </summary>
        public string TopicName { get; set; }

        /// <summary>
        /// Set the subscription name for this
        /// </summary>
        public string SubscriptionName { get; set; }

        /// <summary>
        /// Endpoint URL for Service Bus
        /// </summary>
        //public string EndpointUrl { get; private set; } = @"Endpoint=sb://bre-dev02.breanos.local/BreanosSB;StsEndpoint=https://bre-dev02.breanos.local:9355/BreanosSB;RuntimePort=9354;ManagementPort=9355;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=lYVJowZHY5yNBIVHXertZw4wV1hEW9/fIigG1MUZ4fQ=";
        public string EndpointUrl { get; private set; } = @"Endpoint=sb://spf-sb01/BreanosSb;StsEndpoint=https://spf-sb01:9355/BreanosSb;RuntimePort=9354;ManagementPort=9355;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=ZYxXmuY1ozN11R4MfbsS0R4VSGG1BsTLHjNanFj5zvg=";
        /// <summary>
        /// Filter Str for ServiceBus
        /// </summary>
        public string FilterStr { get; set; }

        /// <summary>
        /// Breanos Service Bus Connector
        /// </summary>
        private ServiceBusConnector sbc = new ServiceBusConnector();

        /// <summary>
        /// public ctor
        /// </summary>
        /// <param name="topicName"></param>
        /// <param name="subscriptionName"></param>
        /// <param name="filterString"></param>
        public BreanosServiceBusMessages(string topicName, string subscriptionName, string filterString)
        {
            if (logger == null) logger = BreanosLoggerFactory.DuplicateGet(Blackboard.Blackboard.BlackboardLoggerKey, nameof(BreanosServiceBusMessages));
            TopicName = topicName;
            SubscriptionName = subscriptionName;
            FilterStr = filterString;
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

        private void Sbc_Log(object sender, string message, ServiceBusConnectorLogLevel level)
        {
            switch (level)
            {
                case ServiceBusConnectorLogLevel.Error:
                    logger.Error(sender.ToString() + message.ToString());
                break;
                case ServiceBusConnectorLogLevel.Info:
                    logger.Info(sender.ToString() + message.ToString());
                break;
                case ServiceBusConnectorLogLevel.Trace:
                    logger.Trace(sender.ToString() + message.ToString());
                break;
                default:
                    logger.Trace(sender.ToString() + message.ToString());
                break;
            }
        }
       
        /// <summary>
        /// LineTopic
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Sbc_Message(object sender, OnMessageEventArgs e)
        {
            logger.Trace(sender.ToString() + "Sbc_Message called");
           
            try
            {
                if (e.ContentType.CompareTo("MU1") == 0)
                {
                    sbc.Send(e.Content, TOPIC_FOR_STATUS_UPDATE, e.ContentType, e.Properties).Wait();                  
                }
                else if (e.ContentType.CompareTo("DU1") == 0)
                {
                    sbc.Send(e.Content, TOPIC_FOR_ANSWER, e.ContentType, e.Properties).Wait();                    
                }
                else
                {
                    logger.Trace(sender.ToString() + "Type not valid!: " + e.ContentType);
                }
            }
            catch (Exception ex)
            {
                logger.Error(sender.ToString() + "Error:" + ex.ToString());
            }                     
        }
        /// <summary>
        /// To initialize the service bus
        /// </summary>
        public async Task<bool> InitServiceBus()
        {                  
            var fqdn = ConfigurationManager.AppSettings["SB.Fqdn"];
            var ns = ConfigurationManager.AppSettings["SB.Ns"];
            var hp = ConfigurationManager.AppSettings["SB.HttpPort"];
            var tp = ConfigurationManager.AppSettings["SB.TcpPort"];           

            if (fqdn is null || ns is null || hp is null || tp is null)
            {
                logger.Debug("One of configuration manager expected values is null");
                return false;
            }
            var hpi = int.Parse(hp);
            var tpi = int.Parse(tp);
            sbc.Log += Sbc_Log;
            bool ret = await sbc.Connect(EndpointUrl);
            
            logger.Debug($"sbc.Connect returned {ret}");
            sbc.Message += Sbc_Message;
            await sbc.ListenTo(TopicName, SubscriptionName/*, filterString*/);

            //_CoreService = ServiceProxy.Create<ICoreService>(new Uri("fabric:/Assistant/CoreService"));
            
            return true;
        }           
    }
}
