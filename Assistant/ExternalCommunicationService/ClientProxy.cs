//-----------------------------------------------------------------------

// <copyright file="ClientProxy.cs" company="Breanos GmbH">
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ExternalCommunicationService
{
    public interface IClientProxy
    {
        Task UpdateClient(TargetedModelUpdate m);
        Task UpdateBatchClient(TargetedBatchUpdate m);
        Task SendAddToMasterlist(string connectionId, string viewTitle, string viewId);
        Task SendRemoveFromMasterlist(string connectionId, string viewId);
        Task SendMessage(string connectionId, string message, params string[] buttons);
        Task SendXamlView(string connectionId, string viewId, string xaml);
        Task SendPackage(string connectionId, string viewId, string data);
    }

    public class ClientProxy : IClientProxy
    {
        
        private const string classname = nameof(ClientProxy);
        private static BreanosLogger logger = new BreanosLogger(classname, ServiceEventSource.Current.Message);
        IHubContext<ExternalCommunicationHub> _hub;
        public ClientProxy(IHubContext<ExternalCommunicationHub> hubContext)
        {
            _hub = hubContext;
        }

        public async Task SendAddToMasterlist(string connectionId, string viewTitle, string viewId)
        {
            logger.Trace($"connectionId = {connectionId}, viewTitle = {viewTitle}, viewId={viewId}");
            try
            {
            await _hub.Clients.Client(connectionId).SendAsync("OnAddToMasterList",viewTitle, viewId);

            }
            catch (Exception E)
            {
                logger.Error(E.ToString());
            }
            logger.Trace($"connectionId = {connectionId}, viewTitle = {viewTitle}, viewId={viewId} completed");
        }
        public async Task SendRemoveFromMasterlist(string connectionId, string viewId)
        {
            logger.Trace($"connectionId = {connectionId}, viewId={viewId}");
            await _hub.Clients.Client(connectionId).SendAsync("OnRemoveFromMasterList",viewId);
        }

        public async Task SendMessage(string connectionId, string message, params string[] buttons)
        {
            logger.Trace();
            await _hub.Clients.Client(connectionId).SendAsync("OnMessageReceived",message, buttons);
        }
        public async Task UpdateClient(TargetedModelUpdate m)
        {
            foreach (var sub in m.Subscriptions)
            {
                await _hub.Clients.Client(sub).SendAsync("OnDataReceived", m.Update.ModelId, m.Update.Property, m.Update.Value, m.Update.TimestampUtc);
            }
        }

        public async Task UpdateBatchClient(TargetedBatchUpdate m)
        {
            logger.Trace($"received {m.Updates.Count()} updates");
            foreach (var sub in m.Subscriptions)
            {
                try
                {
                    await _hub.Clients.Client(sub).SendAsync("OnBatchDataReceived",
    m.Updates.Select(u => u.ModelId).First(),
    m.Updates.Select(u => u.Property).ToArray(),
    m.Updates.Select(u => u.Value).ToArray(),
    m.Updates.Select(u=>u.TimestampUtc).ToArray());
                }
                catch (Exception e)
                {
                    logger.Error($"An exception was caused while trying to send data to a client: {e.ToString()}");
                }

            }
        }
        public async Task SendPackage(string connectionId, string viewId, string xmlEncodedData)
        {
            logger.Trace($"sending view package of {xmlEncodedData.Length} characters for {viewId} to client {connectionId}");
            await _hub.Clients.Client(connectionId).SendAsync("OnPackageReceived", viewId, xmlEncodedData);
        }
        public async Task SendXamlView(string connectionId,string viewId, string xaml)
        {
            logger.Trace($"sending xaml for {viewId} to {connectionId}");
            await _hub.Clients.Client(connectionId).SendAsync($"OnViewReceived",viewId, xaml);
        }
    }
}
