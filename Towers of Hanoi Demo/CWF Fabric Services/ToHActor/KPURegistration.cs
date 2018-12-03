//-----------------------------------------------------------------------

// <copyright file="KPURegistration.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Monday, December 3, 2018 3:34:35 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using BreanosConnectors;
using BreanosConnectors.ActiveMqConnector;
using BreanosConnectors.Interface;
using BreanosConnectors.Kpu.Communication.Common;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;
using File = BreanosConnectors.Kpu.Communication.Common.File;

namespace ToHActor
{
    public class KPURegistration : INotifyPropertyChanged
    {
        private NLog.Logger logger = LogManager.GetCurrentClassLogger();

        public string ConnectionString { get; set; } = "activemq:tcp://192.168.50.1:61616";
        public string User { get; set; } = "admin";
        public string Password { get; set; } = "admin";

        public readonly string QueueString = "queue://LineTopic_1";

        public readonly string KpuQueueString = "queue://KpuQueue";

        public readonly string ManagementQueueString = "queue://ManagementQueue";
        public readonly string AssistantQueueString = "queue://AssistantQueue";

        string KpuPath { get; set; }

        string TempDir { get; set; }

        static public readonly string ZipFileName = "out.zip";

        static public readonly string FileDelimiter = "\\";
       
        private Connector receiveConnector;

        public event PropertyChangedEventHandler PropertyChanged;

        public KPURegistration(string kpuPath, string tempDir, string endpoint, string user, string password)
        {
            KpuPath = kpuPath;
            TempDir = tempDir;

            ConnectionString = endpoint;
            User = user;
            Password = password;

            //Init receive queue
            RegisterKpuQueue();

            InitConfigureRoutes();
        }

        public async void InitConfigureRoutes()
        {
            logger.Debug($"InitConfigureRoutes.");

            var registerForMessagesAtBlackboardRequest = new RoutingRequest()
            {
                Id = "ToHActor",
                Path = KpuQueueString,
                ContentTypes = new string[]
              {
                    BrokerCommands.EXECUTE_REQUEST,
                    BrokerCommands.PACKAGE_REQUEST,
              },
            };

            var registerPackage = BreanosConnectors.SerializationHelper.Pack(registerForMessagesAtBlackboardRequest);
            await receiveConnector.SendAsync(registerPackage, QueueString, BrokerCommands.CONFIGURE_ROUTES);

            logger.Trace($"Registration with Blackboard for {QueueString} complete.");

            var registerForMessagesAtBlackboardRequest2 = new RoutingRequest()
            {
                Id = "ToHActor",
                Path = AssistantQueueString,
                ContentTypes = new string[]
             {                    
                 BrokerCommands.KPU_DEPLOYMENT,
                 BrokerCommands.REQUESTKPUID,
                 //BrokerCommands.TELLKPUID
             },
            };

            var registerPackage2 = BreanosConnectors.SerializationHelper.Pack(registerForMessagesAtBlackboardRequest2);
            await receiveConnector.SendAsync(registerPackage2, QueueString, BrokerCommands.CONFIGURE_ROUTES);

            var registerForMessagesAtBlackboardRequest3 = new RoutingRequest()
            {
                Id = "ToHActor",
                Path = ManagementQueueString,
                ContentTypes = new string[]
           {                    
                 //BrokerCommands.KPU_DEPLOYMENT,
                 //BrokerCommands.REQUESTKPUID,
                 BrokerCommands.TELLKPUID
           },
            };

            var registerPackage3 = BreanosConnectors.SerializationHelper.Pack(registerForMessagesAtBlackboardRequest2);
            await receiveConnector.SendAsync(registerPackage3, QueueString, BrokerCommands.CONFIGURE_ROUTES);

            logger.Trace($"Registration with Blackboard for {QueueString} complete.");
        }

