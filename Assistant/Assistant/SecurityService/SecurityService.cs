//-----------------------------------------------------------------------

// <copyright file="SecurityService.cs" company="Breanos GmbH">
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
using System.Fabric;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using NLog;
using AssistantUtilities;
using System.Text;
using AccessControlService.Data;
using BreanosConnectors.Kpu.Communication.Common;

namespace SecurityService
{
    class DummyAccessControl
    {
        Dictionary<string, string> _users;
        Dictionary<string, string[]> _userPermissions;
        Dictionary<string, string> _actionPermissions;
        List<string> _permissions;
        public DummyAccessControl()
        {
            _permissions = new List<string>()
            {
                        "Driller1.control",
                        "Driller1.view",
                        "Driller2.control",
                        "Driller2.view"
            };
            _users = new Dictionary<string, string>()
            {
                { "admin", "12345" },
                { "daudau", "42" },
            };
            _userPermissions = new Dictionary<string, string[]>()
            {
                { "admin",
                    new[]
                    {
                        "Driller1.control",
                        "Driller1.view",
                        "Driller2.control",
                        "Driller2.view"
                    }
                },
                { "daudau",
                    new[]
                    {
                        "Driller1.view",
                        "Driller2.control",
                        "Driller2.view"
                    }
                }
            };
            _actionPermissions = new Dictionary<string, string>()
            {
                { "Driller1.subscribe","Driller1.view" },
                { "Driller1.start","Driller1.control" },
                { "Driller1.stop","Driller1.control" },
                { "Driller1.forward","Driller1.control" },
                { "Driller2.subscribe","Driller2.view" },
                { "Driller2.start","Driller2.control" },
                { "Driller2.stop","Driller2.control" },
                { "Driller2.forward","Driller2.control" }
            };
        }
        public bool Login(string name, string password)
        {
            return _users.ContainsKey(name) && _users[name] == password;
        }
        public IEnumerable<string> GetViewPermissionsForUser(string user)
        {

            return _userPermissions[user].Where(s => s.EndsWith(".view")).Select(perm => perm.Replace(".view", ""));
        }

        public bool IsUserHasPermission(string user, string action)
        {

            if (_userPermissions.ContainsKey(user) && _actionPermissions.ContainsKey(action))
            {
                var permission = _actionPermissions[action];
                return _userPermissions[user].Contains(permission);
            }
            return false;
        }

