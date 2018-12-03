//-----------------------------------------------------------------------

// <copyright file="ICoreService.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Monday, December 3, 2018 3:34:35 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AssistantInternalInterfaces
{
    /// <summary>
    /// Core service interface
    /// Core service connects External Communication and Presenter to the Blackboard
    /// </summary>
    public interface ICoreService : IAssistantService
    {
        /// <summary>
        /// Requests the display MVVM package for displaying / controlling a certain KPU from the Blackboard/KPU
        /// </summary>
        /// <param name="connectionId">The requestor's connectionId for returning the package to the correct recipient</param>
        /// <param name="id">the unique identifier of the package/KPU</param>
        /// <returns></returns>
        Task RequestKpuPackage(string connectionId, string id);
        /// <summary>
        /// Requests a KPU to execute a certain action, e.g. start up.
        /// </summary>
        /// <param name="kpuId">The KPU id</param>
        /// <param name="actionId">The action to perform.</param>
        /// <param name="parameters">parameters for the action call</param>
        /// <returns></returns>
        Task RequestExecute(string kpuId, string actionId, string[] parameters);
    }
}
