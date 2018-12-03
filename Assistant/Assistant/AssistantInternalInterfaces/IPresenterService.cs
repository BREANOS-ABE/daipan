//-----------------------------------------------------------------------

// <copyright file="IPresenterService.cs" company="Breanos GmbH">
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
using System.Text;
using System.Threading.Tasks;

namespace AssistantInternalInterfaces
{
    /// <summary>
    /// Presenter common interface.
    /// The Presenter is used to link a collection of subscribers to their subscribed models so they can all receive model updates
    /// </summary>
    public interface IPresenterService : IAssistantService
    {
        /// <summary>
        /// Initialization method called by the CoreService upon its initialization.
        /// </summary>
        /// <returns></returns>
        Task Initialize();
        /// <summary>
        /// Links a connectionId as a subscriber to a modelId so it will receive ModelUpdates
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="modelId"></param>
        /// <returns></returns>
        Task SubscribeConnectionToModel(string connectionId, string modelId);
        /// <summary>
        /// Unlinks a connectionId as a subscriber from a modelId so it will no longer receive ModelUpdates
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="modelId"></param>
        /// <returns></returns>
        Task UnsubscribeConnectionFromModel(string connectionId, string modelId);
        /// <summary>
        /// Unlinks a connectionId from all subscribed modelIds
        /// </summary>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        Task UnsubscribeConnection(string connectionId);
        /// <summary>
        /// Adds a modelId to the collection of known modelIds so connections may subscribe to it
        /// </summary>
        /// <param name="modelId"></param>
        /// <returns></returns>
        Task AppendKnownModel(string modelId);
        /// <summary>
        /// Removes a modelId from the collection of known modelIds so connections can no longer subscribe to it.
        /// </summary>
        /// <param name="modelId"></param>
        /// <returns></returns>
        Task RemoveKnownModel(string modelId);
        /// <summary>
        /// Gives a batch of ModelUpdates to the presenter so it may forward it to the subscribers
        /// </summary>
        /// <param name="updates"></param>
        /// <returns></returns>
        Task BatchUpdateValues(BreanosConnectors.Kpu.Communication.Common.ModelUpdate[] updates);
    }
}

