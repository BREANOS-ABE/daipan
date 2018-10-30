//-----------------------------------------------------------------------

// <copyright file="BcdValue.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Tuesday, October 30, 2018 1:26:47 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;

namespace Daipan.Core.Messaging.Contracts
{

    /// <summary>
    /// Represents a BCD value
    /// </summary>
    public struct BcdValue
    {
        long _value;

        private BcdValue(long value)
        {
            _value = value;
        }

        public static implicit operator BcdValue(byte value) { return new BcdValue(Convert.ToInt64(value)); }
        public static implicit operator BcdValue(short value) { return new BcdValue(Convert.ToInt64(value)); }
        public static implicit operator BcdValue(int value) { return new BcdValue(Convert.ToInt64(value)); }
        public static implicit operator BcdValue(long value) { return new BcdValue(Convert.ToInt64(value)); }

        public static implicit operator byte(BcdValue value) { return Convert.ToByte(value._value); }
        public static implicit operator short(BcdValue value) { return Convert.ToInt16(value._value); }
        public static implicit operator int(BcdValue value) { return Convert.ToInt32(value._value); }
        public static implicit operator long(BcdValue value) { return value._value; }

        public static BcdValue operator +(BcdValue value1, BcdValue value2) { return new BcdValue(value1._value + value2._value); }
        public static BcdValue operator -(BcdValue value1, BcdValue value2) { return new BcdValue(value1._value - value2._value); }
        public static BcdValue operator *(BcdValue value1, BcdValue value2) { return new BcdValue(value1._value * value2._value); }
        public static BcdValue operator /(BcdValue value1, BcdValue value2) { return new BcdValue(value1._value / value2._value); }
        public static BcdValue operator %(BcdValue value1, BcdValue value2) { return new BcdValue(value1._value % value2._value); }

        public byte[] GetBytes(int length)
        {
            return GetBytes(length, this);
        }

        public static byte[] GetBytes(int length, BcdValue value)
        {
            byte[] returnValue = new byte[length];

            for (int i = 0; i < length && value > 0; i++)
            {
                // first val Bit 0-3
                returnValue[i] = Convert.ToByte(value % 10);
                value /= 10;

                // second val Bit 4-7
                returnValue[i] |= Convert.ToByte((value % 10) << 4);
                value /= 10;
            }

            return returnValue;
        }
    }
}
