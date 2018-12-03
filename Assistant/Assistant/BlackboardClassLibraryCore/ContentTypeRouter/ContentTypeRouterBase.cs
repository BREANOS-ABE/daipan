//-----------------------------------------------------------------------

// <copyright file="ContentTypeRouterBase.cs" company="Breanos GmbH">
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
using System.Linq;
using System.Text;

namespace BlackboardClassLibraryCore
{ 
    public class ContentTypeRouterBase
    {
        /// <summary>
        /// Logger instance
        /// </summary>
        protected static NLog.Logger logger;

        protected ContentTypeList contentTypeList;

        public string[] GetQueuesFromContentType(string contentType)
        {
            var list = contentTypeList.ValueSet.Where(x => x.Content.CompareTo(contentType) == 0).FirstOrDefault();
            if (list != null)
                return list.Queues.Select(x => x.Value.QueueName).ToArray();
            else
            {
                logger.Debug($"Error in GetQueuesFromContentType missing contentType={contentType}");
            }

            return null;
        }
    }
}
