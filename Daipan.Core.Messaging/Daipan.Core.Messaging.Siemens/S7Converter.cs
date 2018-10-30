//-----------------------------------------------------------------------

// <copyright file="S7Converter.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Tuesday, October 30, 2018 1:26:47 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using Daipan.Core.Messaging.General;
using System;

namespace Daipan.Core.Messaging.Siemens
{
    /// <summary>
    /// Converter for S7 messages.
    /// </summary>
    public class S7Converter : MessageConverter
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public S7Converter()
        {
            SwapByteOrder = true;
        }

        /// <summary>
        /// Reads a <see cref="DateTime"/> from a byte array.
        /// </summary>
        /// <param name="data">Binary data stream, within the value should be read.</param>
        /// <param name="index">Byte position of the values within the array.</param>
        /// <returns>Deserialized value</returns>
        public override DateTime GetDateTime(byte[] data, int index)
        {
            // For DATE_AND_TIME structure: see the class description header
            int year = GetBcd(data, index, 1);
            int month = GetBcd(data, index + 1, 1);
            int day = GetBcd(data, index + 2, 1);
            int hour = GetBcd(data, index + 3, 1); 
            int minute = GetBcd(data, index + 4, 1); 
            int second = GetBcd(data, index + 5, 1); 
            int mSecond = GetBcd(data, index + 6, 2) / 10;

            //Convert year: >90 -> 1900 + year; <90 -> 2000 + year
            if (year >= 90) year += 1900;
            else year += 2000;

            //Return the converted DateTime
            return new DateTime(year, month, day, hour, minute, second).AddMilliseconds(mSecond);
        }

        /// <summary>
        /// Writes a <see cref="DateTime"/> to a byte array.
        /// </summary>
        /// <param name="data">Binary data stream, in which should be written.</param>
        /// <param name="value">Value that should be seralized and written to the binary stream.</param>
        /// <param name="index">Byte position of the values within the array.</param>
        public override void SetDateTime(byte[] data, DateTime value, int index)
        {
            SetBcd(data, value.Year % 100, index, 1);
            SetBcd(data, value.Month, index + 1, 1);
            SetBcd(data, value.Day, index + 2, 1);
            SetBcd(data, value.Hour, index + 3, 1);
            SetBcd(data, value.Minute, index + 4, 1);
            SetBcd(data, value.Second, index + 5, 1);
            SetBcd(data, value.Millisecond * 10, index + 6, 2);
        }

    }

}
