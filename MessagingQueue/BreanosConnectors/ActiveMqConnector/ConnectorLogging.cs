//-----------------------------------------------------------------------

// <copyright file="ConnectorLogging.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Monday, December 3, 2018 3:34:35 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace BreanosConnectors
{
    namespace ActiveMqConnector
    {
        class ConnectorLogging
        {
            NLog.Logger _logger;
            private string _className;
            public ConnectorLogging(string className)
            {
                _className = className;
                _logger = NLog.LogManager.GetLogger(_className);
            }
            private void LogMessage(int level, string message = "", [CallerMemberName]string method = "")
            {
                NLog.LogLevel logLevel;
                switch (level)
                {
                    case 0:
                        logLevel = NLog.LogLevel.Trace;
                        break;
                    case 1:
                        logLevel = NLog.LogLevel.Debug;
                        break;
                    case 2:
                        logLevel = NLog.LogLevel.Info;
                        break;
                    case 3:
                        logLevel = NLog.LogLevel.Warn;
                        break;
                    case 4:
                        logLevel = NLog.LogLevel.Error;
                        break;
                    default:
                        logLevel = NLog.LogLevel.Fatal;
                        break;
                }
                var logMessage = $"{_className}.{method}{((string.IsNullOrEmpty(message)) ? ("") : (": "))}{message??""}";
                var lInfo = new NLog.LogEventInfo(logLevel, _className, logMessage);
                _logger.Log(lInfo);
            }
            public void Fatal(string message = "", [CallerMemberName]string method = "")
            {
                LogMessage(5, message, method);
            }
            public void Error(string message = "", [CallerMemberName]string method = "")
            {
                LogMessage(4, message, method);
            }
            public void Warn(string message = "", [CallerMemberName]string method = "")
            {
                LogMessage(3, message, method);
            }
            public void Info(string message = "", [CallerMemberName]string method = "")
            {
                LogMessage(2, message, method);
            }
            public void Debug(string message = "", [CallerMemberName]string method = "")
            {
                LogMessage(1, message, method);
            }
            public void Trace(string message = "", [CallerMemberName]string method = "")
            {
                LogMessage(0, message, method);
            }
            public static string Process(params (string,object)[] parameters)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("(");
                for (int i = 0; i < parameters.Length; i++)
                {
                    sb.Append($"{parameters[i].Item1 ?? "null"} = {parameters[i].Item2 ?? "null"}");
                    if (i < parameters.Length - 1) sb.Append(", ");
                }
                sb.Append(")");
                return sb.ToString();
            }
        }
    }
}
