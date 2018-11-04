//-----------------------------------------------------------------------

// <copyright file="Logger.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Monday, November 5, 2018 12:22:54 AM</date>
// </copyright>

//-----------------------------------------------------------------------


using NLog;
using System;

namespace CWF.Core
{
    /// <summary>
    /// Logger.
    /// </summary>
    public static class Logger
    {
        private static NLog.Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Logs an information message.
        /// </summary>
        /// <param name="msg">Log message.</param>
        public static void Info(string msg)
        {
            logger.Info(msg);            
        }

        /// <summary>
        /// Logs a formatted information message.
        /// </summary>
        /// <param name="msg">Formatted log message.</param>
        /// <param name="args">Arguments.</param>
        public static void InfoFormat(string msg, params object[] args)
        {
            logger.Info(msg, args);
        }

        /// <summary>
        /// Logs a formatted log message and an exception.
        /// </summary>
        /// <param name="msg">Formatted log message.</param>
        /// <param name="e">Exception.</param>
        /// <param name="args">Arguments.</param>
        public static void InfoFormat(string msg, Exception e, params object[] args)
        {
            logger.Info(e, msg, args);
        }

        /// <summary>
        /// Logs a Debug log message.
        /// </summary>
        /// <param name="msg">Log message.</param>
        public static void Debug(string msg)
        {
            logger.Debug(msg);
        }

        /// <summary>
        /// Logs a formatted debug message.
        /// </summary>
        /// <param name="msg">Log message.</param>
        /// <param name="args">Arguments.</param>
        public static void DebugFormat(string msg, params object[] args)
        {
            logger.Debug(msg, args);
        }

        /// <summary>
        /// Logs an error log message.
        /// </summary>
        /// <param name="msg">Log message.</param>
        public static void Error(string msg)
        {
            logger.Error(msg);
        }

        /// <summary>
        /// Logs a formatted error message.
        /// </summary>
        /// <param name="msg">Log message.</param>
        /// <param name="args">Arguments.</param>
        public static void ErrorFormat(string msg, params object[] args)
        {
            logger.Error(msg, args);
        }

        /// <summary>
        /// Logs an error message and an exception.
        /// </summary>
        /// <param name="msg">Log message.</param>
        /// <param name="e">Exception.</param>
        public static void Error(string msg, Exception e)
        {
            logger.Error(e, msg);
        }

        /// <summary>
        /// Logs a formatted log message and an exception.
        /// </summary>
        /// <param name="msg">Formatted log message.</param>
        /// <param name="e">Exception.</param>
        /// <param name="args">Arguments.</param>
        public static void ErrorFormat(string msg, Exception e, params object[] args)
        {
            logger.Error(e, msg, args);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        public static void Trace(string msg)
        {
            logger.Trace(msg);
        }

      
    }
}
