//-----------------------------------------------------------------------

// <copyright file="BreanosIdentityProvider.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Tuesday, October 30, 2018 1:26:47 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using IdentityProvider.Interfaces;
using IdentityProviderModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BreanosIdentityProvider
{
    public class BreanosIdentityProvider : IIdentityProvider
    {
        private string connectionString;
        public static BreanosIdentityProvider Create(string databaseConnectionString)
        {
            var bip = new BreanosIdentityProvider();
            bip.connectionString = databaseConnectionString;
            return bip;
        }

        public async Task<CreateUserResponse> CreateUser(string userIdentifier, string password, string location, string createdBy)
        {
            var reply = new CreateUserResponse()
            {
                IsSuccessful = false,
                Result = CreateUserResult.CREATE_OTHER_FAILURE
            };
            using (IdentityProviderContext context = IdentityProviderContext.Create(connectionString))
            {
                try
                {
                    var user = new User(createdBy, userIdentifier, password);
                    context.Users.Add(user);
                    await context.SaveChangesAsync();
                    reply.Result = CreateUserResult.CREATE_SUCCESS;
                    reply.IsSuccessful = true;
                }
                catch (Exception e)
                {
                    reply.Result = CreateUserResult.CREATE_DATABASE_FAILURE;
                    reply.IsSuccessful = false;
                }
            }
            return reply;
        }

        public async Task<ICollection<GroupInfo>> GetAllGroups()
        {
            List<GroupInfo> ret = new List<GroupInfo>();
            using (IdentityProviderContext context = IdentityProviderContext.Create(connectionString))
            {
                await Task.Run(() => context.Groups.Where(g => g.ParentGroup == null).ToList().ForEach(delegate (Group g)
                {
                    ret.AddRange(GetGroups(g));
                }));
            }
            return ret;
        }

        public async Task<ICollection<GroupInfo>> GetUserGroups(string userIdentifier)
        {
            List<GroupInfo> ret = new List<GroupInfo>();
            using (IdentityProviderContext context = IdentityProviderContext.Create(connectionString))
            {
                if (!context.Users.Any(u => u.UserIdentifier == userIdentifier)) return new List<GroupInfo>();
                
                (context.Users.Include("Groups").Include("Groups.Group").Single(u => u.UserIdentifier == userIdentifier).Groups.ToList().Select(ug => ug.Group).ToList())
                .ForEach(delegate (Group g)
                {
                    Group tmpGrp = g;
                    GroupInfo tmpGi = null;
                    GroupInfo childGi = null;
                    while (tmpGrp != null)
                    {
                        if (tmpGi != null)
                        {
                            childGi = tmpGi;
                        }
                        tmpGi = ret.Any(gi => gi.GroupIdentifier == tmpGrp.GroupIdentifier) ?
                            ret.First(gi => gi.GroupIdentifier == tmpGrp.GroupIdentifier) :
                            new GroupInfo { GroupIdentifier = tmpGrp.GroupIdentifier, ParentGroup = null };
                        if (childGi != null)
                        {
                            childGi.ParentGroup = tmpGi;
                        }
                        if (!ret.Any(gi => gi.GroupIdentifier == tmpGi.GroupIdentifier))
                            ret.Add(tmpGi);
                        if (tmpGrp.ParentGroupId != null)
                            tmpGrp = context.Groups.Single(grp => grp.GroupId == tmpGrp.ParentGroupId);
                        else tmpGrp = null;
                    }
                });
            }
            return ret;
        }

        public LoginInformation VerifyLogin(User dbUser, string cleartext, string location = "")
        {
            LoginInformation ret = new LoginInformation { IsValid = false, LoginResult = LoginResult.LOGIN_PROVIDER_FAILURE };
            
            try
            {
                if (dbUser.Password == User.GetPassword(cleartext, dbUser.Salt))
                {
                    ret.IsValid = true;
                    ret.LoginResult = LoginResult.LOGIN_SUCCESS;
                    dbUser.UpdateLoginTimestamps();
                }
                else
                {
                    ret.LoginResult = LoginResult.LOGIN_AUTHENTICATION_FAILURE;
                }
            }
            catch { }
            return ret;
        }

        public async Task<LoginInformation> GetUserLogin(string userIdentifier, string password, string location)
        {
            LoginInformation ret = new LoginInformation() { IsValid = false, LoginResult = LoginResult.LOGIN_PROVIDER_FAILURE };
            try
            {
                using (IdentityProviderContext context = IdentityProviderContext.Create(connectionString))
                {
                    User user = await Task.Run(() => context.Users.FirstOrDefault(u => u.UserIdentifier == userIdentifier));
                    if (user != null)
                    {
                        ret = VerifyLogin(user, password, location);
                        try
                        {
                            LoginAttempt la = new LoginAttempt() { LoginLocation = location, Result = ret.LoginResult, UserId = user.UserId };
                            context.LoginAttempts.Add(la);
                            context.SaveChanges();
                        }
                        catch (Exception e)
                        {
                            ret.IsException = true;
                            ret.ExceptionInformation = e.ToString();
                            ret.LoginResult = LoginResult.LOGIN_DATABASE_FAILURE;
                            ret.IsValid = false;
                        }
                    }
                    else
                    {
                        ret.LoginResult = LoginResult.LOGIN_AUTHENTICATION_FAILURE;
                    }
                }
            }
            catch (Exception e)
            {
                ret.IsException = true;
                ret.ExceptionInformation = e.ToString();
                ret.LoginResult = LoginResult.LOGIN_OTHER_FAILURE;
            }
            return ret;
        }

        private List<GroupInfo> GetGroups(Group g, GroupInfo p = null)
        {
            List<GroupInfo> ret = new List<GroupInfo>();
            GroupInfo gi = new GroupInfo() { GroupIdentifier = g.GroupIdentifier, ParentGroup = p };
            ret.Add(gi);
            foreach (Group sg in g.Children)
            {
                ret.AddRange(GetGroups(sg, gi));
            }
            return ret;
        }
    }
}
