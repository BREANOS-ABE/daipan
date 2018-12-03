//-----------------------------------------------------------------------

// <copyright file="ActorDeployer.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Monday, December 3, 2018 3:34:35 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using BreanosConnectors.ActiveMqConnector;
using BreanosConnectors.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using BreanosConnectors.Kpu.Communication.Common;
using NLog;

namespace CWFStateless
{
    public class ActorDeployer
    {
        private NLog.Logger logger = LogManager.GetCurrentClassLogger();

        private Connector receiveConnector;
        public string ConnectionString { get; set; } = "activemq:tcp://127.0.0.1:61616";
        public string User { get; set; } = "admin";
        public string Password { get; set; } = "admin";

        public readonly string KpuQueueString = "queue://KpuQueue";

        public delegate void ZipFileReceivedEventHandler(object sender, CustomEventArgs a);

        //public event ZipFileReceivedEventHandler zipFileReceivedHandler;        

        public async void RegisterKpuQueue()
        {
            receiveConnector = new Connector();
            bool connectOk = await receiveConnector.ConnectAsync(ConnectionString, User, Password);

            receiveConnector.Message += MessageHandlingMethod;
            receiveConnector.ListenAsync(KpuQueueString).Wait();
        }

        private void RequestKPUId()
        {
            logger.Error("RequestKPUId has called");
        }

        private void DeployKPU(string content)
        {
            logger.Error("DeployKPU has been called.");
        }

        private void MessageHandlingMethod(object sender, OnMessageEventArgs e)
        {
            //BrokerCommands.KPU_DEPLOYMENT,
                    
            if ((e.Properties["ContentType"] as string).CompareTo(BrokerCommands.KPU_DEPLOYMENT) == 0)
            {
                DeployKPU(e.Content);
                //WriteManifest(KpuPath, "HanoiLibrary.HanoiWorkflowState");
                //GenerateZipFile(KpuPath, TempDir);
                //PublishToServiceBus(TempDir + KPURegistration.FileDelimiter + KPURegistration.ZipFileName, ConnectionString, QueueString);
            } else if ((e.Properties["ContentType"] as string).CompareTo(BrokerCommands.REQUESTKPUID) == 0)
            {
                RequestKPUId();
            }
            //BrokerCommands.REQUESTKPUID

        }

        public ActorDeployer()
        {
            logger.Error("CWFStateless has been instanciated");
        }
    }
}
