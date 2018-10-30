//-----------------------------------------------------------------------

// <copyright file="ExternalCommunicationHub.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Tuesday, October 30, 2018 1:26:47 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using AssistantInternalInterfaces;
using AssistantUtilities;
using AssistantViewerInterfaces;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExternalCommunicationService
{
    public class ExternalCommunicationHub : Hub<IAssistantViewer>, IExternalCommunicationHub
    {
        public ExternalCommunicationHub()
        {

        }
        ExternalCommunicationService _service => ExternalCommunicationService.Instance;

        private const string classname = nameof(ExternalCommunicationHub);
        private static BreanosLogger logger = new BreanosLogger(classname, ServiceEventSource.Current.Message);

        public async Task<bool> Handshake()
        {
            logger.Trace($"Viewer connected with connectionId = {Context.ConnectionId}");
            await _service.CreateSession(Context.ConnectionId);
            return true;
        }

        public async Task Login(string user, string password)
        {
            logger.Trace($"user ={user}; password={password}");
            try
            {

                if (await _service?.Login(Context.ConnectionId, user, password))
                {
                    logger.Trace($"Sending OnReceiveGoodLogin to {Context.ConnectionId}");
                    await Clients.Client(Context.ConnectionId).OnReceiveGoodLogin();
                    var menu = await _service?.GetUserMenu(Context.ConnectionId);
                    if (!string.IsNullOrEmpty(menu))
                    {
                        await Clients.Client(Context.ConnectionId).OnMenuReceived(menu);
                    }
                    logger.Trace($"Sent OnReceiveGoodLogin to {Context.ConnectionId}");
                }
                else
                {
                    logger.Trace($"Sending OnReceiveBadLogin to {Context.ConnectionId}");
                    await Clients.Client(Context.ConnectionId).OnReceiveBadLogin();
                    logger.Trace($"Sent OnReceiveBadLogin to {Context.ConnectionId}");
                }

            }
            catch (Exception e)
            {
                logger.Error($"Exception on sending login-state information to client {Context.ConnectionId}. {e.ToString()}");
            }
        }

        public async Task RequestExecute(string controllerId, string actionId, params string[] parameters)
        {
            StringBuilder sb = new StringBuilder($"controllerId = {controllerId}; actionId= {actionId}");
            if (parameters != null && parameters.Length > 0)
                sb.AppendJoin(", ", parameters);
            sb.Append(')');
            logger.Trace(sb.ToString());
            await _service?.RequestExecute(Context.ConnectionId, controllerId, actionId, parameters);

        }

        public async Task ReceiveViewRequest(string viewId)
        {
            logger.Trace();
            await _service.ReceiveViewRequest(Context.ConnectionId, viewId);
        }

        private bool IsStringEqualBooleanTrue(string s)
        {
            logger.Trace();
            return (!string.IsNullOrEmpty(s)) &&
                ((s == "1")
                || (s == "true"));
        }
        public async Task ReceiveSubscribeRequest(string viewId)
        {
            await _service.ReceiveSubscribeRequest(Context.ConnectionId, viewId);
        }
        public async Task ReceiveUnsubscribeRequest(string viewId)
        {
            await _service.ReceiveUnsubscribeRequest(Context.ConnectionId, viewId);
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            logger.Trace($"Client {Context.ConnectionId} disconnected. Deleting subscriptions...");
            await _service.OnClientDisconnected(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task RequestManage(string actionId, params string[] parameters)
        {
            logger.Trace(actionId);
            await _service?.ParseManagementRequest(Context.ConnectionId, actionId, parameters);
        }


    }
}
