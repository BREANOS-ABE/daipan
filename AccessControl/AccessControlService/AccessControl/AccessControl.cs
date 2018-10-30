//-----------------------------------------------------------------------

// <copyright file="AccessControl.cs" company="Breanos GmbH">
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
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AccessControlService.Data;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using AccessControlServiceModel;
using IdentityProvider.Interfaces;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using BreanosConnectors.Kpu.Communication.Common;
using MenuProvider.Interfaces;
using System.Runtime.CompilerServices;

namespace AccessControl
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class AccessControl : StatelessService, AccessControlService.Interfaces.IAccessControlService
    {
        private const int KpuMetaDataType_MenuDefinition = 1;
        private const string MenuProviderSettingsKey = "AccessControl.MenuProvider";
        private const string WpfMenuProviderConnectionStringKey = "WpfMenuProvider.ServiceContext.ConnectionString";
        private const string IdentityProviderSettingsKey = "AccessControl.IdentityProvider";

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        /// <summary>
        /// pseudo-singleton instance storage.
        /// it's currently unknown how often / when ctor will be called
        /// used to access the currently running AccessControl instance 
        /// from other classes without having to create new instances.
        /// Mainly used since on constructing, Settings.xml infos are loaded
        /// which shouldn't be done often.
        /// </summary>
        private static AccessControl _instance;
        public static AccessControl Instance
        {
            get
            {
                return _instance;
            }
        }
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="context"></param>
        public AccessControl(StatelessServiceContext context)
            : base(context)
        {
            _instance = this;
            LoadAppSettings();
        }

        private Dictionary<string, string> AppSettings { get; set; }
        private void LoadAppSettings()
        {
            logger.Trace("");
            AppSettings = new Dictionary<string, string>();
            var package = Context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
            foreach (var section in package.Settings.Sections)
            {
                foreach (var parameter in section.Parameters)
                {
                    AppSettings[$"{section.Name}.{parameter.Name}"] = parameter.Value;
                }
            }
            logger.Trace($"Loaded {AppSettings.Count} app settings");
        }
        private IIdentityProvider GetIdentityProvider()
        {
            if (!AppSettings.ContainsKey(IdentityProviderSettingsKey)) return null;
            string iprov = AppSettings[IdentityProviderSettingsKey];
            switch (iprov)
            {
                case "BreanosIdentityProvider":
                    string connectionString = null;
                    if (AppSettings.ContainsKey("BreanosIdentityProvider.ServiceContext.ConnectionString"))
                        connectionString = AppSettings["BreanosIdentityProvider.ServiceContext.ConnectionString"];
                    return BreanosIdentityProvider.BreanosIdentityProvider.Create(connectionString);
                case "ADIdentityProvider":
                case "ADLDSIdentityProvider":
                    return null;

                default:
                    return null;
            }
        }
        private IMenuProvider GetMenuProvider()
        {
            if (!AppSettings.ContainsKey(MenuProviderSettingsKey)) return null;
            string mprov = AppSettings[MenuProviderSettingsKey];
            switch (mprov)
            {
                case "DummyMenuProvider":
                    if (AppSettings.ContainsKey(WpfMenuProviderConnectionStringKey))
                        return WpfMenuProvider.WpfMenuProviderService.Create(AppSettings[WpfMenuProviderConnectionStringKey]);
                    else return null;
                default:
                    return null;

            }
        }

        /// <summary>
        /// Get all permissions linked to a given group
        /// This will not enumerate all permissions linked to any hierarchical group, only the ones directly associated with this group
        /// </summary>
        /// <param name="groupIdentifier">the group's identifier</param>
        /// <returns></returns>
        public async Task<PermissionInfo[]> GetGroupPermissions(string groupIdentifier)
        {
            List<PermissionInfo> ret = new List<PermissionInfo>();
            using (var context = new AccessControlServiceContextFactory(AppSettings).CreateDbContext(null))
            {
                (await context.GroupPermissions.Where(g => g.GroupIdentification == groupIdentifier).ToListAsync()).ForEach(groupPermission => LinkPermissionInfos(context, groupPermission, ref ret));
                return ret.ToArray();
            }
        }

        /// <summary>
        /// Get all permissions linked to a given user
        /// </summary>
        /// <param name="userIdentifier">the user's identifier</param>
        /// <returns></returns>
        public async Task<PermissionInfo[]> GetUserPermissions(string userIdentifier)
        {
            var ip = GetIdentityProvider();
            var groupIdentifiers = (await ip.GetUserGroups(userIdentifier)).Select(x => x.GroupIdentifier);
            List<PermissionInfo> ret = new List<PermissionInfo>();
            if (groupIdentifiers.Count() <= 0)
                return ret.ToArray();
            using (var context = new AccessControlServiceContextFactory(AppSettings).CreateDbContext(null))
            {
                (await context.GroupPermissions.Where(g => groupIdentifiers.Contains(g.GroupIdentification)).Distinct().ToListAsync()).ForEach(groupPermission => LinkPermissionInfos(context, groupPermission, ref ret));
                return ret.ToArray();
            }
        }

        private void LinkPermissionInfos(AccessControlServiceContext context, GroupPermission groupPermission, ref List<PermissionInfo> ret)
        {
            try
            {
                if (!groupPermission.IsGranted) return;
                var gp = groupPermission;
                PermissionInfo p = null;
                PermissionInfo childP = null;
                Func<long, string> gpPermissionName = (long id) => (from permission in context.Permissions where permission.PermissionId == id select permission.PermissionName).FirstOrDefault();
                while (gp != null)
                {
                    if (p != null)
                    {
                        childP = p;
                    }
                    var permissionName = gpPermissionName(gp.PermissionId);
                    p = ret.Any(pi => pi.Name == permissionName) ?
                    ret.First(pi => pi.Name == permissionName) :
                    new PermissionInfo { Name = permissionName, ParentPermission = null };
                    if (childP != null)
                    {
                        childP.ParentPermission = p;
                    }
                    if (!ret.Any(pi => pi.Name == p.Name))
                        ret.Add(p);
                    var parentPermission = (from pp in context.Permissions.Include(x => x.ParentPermission)
                                            where pp.PermissionId == gp.PermissionId
                                            select pp.ParentPermission).SingleOrDefault();
                    if (parentPermission != null)
                        gp = (from gg in context.GroupPermissions
                              where gg.PermissionId == parentPermission.PermissionId
                              && gg.GroupIdentification == gp.GroupIdentification
                              select gg).SingleOrDefault();
                    else gp = null;
                }
            }
            catch (Exception e)
            {

            }

        }

        /// <summary>
        /// Get all groups linked to a given user
        /// </summary>
        /// <param name="userIdentifier"></param>
        /// <returns></returns>
        public async Task<AccessControlService.Data.GroupInfo[]> GetUserGroups(string userIdentifier)
        {
            var ipGroups = (await GetIdentityProvider().GetUserGroups(userIdentifier)).ToList();
            List<AccessControlService.Data.GroupInfo> ret = new List<AccessControlService.Data.GroupInfo>();
            ipGroups.ForEach((g) =>
            {
                IdentityProvider.Interfaces.GroupInfo tmpGrp = g;
                AccessControlService.Data.GroupInfo tmpGi = null;
                AccessControlService.Data.GroupInfo childGi = null;
                while (tmpGrp != null)
                {
                    if (tmpGi != null)
                    {
                        childGi = tmpGi;
                    }
                    tmpGi = ret.Any(gi => gi.Name == tmpGrp.GroupIdentifier) ?
                        ret.First(gi => gi.Name == tmpGrp.GroupIdentifier) :
                        new AccessControlService.Data.GroupInfo { Name = tmpGrp.GroupIdentifier, ParentGroup = null };
                    if (childGi != null)
                    {
                        childGi.ParentGroup = tmpGi;
                    }
                    if (!ret.Any(gi => gi.Name == tmpGi.Name))
                        ret.Add(tmpGi);
                    tmpGrp = tmpGrp.ParentGroup;
                }
            });
            return ret.ToArray();
        }

        /// <summary>
        /// Check if a given user has a given permission
        /// </summary>
        /// <param name="userIdentifier">the user's identifier</param>
        /// <param name="permissionIdentifier">the permission's identifier</param>
        /// <returns></returns>
        public async Task<AccessControlResponse> IsUserHasPermission(string userIdentifier, string permissionIdentifier)
        {
            if (string.IsNullOrEmpty(userIdentifier))
            {
                return new AccessControlResponse(AccessControlResponseErrorInfo.BadArgument, nameof(userIdentifier), userIdentifier);
            }
            if (string.IsNullOrEmpty(permissionIdentifier))
            {
                return new AccessControlResponse(AccessControlResponseErrorInfo.BadArgument, nameof(permissionIdentifier), permissionIdentifier);
            }
            var ip = GetIdentityProvider();
            var groupIdentifiers = (await ip.GetUserGroups(userIdentifier)).Select(x => x.GroupIdentifier);
            if (groupIdentifiers.Count() <= 0) return new AccessControlResponse(AccessControlResponseErrorInfo.NoError);

            using (var context = new AccessControlServiceContextFactory(AppSettings).CreateDbContext(null))
            {
                bool isPermissionExists = (from p in context.Permissions
                                           where permissionIdentifier == p.PermissionName
                                           select p.PermissionName).FirstOrDefault() != null;
                if (!isPermissionExists)
                {
                    return new AccessControlResponse(AccessControlResponseErrorInfo.PermissionNotFound, nameof(permissionIdentifier), permissionIdentifier);
                }
                int countFoundPermissions = (from gp in context.GroupPermissions
                                             where groupIdentifiers.Contains(gp.GroupIdentification)
                                             && gp.Permission.PermissionName == permissionIdentifier
                                             && gp.IsGranted
                                             select gp).Count();
                return (countFoundPermissions > 0) ? (new AccessControlResponse()) : (new AccessControlResponse(AccessControlResponseErrorInfo.NoError));
            }
        }

        /// <summary>
        /// Register a new permission with an optional parent permission (which must already exist in the access control service's data)
        /// </summary>
        /// <param name="permissionIdentifier">the permission's identifier</param>
        /// <param name="parentPermission">optional - parent-permission's identifier</param>
        /// <returns></returns>
        public async Task<AccessControlResponse> RegisterPermission(string permissionIdentifier, string parentPermissionIdentifier)
        {
            using (var context = new AccessControlServiceContextFactory(AppSettings).CreateDbContext(null))
            {
                var isPermissionExists = Task.Run(() => context.Permissions.Where(x => x.PermissionName == permissionIdentifier).Count() > 0);
                if (await isPermissionExists) return new AccessControlResponse(AccessControlResponseErrorInfo.DuplicateEntity, nameof(permissionIdentifier), permissionIdentifier);
                if (string.IsNullOrEmpty(parentPermissionIdentifier))
                {
                    await context.Permissions.AddAsync(new Permission() { PermissionName = permissionIdentifier });
                    await context.SaveChangesAsync();
                    return new AccessControlResponse();
                }
                else
                {
                    var parentPermissionEntity = context.Permissions.Where(x => x.PermissionName == parentPermissionIdentifier).FirstOrDefault();
                    if (parentPermissionEntity != default(Permission))
                    {
                        Permission p = new Permission()
                        {
                            PermissionName = permissionIdentifier,
                            ParentPermission = parentPermissionEntity
                        };
                        await context.Permissions.AddAsync(p);
                        await context.SaveChangesAsync();
                        return new AccessControlResponse();
                    }
                    else
                    {
                        return new AccessControlResponse(AccessControlResponseErrorInfo.EntityNotFound, nameof(parentPermissionIdentifier), parentPermissionIdentifier);
                    }
                }
            }
        }
        /// <summary>
        /// Deregister a permission. The permission will be deleted and its childpermissions will be turned into base permissions without parent permission.
        /// All group-permission associations for the permission will be deleted as well if the permission would be created again afterwards (with the same identifier), 
        /// the groups' associations to that permission would have to be recreated as well
        /// </summary>
        /// <param name="permissionIdentifier">the permission's identifier</param>
        /// <param name="parentPermission">optional - parent-permission's identifier</param>
        /// <returns></returns>
        public async Task<AccessControlResponse> DeregisterPermission(string permissionIdentifier)
        {
            using (var context = new AccessControlServiceContextFactory(AppSettings).CreateDbContext(null))
            {
                var permissionToBeDeleted = await context.Permissions.Where(x => x.PermissionName == permissionIdentifier).FirstOrDefaultAsync();
                var isPermissionExists = permissionToBeDeleted != null;
                if (isPermissionExists)
                {
                    var groupPermissionsToBeDeleted = context.GroupPermissions.Where(gp => gp.PermissionId == permissionToBeDeleted.PermissionId);
                    if (groupPermissionsToBeDeleted.Any())
                    {
                        foreach (var groupPermission in groupPermissionsToBeDeleted)
                        {
                            context.GroupPermissions.Remove(groupPermission);
                        }
                        await context.SaveChangesAsync();
                    }
                    var danglingPermissions = context.Permissions.Where(x => x.ParentPermissionId == permissionToBeDeleted.PermissionId);
                    if (danglingPermissions.Any())
                        foreach (var danglingPermission in danglingPermissions)
                        {
                            danglingPermission.ParentPermissionId = null;
                        }
                    await context.SaveChangesAsync();
                    var permissionToRemove = context.Permissions.Remove(permissionToBeDeleted);
                    await context.SaveChangesAsync();
                    return new AccessControlResponse();
                }
                else return new AccessControlResponse(AccessControlResponseErrorInfo.EntityNotFound, nameof(permissionIdentifier), permissionIdentifier);
            }
        }

        /// <summary>
        /// Attempt to verify the validity of a user's provided credentials
        /// </summary>
        /// <param name="userIdentifier">username</param>
        /// <param name="password">password, in plain text</param>
        /// <returns></returns>
        public async Task<AccessControlResponse> GetUserLogin(string userIdentifier, string password, string location)
        {
            var ip = GetIdentityProvider();
            var loginResult = await ip.GetUserLogin(userIdentifier, password, location);

            if (loginResult.IsValid) return new AccessControlResponse();
            else
            {
                var errorResponse = new AccessControlResponse();
                errorResponse.Result = false;

                switch (loginResult.LoginResult)
                {
                    case LoginResult.LOGIN_SUCCESS:

                        return new AccessControlResponse(AccessControlResponseErrorInfo.Unknown);
                    case LoginResult.LOGIN_AUTHENTICATION_FAILURE:
                        return new AccessControlResponse(AccessControlResponseErrorInfo.UserOrPasswordInvalid);
                    case LoginResult.LOGIN_DATABASE_FAILURE:
                        if (loginResult.IsException) errorResponse.ErroneousProperty = "Exception";
                        errorResponse.ErroneousPropertyValue = loginResult.ExceptionInformation;
                        errorResponse.ErrorInfo = AccessControlResponseErrorInfo.IdentityProbviderDatabaseError;
                        return errorResponse;
                    default:
                        if (loginResult.IsException) errorResponse.ErroneousProperty = "Exception";
                        errorResponse.ErroneousPropertyValue = loginResult.ExceptionInformation;
                        errorResponse.ErrorInfo = AccessControlResponseErrorInfo.IdentityProviderLoginError;
                        return new AccessControlResponse(AccessControlResponseErrorInfo.IdentityProviderLoginError);
                }
            }
        }

        /// <summary>
        /// Modifiy the set of permissions linked to a given group by adding / removing a permission, issued e.g. by a certain user
        /// </summary>
        /// <param name="permissionIdentifier">the permission's identifier</param>
        /// <param name="groupIdentifier">the group's identifier</param>
        /// <param name="isPermitted">true if a group permission should be created, false if one should be deleted</param>
        /// <param name="issuer">the change-issuing entity, i.e. user</param>
        /// <returns></returns>
        public async Task<AccessControlResponse> SetPermission(string permissionIdentifier, string groupIdentifier, bool isPermitted, string issuer)
        {
            using (var context = new AccessControlServiceContextFactory(AppSettings).CreateDbContext(null))
            {
                var p = context.Permissions.Where(x => x.PermissionName == permissionIdentifier).FirstOrDefault();
                if (p == null) return new AccessControlResponse(AccessControlResponseErrorInfo.EntityNotFound, nameof(permissionIdentifier), permissionIdentifier);
                var existingGroupPermission = context.GroupPermissions.Where(x => x.GroupIdentification == groupIdentifier && x.Permission.PermissionName == permissionIdentifier).FirstOrDefault();
                if (existingGroupPermission == null)
                {
                    var newGP = new GroupPermission()
                    {
                        PermissionId = p.PermissionId,
                        GroupIdentification = groupIdentifier,
                        GrantedAt = DateTime.UtcNow,
                        Grantee = issuer,
                        IsGranted = isPermitted
                    };
                    await context.GroupPermissions.AddAsync(newGP);
                    if (p.ParentPermissionId != null)
                    {
                        Permission pp = p;
                        while ((pp.ParentPermissionId) != null)
                        {
                            pp = GetParentPermission(context, pp);
                            GroupPermission innerGroupPermission =
                                (from gp in context.GroupPermissions
                                 where pp.PermissionId == gp.PermissionId
                                 && groupIdentifier == gp.GroupIdentification
                                 select gp).SingleOrDefault();
                            bool isNewEntity = (innerGroupPermission == null);
                            if (!isNewEntity && innerGroupPermission.IsGranted == isPermitted) continue;  //if the gp stays the same essentially, do not touch it
                            else if (isNewEntity) innerGroupPermission = new GroupPermission() { PermissionId = pp.PermissionId, GroupIdentification = groupIdentifier };

                            innerGroupPermission.GrantedAt = DateTime.UtcNow;
                            innerGroupPermission.Grantee = issuer;
                            innerGroupPermission.IsGranted = isPermitted;
                            if (isNewEntity) await context.GroupPermissions.AddAsync(innerGroupPermission);
                        }
                    }
                }
                else
                {
                    existingGroupPermission.IsGranted = isPermitted;
                    existingGroupPermission.Grantee = issuer;
                    existingGroupPermission.GrantedAt = DateTime.UtcNow;
                    IEnumerable<Permission> childPermissions = new List<Permission>() { p };
                    if (!isPermitted)
                    {
                        while ((childPermissions = GetChildPermissionsRange(context, childPermissions.Select(cpms => cpms.PermissionId))) != null)
                        {
                            var correspondingGroupPermissions = GetGroupPermissionByGroupAndPIds(context, groupIdentifier, childPermissions.Select(cp => cp.PermissionId));
                            if (correspondingGroupPermissions.Count() <= 0) break;
                            foreach (var cgp in correspondingGroupPermissions)
                            {
                                bool isChildGrantedChange = (cgp.IsGranted != isPermitted);
                                cgp.IsGranted = isPermitted;
                                if (isChildGrantedChange)
                                {
                                    cgp.GrantedAt = DateTime.UtcNow;
                                    cgp.Grantee = issuer;
                                }
                            }
                        }
                    }
                    else
                    {
                        Permission parentPermission = p;
                        while ((parentPermission = GetParentPermission(context, parentPermission)) != null)
                        {
                            var correspondingGroupPermission = GetGroupPermissionByGroupAndPId(context, groupIdentifier, parentPermission.PermissionId);
                            if (correspondingGroupPermission != null)
                            {
                                bool isParentGrantedChange = (correspondingGroupPermission.IsGranted != isPermitted);
                                correspondingGroupPermission.IsGranted = isPermitted;
                                if (isParentGrantedChange)
                                {
                                    correspondingGroupPermission.GrantedAt = DateTime.UtcNow;
                                    correspondingGroupPermission.Grantee = issuer;
                                }
                            }
                        }
                    }

                }
                await context.SaveChangesAsync();
                return new AccessControlResponse();
            }
        }
        private IEnumerable<Permission> GetChildPermissions(AccessControlServiceContext context, long permissionId)
        {
            return (from cp in context.Permissions
                    where cp.ParentPermissionId == permissionId
                    select cp);
        }

        private IEnumerable<Permission> GetChildPermissionsRange(AccessControlServiceContext context, IEnumerable<long> permissionIds)
        {
            List<Permission> permissions = new List<Permission>();
            foreach (var id in permissionIds)
            {
                permissions.AddRange(GetChildPermissions(context, id));
            }
            return permissions;
        }

        private GroupPermission GetGroupPermissionByGroupAndPId(AccessControlServiceContext context, string groupIdentifier, long permissionId)
        {
            return (from cgp in context.GroupPermissions
                    where cgp.GroupIdentification == groupIdentifier
                    && cgp.PermissionId == permissionId
                    select cgp).SingleOrDefault();
        }

        private IEnumerable<GroupPermission> GetGroupPermissionByGroupAndPIds(AccessControlServiceContext context, string groupIdentifier, IEnumerable<long> permissionIds)
        {
            return (from cgp in context.GroupPermissions
                    where cgp.GroupIdentification == groupIdentifier
                    && permissionIds.Contains(cgp.PermissionId)
                    select cgp);
        }

        private Permission GetParentPermission(AccessControlServiceContext context, Permission p)
        {
            return (from parentPermission in context.Permissions
                    where parentPermission.PermissionId == p.ParentPermissionId
                    select parentPermission).SingleOrDefault();
        }

        public async Task<AccessControlResponse> CreateUser(string userIdentifier, string password, string location, string createdBy)
        {
            var ip = GetIdentityProvider();
            var ipResult = await ip.CreateUser(userIdentifier, password, location, createdBy);
            var response = new AccessControlResponse();
            switch (ipResult.Result)
            {
                case CreateUserResult.CREATE_SUCCESS:
                    return new AccessControlResponse();
                case CreateUserResult.CREATE_NOT_SUPPORTED:
                    response.Result = false;
                    if (ipResult.IsException) response.ErroneousProperty = "Exception";
                    response.ErroneousPropertyValue = ipResult.ExceptionInformation;
                    return new AccessControlResponse(AccessControlResponseErrorInfo.IdentityProviderUnsupportedAction);
                case CreateUserResult.CREATE_DATABASE_FAILURE:
                    response.Result = false;
                    if (ipResult.IsException) response.ErroneousProperty = "Exception";
                    response.ErroneousPropertyValue = ipResult.ExceptionInformation;
                    return new AccessControlResponse(AccessControlResponseErrorInfo.IdentityProbviderDatabaseError);
                case CreateUserResult.CREATE_OTHER_FAILURE:
                    response.Result = false;
                    if (ipResult.IsException) response.ErroneousProperty = "Exception";
                    response.ErroneousPropertyValue = ipResult.ExceptionInformation;
                    return new AccessControlResponse(AccessControlResponseErrorInfo.IdentityProviderGeneralError);
                default:
                    response.Result = false;
                    if (ipResult.IsException) response.ErroneousProperty = "Exception";
                    response.ErroneousPropertyValue = ipResult.ExceptionInformation;
                    return new AccessControlResponse(AccessControlResponseErrorInfo.Unknown);
            }
        }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners() => this.CreateServiceRemotingInstanceListeners();
        private string LM(string message, [CallerMemberName]string method = null)
        {
            return $"ACS.{method}: {message}";
        }
        private async Task<bool> ProcessKpuPermissionRequest(string kpuId, KpuPermissionRequest request, KpuPermissionRequest parentPermissionRequest = null)
        {
            logger.Trace(LM($"kputId={kpuId},requestIdentifier={request.PermissionIdentifier}, parentRequestIdentifier={parentPermissionRequest?.PermissionIdentifier ?? "null"}"));
            var permissionId = kpuId + '.' + request.PermissionIdentifier;
            AccessControlResponse result = null;
            if (parentPermissionRequest == null)
            {
                result = await RegisterPermission(permissionId, null);
            }
            else
            {
                var parentPermissionId = kpuId + '.' + parentPermissionRequest.PermissionIdentifier;
                result = await RegisterPermission(permissionId, parentPermissionId);
            }
            if (request.ChildPermissionRequests != null && request.ChildPermissionRequests.Length > 0)
            {
                foreach (var childRequest in request.ChildPermissionRequests)
                {
                    await ProcessKpuPermissionRequest(kpuId, childRequest, request);
                }
            }
            return result.Result;
        }
        public async Task<AccessControlResponse> RegisterKpu(KpuRegistrationRequest krr)
        {
            logger.Trace($"KPU: {krr.KpuId}");
            var kpuName = krr.KpuId;
            var menuDefinitionText = krr.MenuXmlString;
            //test menuDefinition for correctness
            MenuDefinition mdef = null;
            if (!string.IsNullOrEmpty(menuDefinitionText))
                if (!BreanosConnectors.SerializationHelper.TryDeserialize<MenuProvider.Interfaces.MenuDefinition>(krr.MenuXmlString, out mdef))
                {
                    logger.Error($"Could not properly deserialize the sent menu definition for RegistrationRequest for {krr.KpuId}");
                    return new AccessControlResponse(AccessControlResponseErrorInfo.BadArgument, nameof(krr.MenuXmlString), krr.MenuXmlString);
                }
            //add permissions to db with fk to kpu
            logger.Trace($"Adding permissions from RegistrationRequest for {krr.KpuId}");
            foreach (var permissionRequest in krr.PermissionRequests)
            {
                await ProcessKpuPermissionRequest(krr.KpuId, permissionRequest, null);
            }
            //gather required permissions
            var requiredPermissionsByName = new List<string>();
            foreach (var mg in mdef.MenuGroups)
            {
                foreach (var mi in mg.MenuItems)
                {
                    if (!requiredPermissionsByName.Contains(mi.PermissionIdentifier)) requiredPermissionsByName.Add(mi.PermissionIdentifier);
                }
            }
            logger.Trace($"Found {requiredPermissionsByName.Count} required permissions in menu definition");
            using (var context = new AccessControlServiceContextFactory(AppSettings).CreateDbContext(null))
            {
                if (!requiredPermissionsByName.All(context.Permissions.Select(p => p.PermissionName).Contains)) // if any menu-required permission is not present in the DB by now
                {
                    var notFoundPermissions = requiredPermissionsByName.Where(rp => !context.Permissions.Select(p => p.PermissionName).Contains(rp));
                    logger.Error($"Some required permissions were not present in the database: {string.Join(", ", notFoundPermissions)}");
                    return new AccessControlResponse(AccessControlResponseErrorInfo.EntityNotFound, "Required Permission", string.Join(", ", notFoundPermissions.ToArray()));
                }
                var actualRequiredPermissionIds = context.Permissions.Where(p => requiredPermissionsByName.Contains(p.PermissionName)).Select(p => p.PermissionId);
                KpuMetadata menuMetadata = new KpuMetadata()
                {
                    Data = krr.MenuXmlString,
                    MetadataType = KpuMetaDataType_MenuDefinition,
                    Name = krr.KpuId + "_Menu",

                };
                await context.AddAsync<KpuMetadata>(menuMetadata);
                logger.Trace($"Created KpuMetadata with Id {menuMetadata.Id}");
                await context.SaveChangesAsync();
                foreach (var pId in actualRequiredPermissionIds)
                {
                    await context.AddAsync(new KpuMetadataPermission()
                    {
                        KpuMetadataId = menuMetadata.Id,
                        PermissionId = pId
                    });
                }
                await context.SaveChangesAsync();
                logger.Trace("Linked permissions to new metadata");
                //add menu to db with fk to kpu?

            }
            return new AccessControlResponse();

        }
        public async Task<string> GetMenuDefinitionForUser(string userName)
        {
            var menuProvider = GetMenuProvider();
            var groups = await GetIdentityProvider().GetUserGroups(userName);
            var permissions = new List<Permission>();
            using (var context = new AccessControlServiceContextFactory(AppSettings).CreateDbContext(null))
            {
                var permissionSet = context.GroupPermissions.Include(gp=>gp.Permission).Where(gp => groups.Select(g => g.GroupIdentifier).Contains(gp.GroupIdentification)).Where(gp=>gp.IsGranted).Select(gp=>gp.Permission).Distinct();
                permissions.AddRange(permissionSet);
            }
            var menuValue = await menuProvider.CreateMenuDefinitionForPermissionSet(permissions.Select(p=>p.PermissionId).ToArray());
            var menuValueSerialized = BreanosConnectors.SerializationHelper.Serialize(menuValue);
            return menuValueSerialized;
        }
    }
}
