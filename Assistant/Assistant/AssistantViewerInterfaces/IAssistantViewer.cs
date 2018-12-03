//-----------------------------------------------------------------------

// <copyright file="IAssistantViewer.cs" company="Breanos GmbH">
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
using System.Threading.Tasks;

namespace AssistantViewerInterfaces
{
    public interface IAssistantViewer
    {
        /// <summary>
        /// Will be called by the server if a login attempt could not be completed
        /// This can have multiple causes, for instance:
        /// wrong password, username not recognized, access control service not reachable, identity provider not reachable,...
        /// </summary>
        /// <returns></returns>
        Task OnReceiveBadLogin();
        /// <summary>
        /// Will be called by the server if a login attempt was successfully completed
        /// </summary>
        /// <returns></returns>
        Task OnReceiveGoodLogin();
        /// <summary>
        /// Will be called by the server if a view link should be added to the master navigation list of the viewer.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="viewId"></param>
        /// <returns></returns>
        Task OnAddToMasterList(string title, string viewId);
        /// <summary>
        /// Will be called by the server if a view link should be removed from the master navigation list of the viewer.
        /// </summary>
        /// <param name="viewId"></param>
        /// <returns></returns>
        Task OnRemoveFromMasterList(string viewId);
        /// <summary>
        /// Will be called by the server to send text messages to the client that would typically be displyed as pop-ups with a certain array of buttons.
        /// </summary>
        /// <param name="message">the message text</param>
        /// <param name="buttons">the array of buttons' texts</param>
        /// <returns></returns>
        Task OnMessageReceived(string message, string[] buttons);

        /// <summary>
        /// will be called by the server to notify the viewer to update certain properties on certain views with new values
        /// Each of the arrays must have the same length
        /// </summary>
        /// <param name="views"></param>
        /// <param name="items"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        Task OnBatchDataReceived(string viewId, string[] items, string[] data, DateTime[] timestampsUtc);
        /// <summary>
        /// Will be called by the server if the client requests a certain view be displayed.
        /// </summary>
        /// <param name="viewId">The requested viewId</param>
        /// <param name="xaml">The xaml description of the view</param>
        /// <returns></returns>
        Task OnPackageReceived(string viewId, string data);
        /// <summary>
        /// Will be called by the server right after good login
        /// </summary>
        /// <param name="menuXml"></param>
        /// <returns></returns>
        Task OnMenuReceived(string menuXml);

    }
}
