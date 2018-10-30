//-----------------------------------------------------------------------

// <copyright file="IAccessControlService.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Tuesday, October 30, 2018 1:26:47 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using AccessControlService.Data;
using System;
using Microsoft.ServiceFabric.Services.Remoting;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport;
using BreanosConnectors.Kpu.Communication.Common;

[assembly: FabricTransportServiceRemotingProvider(RemotingListener = RemotingListener.V2Listener, RemotingClient = RemotingClient.V2Client)]
namespace AccessControlService.Interfaces
{
    public interface IAccessControlService : IService
    {
        /// <summary>
        /// Get all permissions linked to a given group
        /// </summary>
        /// <param name="groupIdentifier">the group's identifier</param>
        /// <returns></returns>
        Task<PermissionInfo[]> GetGroupPermissions(string groupIdentifier);
        /// <summary>
        /// Get all permissions linked to a given user
        /// </summary>
        /// <param name="userIdentifier">the user's identifier</param>
        /// <returns></returns>
        Task<PermissionInfo[]> GetUserPermissions(string userIdentifier);
        /// <summary>
        /// Get all groups linked to a given user
        /// </summary>
        /// <param name="userIdentifier"></param>
        /// <returns></returns>
        Task<GroupInfo[]> GetUserGroups(string userIdentifier);
        /// <summary>
        /// Check if a given user has a given permission
        /// </summary>
        /// <param name="userIdentifier">the user's identifier</param>
        /// <param name="permissionIdentifier">the permission's identifier</param>
        /// <returns></returns>
        Task<AccessControlResponse> IsUserHasPermission(string userIdentifier, string permissionIdentifier);
        /// <summary>
        /// Register a new permission with an optional parent permission (which must already exist in the access control service's data)
        /// </summary>
        /// <param name="permissionIdentifier">the permission's identifier</param>
        /// <param name="parentPermission">optional - parent-permission's identifier</param>
        /// <returns></returns>
        Task<AccessControlResponse> RegisterPermission(string permissionIdentifier, string parentPermission);
        /// <summary>
        /// Deregister a permission. The permission will be deleted and its childpermissions will be turned into base permissions without parent permission.
        /// All group-permission associations for the permission will be deleted as well if the permission would be created again afterwards (with the same identifier), 
        /// the groups' associations to that permission would have to be recreated as well
        /// </summary>
        /// <param name="permissionIdentifier">the permission's identifier</param>
        /// <param name="parentPermission">optional - parent-permission's identifier</param>
        /// <returns></returns>
        Task<AccessControlResponse> DeregisterPermission(string permissionIdentifier);
        /// <summary>
        /// Attempt to verify the validity of a user's provided credentials
        /// </summary>
        /// <param name="userIdentifier">username</param>
        /// <param name="password">password, in plain text</param>
        /// <param name="location">additional location information</param>
        /// <returns></returns>
        Task<AccessControlResponse> GetUserLogin(string userIdentifier, string password, string location);
        /// <summary>
        /// Modifiy the set of permissions linked to a given group by adding / removing a permission, issued e.g. by a certain user
        /// </summary>
        /// <param name="permissionIdentifier">the permission's identifier</param>
        /// <param name="groupIdentifier">the group's identifier</param>
        /// <param name="isPermitted">true if a group permission should be created, false if one should be deleted</param>
        /// <param name="issuer">the change-issuing entity, i.e. user</param>
        /// <returns></returns>
        Task<AccessControlResponse> SetPermission(string permissionIdentifier, string groupIdentifier, bool isPermitted, string issuer);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userIdentifier"></param>
        /// <param name="password"></param>
        /// <param name="location"></param>
        /// <param name="createdBy"></param>
        /// <returns></returns>
        Task<AccessControlResponse> CreateUser(string userIdentifier, string password, string location, string createdBy);
        /// <summary>
        /// Registers a Kpu in the system along with its permissions and menu entries
        /// </summary>
        /// <param name="krr">the registration request</param>
        /// <returns></returns>
        Task<AccessControlResponse> RegisterKpu(KpuRegistrationRequest krr);
        /// <summary>
        /// Returns a string containing the xml definition of a navigation menu for a specific user
        /// </summary>
        /// <param name="userName">the user's username</param>
        /// <returns></returns>
        Task<string> GetMenuDefinitionForUser(string userName);
    }
}
