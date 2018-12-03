//-----------------------------------------------------------------------

// <copyright file="TypedModelUpdateBatcher.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Monday, December 3, 2018 3:34:35 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using BreanosConnectors.Kpu.Communication.Common;
using BreanosConnectors.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BreanosConnectors.Kpu.Communication.Utilities
{
    public class TypedModelUpdateBatcher
    {

        SizeBatcher<TypedModelUpdate> _sizeBatcher;

        #region private fields
        private bool _isCollectionMode;                             //boolean switch for OnMessage between starting a new bag and just adding to an existing one
        int _delayMilliseconds;
        private object _onMessageModeLock = new object();           //concurrency lock for mode change of _isCollectionMode
        private object _batchLock = new object();                   //concurrency lock for bag alteration
        private ConcurrentDictionary<string, TypedModelUpdate> _batch;                              //the batch of items to send after the specified time has passed
        private Action<IEnumerable<TypedModelUpdate>> _collectionFinishedAction;   //callback to use for forwarding the messages
        private Task _collectionWaitTask;                           //the task started by the first message of a batch that waits for a specified time before sending the batch
        #endregion
        #region c'tor
        public TypedModelUpdateBatcher(Action<IEnumerable<TypedModelUpdate>> batchSendAction, int delayMilliseconds = 100, int maxBatchSize = int.MaxValue)
        {
            _batch = new ConcurrentDictionary<string, TypedModelUpdate>();
            _sizeBatcher = new SizeBatcher<TypedModelUpdate>(batchSendAction, maxBatchSize);
            _delayMilliseconds = delayMilliseconds;
        }
        #endregion
        #region public methods
        /// <summary>
        /// Receives a ModelUpdate for delivery after the current waiting delay has completed.
        /// ModelUpdates will NOT be sent out if they are superseded by new updates for the same property with greater TimestampUtc
        /// </summary>
        /// <param name="mu"></param>
        public void OnMessage(TypedModelUpdate message)
        {
            lock (_onMessageModeLock)
            {
                if (_isCollectionMode)
                {
                    lock (_batchLock)
                    {
                        _batch.AddOrUpdate(message.Property, message, (key, old) => (old.TimestampUtc > message.TimestampUtc) ? (old) : (message));
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
        private async Task StartCollect(TypedModelUpdate message)
        {
            lock (_batchLock)
            {
                _batch.AddOrUpdate(message.Property, message, (key, old) => (old.TimestampUtc > message.TimestampUtc) ? (old) : (message));

            }
            await Task.Delay(_delayMilliseconds);
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
        private void SendAndClearBag()
        {
            if (_batch.Count > 0)
                _sizeBatcher.OnBatch(_batch.Values);
            _batch = new ConcurrentDictionary<string, TypedModelUpdate>();
        }
        #endregion

    }
}
