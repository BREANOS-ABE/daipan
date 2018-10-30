//-----------------------------------------------------------------------

// <copyright file="BreanosLogger.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Tuesday, October 30, 2018 1:26:47 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using NLog;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace AssistantUtilities
{
    public static class BreanosLoggerFactory
    {
        private static Dictionary<string, BreanosLogger> _existingLoggers = new Dictionary<string, BreanosLogger>();
        public static BreanosLogger DuplicateGet(string key, string classname=null)
        {
            if (_existingLoggers.ContainsKey(key))
            {
                var original = _existingLoggers[key];
                if (string.IsNullOrEmpty(classname))
                {
                    return new BreanosLogger(original.ClassName, original.MessageAction);
                }
                else
                {
                    return new BreanosLogger(classname, original.MessageAction);
                }
            }
            return null;
        }


        public static BreanosLogger CreateGet(string key, string classname, Action<string> logAction)
        {
            _existingLoggers[key] = new BreanosLogger(classname, logAction);

            return _existingLoggers[key];
        }
    }
    public class BreanosLogger
    {
        private NLog.Logger logger = LogManager.GetCurrentClassLogger();
        public string ClassName { get; set; }
        public Action<string> MessageAction { get { return _messageAction; } }
        private Action<string> _messageAction;
        public BreanosLogger(string classname, Action<string> messageAction = null)
        {
            ClassName = classname;
            _messageAction = messageAction;
        }
        
        private void Log(string message, string method, NLog.LogLevel level)
        {
            _messageAction?.Invoke($"( {level.ToString()} ) {ClassName}.{method}{((string.IsNullOrEmpty(message)) ? ("") : (": "))}{message??""}");
            logger.Log(level, $"{ClassName}.{method}{((string.IsNullOrEmpty(message)) ? ("") : (": "))}{message ?? ""}");
        }
        public void Trace(string message = "", [CallerMemberName]string method = "")
        {
            Log(message, method, LogLevel.Trace);
        }
        public void Info(string message = "", [CallerMemberName]string method = "")
        {
            Log(message, method, LogLevel.Info);
        }
        public void Warn(string message = "", [CallerMemberName]string method = "")
        {
            Log(message, method, LogLevel.Warn);
        }
        public void Error(string message = "", [CallerMemberName]string method = "")
        {
            Log(message, method, LogLevel.Error);
        }
        public void Fatal(string message = "", [CallerMemberName]string method = "")
        {
            Log(message, method, LogLevel.Fatal);
        }
        public void Debug(string message = "", [CallerMemberName]string method = "")
        {
            Log(message, method, LogLevel.Debug);
        }
    }
}
