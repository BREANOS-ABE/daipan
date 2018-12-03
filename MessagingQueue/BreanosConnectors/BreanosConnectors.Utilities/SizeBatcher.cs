//-----------------------------------------------------------------------

// <copyright file="SizeBatcher.cs" company="Breanos GmbH">
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

namespace BreanosConnectors.Utilities
{

    /// <summary>
    /// A utility class useful for lowering the amound of items in a call over an interface
    /// at the cost of increasing the amount of calls
    /// The maxmimum batch size can be specified.
    /// </summary>
    /// <typeparam name="T">The type of messages this batcher can process</typeparam>
    public class SizeBatcher<T>
    {
        /// <summary>
        /// The maximum batch size to send out
        /// </summary>
        public int BatchSize { get; set; }

        private Action<IEnumerable<T>> _batchSendAction; // the action to perform for each batch

        public SizeBatcher(Action<IEnumerable<T>> batchSendAction, int batchsize = 500)
        {
            BatchSize = batchsize;
            _batchSendAction = batchSendAction;
        }

        /// <summary>
        /// Receives a batch of messages and redirects them into batches of the specified maximum size to then send them out
        /// </summary>
        /// <param name="batch">the incoming batch</param>
        public void OnBatch(IEnumerable<T> batch)
        {
            int countSubBatches = batch.Count() / BatchSize + 1;
            int i = 0;
            var subBatches = from item in batch
                             group item by i++ % countSubBatches into partial
                             select partial.AsEnumerable();
            foreach (var subBatch in subBatches)
            {
                _batchSendAction?.Invoke(subBatch);
            }
        }
    }

}
