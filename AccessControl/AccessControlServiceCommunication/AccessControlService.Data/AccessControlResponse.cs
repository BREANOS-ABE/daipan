//-----------------------------------------------------------------------

// <copyright file="AccessControlResponse.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Tuesday, October 30, 2018 1:26:47 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using System;

namespace AccessControlService.Data
{
    /// <summary>
    /// Represents the response object for a request sent to the AccessControl service.
    /// </summary>
    public class AccessControlResponse
    {
        /// <summary>
        /// The result of the requested action - true or false
        /// </summary>
        public bool Result { get; set; }
        /// <summary>
        /// Information about a possible error as the reason for a false result
        /// </summary>
        public AccessControlResponseErrorInfo ErrorInfo { get; set; }

        public string ErroneousProperty { get; set; }
        public string ErroneousPropertyValue { get; set; }

        /// <summary>
        /// Constructor for a "success"-response
        /// </summary>
        public AccessControlResponse()
        {
            Result = true;
            ErrorInfo = AccessControlResponseErrorInfo.NoError;
        }
        /// <summary>
        /// Constructor for a "failure"-response
        /// can set property and value for info about the cause of the error
        /// </summary>
        /// <param name="info">enumeration value describing the error</param>
        /// <param name="property">If the error was caused by a faulty property, its name may be indicated here</param>
        /// <param name="value">If the error was caused by a faulty property, its (faulty) value may be indicated here</param>
        public AccessControlResponse(AccessControlResponseErrorInfo info, string property=null, string value=null)
        {
            Result = false;
            ErrorInfo = info;
            ErroneousProperty = property;
            ErroneousPropertyValue = value;
        }
    }
}
