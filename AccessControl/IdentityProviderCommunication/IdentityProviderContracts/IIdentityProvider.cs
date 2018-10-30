//-----------------------------------------------------------------------

// <copyright file="IIdentityProvider.cs" company="Breanos GmbH">
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
using System.Threading.Tasks;

namespace IdentityProvider.Interfaces
{
    public interface IIdentityProvider
    {
        Task<ICollection<GroupInfo>> GetAllGroups();
        Task<ICollection<GroupInfo>> GetUserGroups(string userIdentifier);
        Task<LoginInformation> GetUserLogin(string userIdentifier, string password, string location);
        Task<CreateUserResponse> CreateUser(string userIdentifier, string password, string location, string createdBy);
    }
    public class GroupInfo
    {
        public string GroupIdentifier { get; set; }
        public GroupInfo ParentGroup { get; set; }
    }

    public class LoginInformation
    {
        public bool IsValid { get; set; }
        public LoginResult LoginResult { get; set; }
        public bool IsException { get; set; }
        public string ExceptionInformation { get; set; }
    }

    public class CreateUserResponse
    {
        public bool IsSuccessful { get; set; }
        public CreateUserResult Result { get; set; }
        public bool IsException { get; set; }
        public string ExceptionInformation { get; set; }
    }

    public enum CreateUserResult : int
    {
        CREATE_SUCCESS = 0,
        CREATE_NOT_SUPPORTED = 1,
        CREATE_DATABASE_FAILURE = 2,
        CREATE_OTHER_FAILURE = 99
    }

    public enum LoginResult : int
    {
        LOGIN_SUCCESS = 0,
        LOGIN_AUTHENTICATION_FAILURE = 1,
        LOGIN_PROVIDER_FAILURE = 2,
        LOGIN_DATABASE_FAILURE = 3,
        LOGIN_OTHER_FAILURE = 99
    }

    

}
