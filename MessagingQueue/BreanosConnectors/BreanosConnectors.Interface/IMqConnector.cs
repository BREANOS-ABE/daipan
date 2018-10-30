//-----------------------------------------------------------------------

// <copyright file="IMqConnector.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Tuesday, October 30, 2018 1:26:47 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using System;
using System.Threading.Tasks;

namespace BreanosConnectors.Interface
{
    public delegate void OnMessage(object sender, OnMessageEventArgs e);
    public interface IMqConnector
    {
        event OnMessage Message;
        /// <summary>
        /// Attempts to establish a connection with a given connection endpoint defined by the parameters.
        /// See the documentation of the actual implementation for details
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Task<bool> ConnectAsync(params string[] parameters);
        /// <summary>
        /// Attempts to create a listening subscription-like session with a specific listening endpoint on the connected server
        /// See the documentation of the actual implementation on how to fill these parameters
        /// </summary>
        /// <param name="path"></param>
        /// <param name="subscription"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<bool> ListenAsync(string path, string subscription = null, string filter = null);
        /// <summary>
        /// Attempts to send a message containing a payload and possibly some metadata properties
        /// to a specified sending endpoint on the connected server.
        /// See the documentation of the actual implementation on how to use the parameters
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="path"></param>
        /// <param name="contentType"></param>
        /// <param name="properties"></param>
        /// <param name="retries"></param>
        /// <param name="retryDelayMs"></param>
        /// <returns></returns>
        Task<bool> SendAsync(string payload, string path, string contentType, (string, object)[] properties = null, int retries = 5, int retryDelayMs = 5000);
        /// <summary>
        /// Attempts to close an open listening session. 
        /// If none was open, it shouldn't do anything.
        /// </summary>
        void StopListening();

    }
}
