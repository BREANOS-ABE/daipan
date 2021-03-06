﻿//-----------------------------------------------------------------------

// <copyright file="KnowledgeSourceBase.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Monday, December 3, 2018 3:34:35 PM</date>
// </copyright>

//-----------------------------------------------------------------------

///////////////////////////////////////////////////////////
//  KnowledgeSourceBase.cs
//  Implementation of the Class KnowledgeSourceBase
//  Generated by Enterprise Architect
//  Created on:      02-Feb-2018 10:26:57
//  Original author: bezdedeanu
///////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using NLog;
using AssistantUtilities;

namespace BlackboardClassLibrary.KnowledgeSources
{
    /// <summary>
    /// Abstracte Basisklasse für eine KS
    /// </summary>
    public abstract class KnowledgeSourceBase : IKnowledgeSource
    {
        /// <summary>
        /// Logger variable
        /// </summary>
        //private static Logger logger = LogManager.GetCurrentClassLogger();
        private static BreanosLogger logger;

        /// <summary>
        /// Referenz auf das Blackboard
        /// </summary>
        internal Blackboard.Blackboard blackboard;

        /// <summary>
        /// Öffentlicher Konstruktor
        /// </summary>
        public KnowledgeSourceBase()
        {
            if (logger == null) logger = BreanosLoggerFactory.DuplicateGet(Blackboard.Blackboard.BlackboardLoggerKey, nameof(KnowledgeSourceBase));
        }

        /// <summary>
        /// Destruktor
        /// </summary>
        ~KnowledgeSourceBase()
        {

        }

        /// <summary>
        /// Jede KS kennt das Blackboard
        /// </summary>
        /// <param name="blackboard"></param>
        public virtual void Configure(Blackboard.Blackboard blackboard)
        {
            logger.Debug("KnowledgeSourceBase.Configure called");
            this.blackboard = blackboard;
        }

        /// <summary>
        /// Wird konkret in jeder KU implementiert
        /// </summary>
        public virtual void ExecuteAction()
        {

        }

        /// <summary>
        /// KUs können abgeschalten werden.
        /// </summary>
        public virtual bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Legt den KS Type fest
        /// </summary>
        public abstract KnowledgeSourceType KSType { get; }

        /// <summary>
        /// Jede KS hat eine Priorität
        /// </summary>
        public virtual KnowledgeSourcePriority Priority { get; set; }

    }//end KnowledgeSourceBase
}