        public string GetActionPermission(string actionId)
        {
            return (_actionPermissions.ContainsKey(actionId)) ? (_actionPermissions[actionId]) : (null);
        }
    }
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class SecurityService : StatelessService, AssistantInternalInterfaces.ISecurityService
    {
        private const string _accessControlServiceUri = "fabric:/AccessControlService/AccessControl";
        private AccessControlService.Interfaces.IAccessControlService _accessControlService;
        private const string classname = nameof(SecurityService);
        private static BreanosLogger logger = new BreanosLogger(classname, ServiceEventSource.Current.Message);

        public Dictionary<string, string> _sessionUsers;
        public SecurityService(StatelessServiceContext context)
            : base(context)
        {
            _sessionUsers = new Dictionary<string, string>();
            _accessControlService = ServiceProxy.Create<AccessControlService.Interfaces.IAccessControlService>(new Uri(_accessControlServiceUri));
        }

        public async Task<AccessControlResponse> RegisterKpu(KpuRegistrationRequest krr)         
        {
            logger.Trace($"called RegisterKpu {krr}");
            return await _accessControlService.RegisterKpu(krr);
        }

        public async Task<bool> CheckPermission(string connectionId, string actionId)
        {
            try
            {
                logger.Trace($"called with connectionId = {connectionId}, actionId = {actionId}");
                var user = GetUserFromConnectionId(connectionId);
                logger.Trace($"session {connectionId} resolved to user {user}");
                if (_sessionUsers[connectionId] == null) return (await _accessControlService.IsUserHasPermission("nullUser", actionId)).Result;
                var hasPermission = _accessControlService.IsUserHasPermission(user, actionId);
                logger.Trace($"User {user} has permission for {actionId} : {hasPermission}");
                return (await hasPermission).Result;
            }
            catch (Exception e)
            {
                logger.Error($"exception was caused: {e.ToString()}");
                return false;
            }

        }

        public async Task CreateSession(string connectionId)
        {
            try
            {
                logger.Trace($"called with connectionId = {connectionId}");
                _sessionUsers.Add(connectionId, null);
            }
            catch (Exception e)
            {
                logger.Error($"exception was caused: {e.ToString()}");
            }

        }

        public async Task<bool> Login(string connectionId, string user, string password)
        {
            try
            {
                logger.Trace($"called with user = {user}, password = *****");
                var acsResponseTask = _accessControlService.GetUserLogin(user, password, "");
                if ((await acsResponseTask).Result)
                {
                    _sessionUsers[connectionId] = user;
                }
                var res = await acsResponseTask;
                logger.Trace($"Response from ACS: {res.Result}");
                if (!res.Result)
                {
                    logger.Error($"ErrorInfo: {res.ErrorInfo.ToString()}");
                    if (!string.IsNullOrEmpty(res.ErroneousProperty))
                        logger.Error($"ErrorInfo: {res.ErroneousProperty} was {res.ErroneousPropertyValue ?? "null"}");
                }
                return res.Result;
            }
            catch (Exception e)
            {
                logger.Error($"exception was caused: {e.ToString()}");
                return false;
            }

        }
        private string GetUserFromConnectionId(string connectionId)
        {
            string user;

            if (!_sessionUsers.ContainsKey(connectionId))
                user = "nullUser";
            else
                user = _sessionUsers[connectionId];

            return user;
        }
       
        public async Task<string> GetUserMenu(string connectionId)
        {
            var user = GetUserFromConnectionId(connectionId);
            var menu = await _accessControlService.GetMenuDefinitionForUser(user);
            return menu;
        }

        public async Task SetSessionUser(string connectionId, string user)
        {
            try
            {
                logger.Trace($"called with connectionId = {connectionId}, user = {user}");
                _sessionUsers[connectionId] = user;
            }
            catch (Exception e)
            {
                logger.Error($"exception was caused: {e.ToString()}");
            }

        }

        public async Task SessionDisconnected(string connectionId)
        {
            try
            {
                logger.Trace();
                _sessionUsers.Remove(connectionId);

            }
            catch (Exception e)
            {
                logger.Error($"exception was caused: {e.ToString()}");
            }
        }

        public async Task Logout(string connectionId)
        {
            try
            {
                logger.Trace();
                if (_sessionUsers.ContainsKey(connectionId))
                {
                    _sessionUsers[connectionId] = null;
                }
            }
            catch (Exception e)
            {
                logger.Error($"exception was caused: {e.ToString()}");
            }
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners() => this.CreateServiceRemotingInstanceListeners();

        public async Task<bool> CreateUser(string connectionId, string userIdentification, string password, string location)
        {
            logger.Trace();
            var creator = GetUserFromConnectionId(connectionId);
            var result = await _accessControlService.CreateUser(userIdentification, password, location, creator);
            return result.Result;
        }

        /// <summary>
        /// Passthrough method for Access Control' s CreatePermission.
        /// </summary>
        /// <param name="permission">the permission to be created</param>
        /// <param name="parentPermission">the parent permission for the new permission. If someone is granted the permission, they're automatically granted parentPermission</param>
        /// <returns></returns>
        public async Task<bool> CreatePermission(string permission, string parentPermission)
        {
            logger.Trace();
            try
            {
                var result = await _accessControlService.RegisterPermission(permission, parentPermission);
                if (!result.Result)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append($"{nameof(CreatePermission)} failed. ErrorInfo was {result.ErrorInfo.ToString()}; ");
                    if (!string.IsNullOrEmpty(result.ErroneousProperty))
                    {
                        sb.Append($"Erroneous Property {result.ErroneousProperty} was {result.ErroneousPropertyValue ?? "null"}");
                    }
                    logger.Error(sb.ToString());
                }
                return result.Result;
            }
            catch (Exception e)
            {
                logger.Error($"Exception was caused during {nameof(CreatePermission)}: {e.ToString()}");
                return false;
            }
        }
        public async Task<bool> DeletePermission(string permission)
        {
            logger.Trace();
            try
            {
                var result = await _accessControlService.DeregisterPermission(permission);
                if (!result.Result)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append($"{nameof(DeletePermission)} failed. ErrorInfo was {result.ErrorInfo.ToString()}; ");
                    if (!string.IsNullOrEmpty(result.ErroneousProperty))
                    {
                        sb.Append($"Erroneous Property {result.ErroneousProperty} was {result.ErroneousPropertyValue ?? "null"}");
                    }
                    logger.Error(sb.ToString());
                }
                return result.Result;
            }
            catch (Exception e)
            {
                logger.Error($"Exception was caused during {nameof(DeletePermission)}: {e.ToString()}");
                return false;
            }
        }      
    }
}
