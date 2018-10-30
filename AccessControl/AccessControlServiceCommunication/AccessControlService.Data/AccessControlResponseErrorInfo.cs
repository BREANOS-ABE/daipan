//-----------------------------------------------------------------------

// <copyright file="AccessControlResponseErrorInfo.cs" company="Breanos GmbH">
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

namespace AccessControlService.Data
{
    /// <summary>
    /// Known causes for an AccessControlResponse to contain a failure.
    /// </summary>
    public enum AccessControlResponseErrorInfo : int
    {
        /// <summary>
        /// No error was caused - this is the default value for a "success" result
        /// </summary>
        NoError = 0,
        /// <summary>
        /// An unknown error caused the failure, more information might be found inside other properties
        /// </summary>
        Unknown = 99,
        /// <summary>
        /// The username or password provided was invalid. No distinction between these two cases should be made
        /// </summary>
        UserOrPasswordInvalid = 1,
        /// <summary>
        /// The username provided designates a locked user account
        /// </summary>
        UserAccountLocked = 2,
        /// <summary>
        /// A database query attempted to create an entity that already existed
        /// </summary>
        DuplicateEntity = 3,
        /// <summary>
        /// A database query requested an entity that was not found with the provided information
        /// </summary>
        EntityNotFound = 4,
        /// <summary>
        /// An argument for a call was provided in a malformed manner
        /// </summary>
        BadArgument = 5,
        /// <summary>
        /// An attempt was made to create a permission that already existed
        /// </summary>
        CreateDuplicatePermission = 6,
        /// <summary>
        /// A provided permission identifier did not match any known permission
        /// </summary>
        PermissionNotFound = 7,
        /// <summary>
        /// IdentityProvider returned an error upon attempting to verify a user's credentials
        /// </summary>
        IdentityProviderLoginError = 8,
        /// <summary>
        /// Identity provider returned an error
        /// </summary>
        IdentityProviderGeneralError = 9,
        /// <summary>
        /// Identity provider returned a database error
        /// </summary>
        IdentityProbviderDatabaseError = 10,
        /// <summary>
        /// Identity provider returned that the requested action wasn't supported by the specific implementation
        /// </summary>
        IdentityProviderUnsupportedAction = 11,
        /// <summary>
        /// Access control encountered a database error
        /// </summary>
        AccessControlDatabaseError = 12,
        /// <summary>
        /// Access control encountered an unspecified error
        /// </summary>
        AccessControlOtherError = 99

    }
}
