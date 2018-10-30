//-----------------------------------------------------------------------

// <copyright file="IExternalCommunicationHub.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Tuesday, October 30, 2018 1:26:47 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AssistantViewerInterfaces
{
    public interface IExternalCommunicationHub
    {
        /// <summary>
        /// Should be used as the first call to the hub
        /// </summary>
        /// <returns></returns>
        Task<bool> Handshake();
        /// <summary>
        /// Use this to establish a user-context on your session and become able to perform permission-restricted actions
        /// </summary>
        /// <param name="user">the user's name in the linked identitiy provider</param>
        /// <param name="password">the user's password, hashed as required by the identitiy provider</param>
        /// <returns></returns>
        Task Login(string user, string password);
        /// <summary>
        /// Requests the execution of a specific action within the assistant system
        /// </summary>
        /// <param name="controllerId"></param>
        /// <param name="actionId"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Task RequestExecute(string controllerId, string actionId, params string[] parameters);
        /// <summary>
        /// Requests the execution of a specific management action within the assistant system, i.e. 
        /// something that concerns the assistant itself, like user management and not the production.
        /// </summary>
        /// <param name="actionId"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Task RequestManage(string actionId, params string[] parameters);
 
        Task ReceiveViewRequest(string viewId);
        Task ReceiveSubscribeRequest(string viewId);
        Task ReceiveUnsubscribeRequest(string viewId);
    }
}
