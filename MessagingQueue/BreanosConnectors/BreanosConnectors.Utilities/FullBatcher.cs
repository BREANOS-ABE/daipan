//-----------------------------------------------------------------------

// <copyright file="FullBatcher.cs" company="Breanos GmbH">
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

namespace BreanosConnectors.Utilities
{
    /// <summary>
    /// A wrapper class for TimedBatcher and SizeBatcher, linking them with a TimeBatcher first and the SizeBatcher second.
    /// </summary>
    /// <typeparam name="T">The type of messages this batcher can process</typeparam>
    public class FullBatcher<T>
    {
        private TimeBatcher<T> _timed;
        private SizeBatcher<T> _sized;
        public FullBatcher(Action<IEnumerable<T>> batchSendAction, int delayMilliseconds = 100, int maxBatchSize = int.MaxValue)
        {
            _sized = new SizeBatcher<T>(batchSendAction, maxBatchSize);
            _timed = new TimeBatcher<T>(_sized.OnBatch, delayMilliseconds);
        }
        public void OnMessage(T message)
        {
            _timed.OnMessage(message);
        }
    }
}