        public async void RegisterKpuQueue()
        {
            receiveConnector = new Connector();
            bool connectOk = await receiveConnector.ConnectAsync(ConnectionString, User, Password);         

            receiveConnector.Message += KpuMessageHandlingMethod;
            receiveConnector.ListenAsync(KpuQueueString).Wait();
        }

        private void KpuMessageHandlingMethod(object sender, OnMessageEventArgs e)
        {
            if (e.Properties["ContentType"] == null) return;

            string s = e.Content;
            IDictionary<string, object> obj = e.Properties;

            if ((e.Properties["ContentType"] as string).CompareTo(BrokerCommands.PACKAGE_REQUEST) == 0)
            {
                WriteManifest(KpuPath, "HanoiLibrary.HanoiWorkflowState");
                GenerateZipFile(KpuPath, TempDir);
                PublishToServiceBus(TempDir + KPURegistration.FileDelimiter + KPURegistration.ZipFileName, ConnectionString, QueueString);
            }
            else if ((e.Properties["ContentType"] as string).CompareTo(BrokerCommands.EXECUTE_REQUEST) == 0)
            {                
                if (SerializationHelper.TryUnpack(e.Content, out ExecuteRequest executeRequestDto))
                {
                    logger.Debug($"ExecuteRequest {executeRequestDto}");
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(executeRequestDto.Action));
                }
            }
        }

        public void WriteManifest(string FileName, string initializationClassName)
        {           
            using (var writer = new System.IO.StreamWriter(FileName + "\\" + "Manifest.ini"))
            {
                var serializer = new XmlSerializer(typeof(Manifest));

                Manifest manifest = new Manifest()
                {
                    ModelClass = initializationClassName,
                    Assemblies = new List<Assembly> { new Assembly { Value = "HanoiLibraryStandard.dll" } },
                    Views = new List<View> { new View { Value = "Hanoi.xaml" } },
                    Files = new List<File> { new File { Value = "Hanoi.gif" } },
                };

                serializer.Serialize(writer, manifest);
                writer.Flush();
            }         
        }

