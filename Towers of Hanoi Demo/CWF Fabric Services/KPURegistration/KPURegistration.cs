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
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace ToHActor
{
    public class KPURegistration
    {
        private NLog.Logger logger = LogManager.GetCurrentClassLogger();

        public string ConnectionString { get; set; } = "activemq:tcp://192.168.50.1:61616";
        public string User { get; set; } = "admin";
        public string Password { get; set; } = "admin";

        public readonly string QueueString = "queue://AssistantQueue";

        public readonly string KpuQueueString = "queue://KpuQueue";

        string KpuPath { get; set; }

        string TempDir { get; set; }

        static public readonly string ZipFileName = "out.zip";

        static public readonly string FileDelimiter = "\\";

        //public static class BrockerCommands
        //{
        //    public const string KPU_REGISTRATION = "KpuRegistration";
        //    public const string PACKAGE = "Package";
        //    public const string PACKAGE_REQUEST = "PackageRequest";
        //    public const string EXECUTE_REQUEST = "ExecuteRequest";
        //}

        private Connector receiveConnector;
        public KPURegistration(string kpuPath, string tempDir, string endpoint, string user, string password) 
        {
            KpuPath = kpuPath;
            TempDir = tempDir;

            ConnectionString = endpoint;
            User = user;
            Password = password;

            //Init receive queue
            RegisterKpuQueue();
        }

        public async void RegisterKpuQueue()
        {
            receiveConnector = new Connector();
            bool connectOk = await receiveConnector.ConnectAsync(ConnectionString, User, Password);         

            receiveConnector.Message += MessageHandlingMethod;
            receiveConnector.ListenAsync(KpuQueueString).Wait();
        }

        private void MessageHandlingMethod(object sender, OnMessageEventArgs e)
        {
            if (e.Properties["ContentType"] == null) return;

            string s = e.Content;
            IDictionary<string, object> obj = e.Properties;

            if ((e.Properties["ContentType"] as string).CompareTo(BrockerCommands.PACKAGE_REQUEST) == 0)
            {
                WriteManifest(KpuPath, "HanoiLibrary.HanoiWorkflowState");
                GenerateZipFile(KpuPath, TempDir);
                PublishToServiceBus(TempDir + KPURegistration.FileDelimiter + KPURegistration.ZipFileName, ConnectionString, QueueString);
            }
            else if ((e.Properties["ContentType"] as string).CompareTo(BrockerCommands.EXECUTE_REQUEST) == 0)
            {
                ExecuteRequest executeRequestDto;
                if (SerializationHelper.TryDeserialize(e.Content, out executeRequestDto))
                {
                    logger.Info($"ExecuteRequest {executeRequestDto}");
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

        public void RegisterKPU(string connectionString, string queueString)
        {            
            var connector = new Connector();
            connector.ConnectAsync(connectionString, User, Password).Wait();
            connector.SendAsync("lalal", queueString, BrockerCommands.KPU_REGISTRATION, new (string, object)[] { ("KpuId", "Hanoi") }).Wait();
        }


        public async void PublishToServiceBus(string ZipFilePath, string connectionString, string queueString)
        {
            byte[] array = System.IO.File.ReadAllBytes(ZipFilePath);
            string serialized = SerializationHelper.Serialize(array);

            var connector = new Connector();
            bool b = await connector.ConnectAsync(connectionString, User, Password);
            connector.SendAsync(serialized, queueString, BrockerCommands.PACKAGE, new (string, object)[]{ ("KpuId", "Hanoi")}).Wait();
        }
    }
}
