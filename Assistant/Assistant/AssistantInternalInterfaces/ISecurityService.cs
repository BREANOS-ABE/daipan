//-----------------------------------------------------------------------

// <copyright file="ISecurityService.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Monday, December 3, 2018 3:34:35 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using AccessControlService.Data;
using BreanosConnectors.Kpu.Communication.Common;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport;
using System;
using System.Threading.Tasks;

[assembly: FabricTransportServiceRemotingProvider(RemotingListener = RemotingListener.V2Listener, RemotingClient = RemotingClient.V2Client)]
namespace AssistantInternalInterfaces
{
    /// <summary>
    /// SecurityService common interface
    /// The SecurityService acts as a gateway towards the AccessControlService.
    /// </summary>
    public interface ISecurityService : IAssistantService
    {
        /// <summary>
        /// Registers a connectionId as connected
        /// </summary>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        Task CreateSession(string connectionId);
        /// <summary>
        /// Registers a user identifier with a connectionId / session
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        Task SetSessionUser(string connectionId, string user);
        /// <summary>
        /// Attempts to log a user in, using the provided credentials
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        Task<bool> Login(string connectionId, string user, string password);
        /// <summary>
        /// Checks whether a given user has a given permission in the system
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="permissionId"></param>
        /// <returns></returns>
        Task<bool> CheckPermission(string connectionId, string permissionId);
        ///// <summary>
        ///// Returns a collection of viewIds that a given connectionId has permission to view
        ///// </summary>
        ///// <param name="connectionId"></param>
        ///// <returns></returns>
        //Task<string[]> GetPermittedViews(string connectionId);
        Task<string> GetUserMenu(string connectionId);
        /// <summary>
        /// Attempts to log a user out, removing the user from the stored session
        /// </summary>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        Task Logout(string connectionId);
        /// <summary>
        /// Cleanup for when a connection closes
        /// </summary>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        Task SessionDisconnected(string connectionId);
        /// <summary>
        /// Create a user. May fail if the BreanosIdentityProvider is not used in the system but instead another one is deployed, e.g. an IdentityProvider for ActiveDirectory
        /// </summary>
        /// <param name="connectionId">current connectionId</param>
        /// <param name="userIdentification">the new user's id</param>
        /// <param name="password">the new user's password</param>
        /// <param name="location"></param>
        /// <returns></returns>
        Task<bool> CreateUser(string connectionId, string userIdentification, string password, string location);
        /// <summary>
        /// Create a permission that can then be linked to groups of users.
        /// A permission can be linked to a parent permission. if Permission p1 has parent p2 and group g1 is granted p1, it is automatically granted p2 as well.
        /// </summary>
        /// <param name="permission">the permission's id</param>
        /// <param name="parentPermission">the parent permission's id</param>
        /// <returns></returns>
        Task<bool> CreatePermission(string permission, string parentPermission);
        /// <summary>
        /// Delegate Kpu registration to AccessControl
        /// </summary>
        /// <param name="krr">The KpuRegistrationRequest object</param>
        /// <returns></returns>
        Task<AccessControlResponse> RegisterKpu(KpuRegistrationRequest krr);
    }
}