        public void GenerateZipFile(string SourceFilesPath, string TempFilesPath)
        {
            string startPath = SourceFilesPath;
            string zipPath = TempFilesPath + FileDelimiter + ZipFileName;

            if (System.IO.File.Exists(zipPath) == true)
            {
                System.IO.File.Delete(zipPath);
            }

            ZipFile.CreateFromDirectory(startPath, zipPath);           
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static string GetMenuString()
        {
            var someString = String.Join(
                Environment.NewLine,
                @"<?xml version=""1.0"" encoding=""utf-8""?>",
                @"<MenuGroup xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" GroupPriority=""1"" PositionAnchor=""1"">",
                @"  <MenuItem ItemIdentifier=""HanoiMaster"" TextResourceId=""txt.HanoiMaster"" IconXamlGeometry=""icon.HanoiMaster"" PermissionIdentifier=""Hanoi.view"">",
                "   <Children>",
                @"      <MenuItem ItemIdentifier = ""HanoiRestart"" TextResourceId = ""kpu.Hanoi.RestartHanoi"" Command = ""RestartKpu"" IconXamlGeometry = ""icon.HanoiRestart"" PermissionIdentifier = ""Hanoi.restart"" />",
                @"      <MenuItem ItemIdentifier = ""HanoiStop"" TextResourceId = ""kpu.Hanoi.StopHanoi"" Command = ""StopKpu"" IconXamlGeometry = ""icon.HanoiStop"" PermissionIdentifier = ""Hanoi.stop"" />",
                @"      <MenuItem ItemIdentifier = ""HanoiStart"" TextResourceId = ""kpu.Hanoi.StartHanoi"" Command = ""StartKpu"" IconXamlGeometry = ""icon.HanoiStart"" PermissionIdentifier = ""Hanoi.start"" />",
                @"      <MenuItem ItemIdentifier = ""Management"" TextResourceId = ""Administer"" PermissionIdentifier = ""Hanoi.manage"" >",
                "           <Children>",
                @"              <MenuItem ItemIdentifier=""Uptime"" Command=""ViewUptime"" IconXamlGeometry=""B32 T9 L12 20"" PermissionIdentifier=""Hanoi.manage.uptime.view"" />",
                @"              <MenuItem ItemIdentifier = ""Downtimes"" PermissionIdentifier = ""Hanoi.manage.downtime"" >",
                @"                  <Children>",
                @"                      <MenuItem ItemIdentifier = ""PlannedDowntimes"" TextResourceId = ""x.PlannedDowntimes"" Command = ""nav.Downtimes"" CommandParameter = ""t=planned"" IconXamlGeometry = ""L20 10 B30 30"" PermissionIdentifier = ""Hanoi.manage.downtimes.view"" />",
                @"                      <MenuItem ItemIdentifier = ""UnplannedDowntimes"" TextResourceId = ""x.UnplannedDowntimes"" Command = ""nav.Downtimes"" CommandParameter = ""t=unplanned"" IconXamlGeometry = ""L20 10 B30 30"" PermissionIdentifier = ""Hanoi.manage.downtimes.view"" />",
                @"                      <MenuItem ItemIdentifier = ""AllDowntimes"" TextResourceId = ""x.AllDowntimes"" Command = ""nav.Downtimes"" CommandParameter = ""t=all"" IconXamlGeometry = ""L20 10 B30 30"" PermissionIdentifier = ""Hanoi.manage.downtimes.view"" />",
                @"                  </Children>",
                @"              </MenuItem>",
                @"          </Children>",
                @"      </MenuItem>",
                @"  </Children>",
                @"  </MenuItem>",
                @"  <MenuItem ItemIdentifier = ""HanoiStop2"" TextResourceId = ""kpu.Hanoi.StopHanoi"" Command = ""StopKpu"" IconXamlGeometry = ""B32 T9 L12 20"" PermissionIdentifier = ""Hanoi.stop"" />",
                @"  <MenuItem ItemIdentifier = ""HanoiStart2"" TextResourceId = ""kpu.Hanoi.StartHanoi"" Command = ""StartKpu"" IconXamlGeometry = ""B32 T9 L12 20"" PermissionIdentifier = ""Hanoi.start"" />",
                @"</MenuGroup>");

            return someString;
        }

        public void RegisterKPU(string connectionString, string queueString)
        {
            logger.Debug($"In RegisterKPU Parameters ConnectionString:{connectionString}, QueueString:{queueString}");

            var connector = new Connector();
            connector.ConnectAsync(connectionString, User, Password).Wait();

            KpuRegistrationRequest kpuRegistration = new KpuRegistrationRequest();
            kpuRegistration.KpuId = ToHActor.KPU_ID;

            KpuPermissionRequest[] request = new KpuPermissionRequest[2];
            request[0] = new KpuPermissionRequest()
            {
                PermissionIdentifier = "view"
            };
            request[1] = new KpuPermissionRequest()
            {
                PermissionIdentifier = "subscribe"
            };
            kpuRegistration.PermissionRequests = request;
            kpuRegistration.MenuXmlString = GetMenuString();

            string packedStr = SerializationHelper.Pack(kpuRegistration);
            connector.SendAsync(packedStr, queueString, BrokerCommands.KPU_REGISTRATION, null).Wait();
        }


        public async void PublishToServiceBus(string ZipFilePath, string connectionString, string queueString)
        {
            byte[] array = System.IO.File.ReadAllBytes(ZipFilePath);
            string packedSerialized = SerializationHelper.Pack(array);

            var connector = new Connector();
            bool b = await connector.ConnectAsync(connectionString, User, Password);
            connector.SendAsync(packedSerialized, queueString, BrokerCommands.PACKAGE, new (string, object)[]{ ("KpuId", ToHActor.KPU_ID) }).Wait();
        }
    }
}
