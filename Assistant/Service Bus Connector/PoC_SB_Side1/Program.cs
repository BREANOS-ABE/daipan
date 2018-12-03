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
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PoC_SB_Side1
{
    [DataContract]
    class PrivateClassFirstGrade
    {
        [DataMember]
        public int FourtyTwo { get; set; }
        [DataMember]
        public string FourtyThree { get; set; }

    }
    class Program
    {
        static Task t;
        const string Topic_Name = "PoC_SB_Topic";
        static void Main(string[] args)
        {
            new Program().Run().Wait();
            Console.ReadLine();
        }
        private async Task Run()
        {
            ServiceBusConnector sbc = new ServiceBusConnector();
            var fqdn = ConfigurationManager.AppSettings["SB.Fqdn"];
            var ns = ConfigurationManager.AppSettings["SB.Ns"];
            var hp = ConfigurationManager.AppSettings["SB.HttpPort"];
            var tp = ConfigurationManager.AppSettings["SB.TcpPort"];
            var hpi = int.Parse(hp);
            var tpi = int.Parse(tp);
            sbc.Log += Sbc_Log;
            await sbc.Connect(ns, fqdn, hpi, tpi);
            Random r = new Random();
            //byte, sbyte, char, short, ushort, int, uint, long, ulong, float, double, decimal, bool, Guid, string, Uri, DateTime, DateTimeOffset, and TimeSpan

            while (true)
            {
                var props = new Dictionary<string, object>
            {
                {"Byte", (byte) r.Next(0,256) },
                {"Sbyte", (sbyte) r.Next(-127,127) },
                {"char", (char)r.Next(21,128) },
                {"short", (short)r.Next(0,short.MaxValue+1) },
                {"ushort",(ushort)r.Next(0,short.MaxValue+1) },
                {"int",(int)r.Next(0,short.MaxValue+1) },
                {"uint",(uint)r.Next(0,short.MaxValue+1) },
                {"long",(long)r.Next(0,short.MaxValue+1) },
                {"ulong",(ulong)r.Next(0,short.MaxValue+1) },
                {"float",(float)r.NextDouble() },
                {"double",(double)r.NextDouble() },
                {"decimal",(decimal)r.NextDouble() },
                {"bool",bool.Parse(new[]{"false","true" }[r.Next(0,2)]) },
                {"Guid",Guid.NewGuid() },
                {"string","gji8r0e9hzg78shuj" },
                {"Uri",new Uri("http://www.breanos.com/") },
                {"DateTime",DateTime.Now },
                {"DateTimeOffset",DateTimeOffset.Now },
                {"TimeSpan",TimeSpan.FromMinutes(r.Next()) }

            };
                Console.WriteLine("Press enter to send");
                Console.ReadLine();
                
                try
                {

                    await sbc.Send("lalala", Topic_Name, "stringy",props);
                }
                catch (Exception e)
                {
                    Sbc_Log(null, $"BadException: {e.ToString()}", ServiceBusConnectorLogLevel.Error);
                }
                
                Sbc_Log(null, "Sent!", ServiceBusConnectorLogLevel.Info);
            }

        }

        private void Sbc_Log(object sender, string message, ServiceBusConnectorLogLevel level)
        {
            Console.WriteLine(message);
        }
    }
}
