//-----------------------------------------------------------------------

// <copyright file="Batcher.cs" company="Breanos GmbH">
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
using System.Threading.Tasks;

namespace AssistantUtilities
{
    /// <summary>
    /// A utility class useful for lowering the amount of calls over an interface
    /// at the cost of increasing the call size.
    /// A delay from each first incoming message to the point of forwarding can be specified.
    /// This will introduce a delay of at most the specified amount of milliseconds + some processing time 
    /// into the communication path.
    /// </summary>
    /// <example>
    /// Batcher&lt;Payload&gt; b = new Batcher&lt;Payload&gt;(r.Receive, 50);
    /// </example>
    /// <typeparam name="T">The type of messages this batcher can process</typeparam>
    public class TimeBatcher<T>
    {
        #region public properties
        /// <summary>
        /// The amount of time in milliseconds to wait before forwarding a batch of items
        /// </summary>
        public int DelayMilliseconds { get; set; }
        public int MaxBatchSize { get; set; }
        #endregion
        #region private fields
        private bool _isCollectionMode;                             //boolean switch for OnMessage between starting a new bag and just adding to an existing one
        private object _onMessageModeLock = new object();           //concurrency lock for mode change of _isCollectionMode
        private object _batchLock = new object();                   //concurrency lock for bag alteration
        private ICollection<T> _batch;                              //the batch of items to send after the specified time has passed
        private Action<IEnumerable<T>> _collectionFinishedAction;   //callback to use for forwarding the messages
        private Task _collectionWaitTask;                           //the task started by the first message of a batch that waits for a specified time before sending the batch
        #endregion
        #region c'tor
        public TimeBatcher(Action<IEnumerable<T>> collectionFinishedAction, int delayMilliseconds = 100, int maxBatchSize = 100)
        {
            _collectionFinishedAction = collectionFinishedAction;
            MaxBatchSize = maxBatchSize;
            DelayMilliseconds = delayMilliseconds;
            _batch = new List<T>();
        }
        #endregion
        #region public methods
        /// <summary>
        /// Receives a message and puts it into a batch that is forwarded after a specified maximum amount of time
        /// </summary>
        /// <param name="message"></param>
        public void OnMessage(T message)
        {
            lock (_onMessageModeLock)
            {
                if (_isCollectionMode)
                {
                    lock (_batchLock)
                    {
                        _batch.Add(message);
                    }
                }
                else
                {
                    _isCollectionMode = true;
                    _collectionWaitTask = StartCollect(message);
                }
            }
        }
        #endregion
        #region private methods
        /// <summary>
        /// This method is started by the first message of a batch and waits for a specified amount of time before forwarding the entire batch.
        /// </summary>
        /// <param name="message">The first message</param>
        /// <returns></returns>
        async Task StartCollect(T message)
        {
            lock (_batchLock)
            {
                _batch.Add(message);

            }
            await Task.Delay(DelayMilliseconds);
            lock (_onMessageModeLock)
            {
                lock (_batchLock)
                {
                    SendAndClearBag();
                }
                _isCollectionMode = false;
            }
        }
        /// <summary>
        /// Sending the current batch and creating a new one for future use should be handled as a single action so it's put together in this method
        /// </summary>
        void SendAndClearBag()
        {
            if (_batch.Count > 0)
                _collectionFinishedAction?.Invoke(_batch);
            _batch = new List<T>();
        }
        #endregion
    }

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
