//-----------------------------------------------------------------------

// <copyright file="Program.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Monday, December 3, 2018 3:34:35 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using BreanosServiceBusConnector;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PoC_SB_Side2
{
    class Program
    {
        
        static Task t;
        const string Topic_Name = "DataMartTopic_1";//"PoC_SB_Topic";
        const string Subscription_Name = "Sub1";
        static void Main(string[] args)
        {
            t = new Program().Run();
            Console.ReadLine();
        }
        private async Task Run()
        {
            ServiceBusConnector sbc = new ServiceBusConnector();
            /*var fqdn = ConfigurationManager.AppSettings["SB.Fqdn"];
            var ns = ConfigurationManager.AppSettings["SB.Ns"];
            var hp = ConfigurationManager.AppSettings["SB.HttpPort"];
            var tp = ConfigurationManager.AppSettings["SB.TcpPort"];
            var hpi = int.Parse(hp);
            var tpi = int.Parse(tp);*/
            sbc.Log += Sbc_Log;
            //await sbc.Connect(ns,fqdn,hpi,tpi);
            await sbc.Connect("Endpoint=sb://bre-dev02.breanos.local/BreanosSB;StsEndpoint=https://bre-dev02.breanos.local:9355/BreanosSB;RuntimePort=9354;ManagementPort=9355;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=lYVJowZHY5yNBIVHXertZw4wV1hEW9/fIigG1MUZ4fQ=");
            sbc.Message += Sbc_Message;
            string filterString = null;
            /*Console.WriteLine("Please enter a number to select a filter");
            Console.WriteLine("1: MetaLevel = 9000");
            Console.WriteLine("2: MetaLevel > 9000");
            Console.WriteLine("3: MetaLevel < 9000");
            //RandomValue
            Console.WriteLine("4: RandomValue LIKE 'Wooo%'");
            Console.WriteLine("5: Define it yourself");
            var selection = Console.ReadLine();
            string filterString = null;
            switch (selection)
            {
                case "1":
                    filterString = "MetaLevel = 9000";
                    break;
                case "2":
                    filterString = "MetaLevel > 9000";
                    break;
                case "3":
                    filterString = "MetaLevel < 9000";
                    break;
                case "4":
                    filterString = "RandomValue LIKE 'Wooo%'";
                    break;
                case "5":
                    Console.WriteLine("Please enter filter string");
                    filterString = Console.ReadLine();
                    break;
                default:
                    break;
            }
            Console.WriteLine($"Filter set to {filterString}");*/

            await sbc.ListenTo(Topic_Name, Subscription_Name,filterString);
            Console.WriteLine("Waiting for messages...");
            Console.ReadLine();
        }

        private void Sbc_Log(object sender, string message, ServiceBusConnectorLogLevel level)
        {
            Console.WriteLine($"Log {level.ToString()} message: {message}");
        }

        private void Sbc_Message(object sender, OnMessageEventArgs e)
        {
            string contentType = "no content type";
            if (e != null && e.ContentType != null) contentType = e.ContentType;
            string content = "no content";
            if (e != null && e.Content != null) content = e.Content.ToString();
            string meta = "no meta information";
            if (e != null && e.Properties != null) meta = string.Join(", ", e.Properties.Select(kv => $"{kv.Key} = {kv.Value}"));
            Console.WriteLine($"Message received: {contentType}; {content}; {meta}");
        }
    }
}
