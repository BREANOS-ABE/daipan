//-----------------------------------------------------------------------

// <copyright file="MessageConverter.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Tuesday, October 30, 2018 1:26:47 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using Daipan.Core.Messaging.Contracts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Daipan.Core.Messaging.General
{
  /// <summary>
  /// Basic message converter.
  /// </summary>
  public class MessageConverter : IMessageConverter
  {
    #region Generic conversion

    /// <summary>
    /// Auxiliary interface for generic handling.
    /// </summary>
    private interface IConversionHandler { }


    /// <summary>
    /// Auxiliary class for generic handling.
    /// </summary>
    /// <typeparam name="T">Type of the values handeled.</typeparam>
    private class ConversionHandler<T> : IConversionHandler
    {
      public Func<byte[], int, T> GetMethod { get; set; }

      public Action<byte[], T, int> SetMethod { get; set; }
    }

    /// <summary>
    /// Dictionary for managing pairs of types and conversion handels.
    /// </summary>
    private Dictionary<Type, IConversionHandler> _genericHandlers = new Dictionary<Type, IConversionHandler>();

    /// <summary>
    /// Adds handles for a type conversion.
    /// </summary>
    /// <typeparam name="T">Target type of the conversion.</typeparam>
    /// <param name="getMethod">Getter method, that converts a byte array into the target type.</param>
    /// <param name="setMethod">Setter method, that converts a value of the target type into a byte array.</param>
    protected void AddTypeConversion<T>(Func<byte[], int, T> getMethod, Action<byte[], T, int> setMethod)
    {
      if (!_genericHandlers.ContainsKey(typeof(T)))
      {
        _genericHandlers.Add(
            typeof(T),
            new ConversionHandler<T>()
            {
              GetMethod = getMethod,
              SetMethod = setMethod
            });
      }
    }

    #endregion

    /// <summary>
    /// Initialize the basic classes.
    /// </summary>
    public MessageConverter()
    {
      SwapByteOrder = false;

      AddTypeConversion<byte>((data, index) => data[index], (data, value, index) => data[index] = value);
      AddTypeConversion<Int16>(GetInt16, SetInt16);
      AddTypeConversion<Int32>(GetInt32, SetInt32);
      AddTypeConversion<Int64>(GetInt64, SetInt64);
      AddTypeConversion<UInt16>(GetUInt16, SetUInt16);
      AddTypeConversion<UInt32>(GetUInt32, SetUInt32);
      AddTypeConversion<UInt64>(GetUInt64, SetUInt64);
      AddTypeConversion<float>(GetIeee754Float, SetIeee754Float);
      AddTypeConversion<double>(GetIeee754Double, SetIeee754Double);
      AddTypeConversion<DateTime>(GetDateTime, SetDateTime);
    }

    /// <summary>
    /// Flag whether the byte order of each value in a message has to be swaped or not.
    /// </summary>
    public bool SwapByteOrder { get; protected set; }

    #region Implementation of IMessageConverter

    /// <summary>
    /// Gets a single bit out of the byte array.
    /// </summary>
    /// <param name="data">Binary data stream, within the value should be read.</param>
    /// <param name="index">Index, of byte that should be read.</param>
    /// <param name="bitIndex">Index within the byte.</param>
    /// <returns></returns>
    public virtual bool GetBool(byte[] data, int index, int bitIndex)
    {
      return (data[index] & ((byte)1 << bitIndex)) > 0;
    }

    /// <summary>
    /// Sets a single bit in a byte array.
    /// </summary>
    /// <param name="data">Binary data stream, in that should be written.</param>
    /// <param name="value">Binary value that is written to the byte stream.</param>
    /// <param name="index">Index, of byte that should be written.</param>
    /// <param name="bitIndex">Index within the byte.</param>
    public virtual void SetBool(byte[] data, bool value, int index, int bitIndex)
    {
      // get a bit mask where the Bit is set
      // 0000 0001 shifted by the bit offset
      int helpVal = 1 << bitIndex;

      // if set to true use OR
      if (value) data[index] |= (byte)helpVal;

      // if set to false use AND with inverted mask
      else data[index] &= (byte)~helpVal;
    }

    /// <summary>
    /// Reads a <see cref="Int16"/> from a byte array.
    /// </summary>
    /// <param name="data">Binary data stream, within the value should be read.</param>
    /// <param name="index">Byte position of the values within the array.</param>
    /// <returns>Deserialized value</returns>
    public virtual short GetInt16(byte[] data, int index)
    {
      // Convert buffer section into a Int16
      return BitConverter.ToInt16(GetConversionArray(data, index, sizeof(Int16)), 0);
    }

    /// <summary>
    /// Writes a <see cref="Int16"/> to a byte array.
    /// </summary>
    /// <param name="data">Binary data stream, in which should be written.</param>
    /// <param name="value">Value that should be seralized and written to the binary stream.</param>
    /// <param name="index">Byte position of the values within the array.</param>
    public virtual void SetInt16(byte[] data, short value, int index)
    {
      // Convert to byte array and copy
      SetValueType(data, BitConverter.GetBytes(value), index);
    }

    /// <summary>
    /// Reads a <see cref="Int32"/> from a byte array.
    /// </summary>
    /// <param name="data">Binary data stream, within the value should be read.</param>
    /// <param name="index">Byte position of the values within the array.</param>
    /// <returns>Deserialized value</returns>
    public virtual int GetInt32(byte[] data, int index)
    {
      // Convert buffer section into a Int32
      return BitConverter.ToInt32(GetConversionArray(data, index, sizeof(Int32)), 0);
    }

    /// <summary>
    /// Writes a <see cref="Int32"/> to a byte array.
    /// </summary>
    /// <param name="data">Binary data stream, in which should be written.</param>
    /// <param name="value">Value that should be seralized and written to the binary stream.</param>
    /// <param name="index">Byte position of the values within the array.</param>
    public virtual void SetInt32(byte[] data, int value, int index)
    {
      // Convert to byte array and copy
      SetValueType(data, BitConverter.GetBytes(value), index);
    }

    /// <summary>
    /// Reads a <see cref="Int64"/> from a byte array.
    /// </summary>
    /// <param name="data">Binary data stream, within the value should be read.</param>
    /// <param name="index">Byte position of the values within the array.</param>
    /// <returns>Deserialized value</returns>
    public virtual long GetInt64(byte[] data, int index)
    {
      // Convert buffer section into a Int64
      return BitConverter.ToInt64(GetConversionArray(data, index, sizeof(Int64)), 0);
    }

    /// <summary>
    /// Writes a <see cref="Int64"/> to a byte array.
    /// </summary>
    /// <param name="data">Binary data stream, in which should be written.</param>
    /// <param name="value">Value that should be seralized and written to the binary stream.</param>
    /// <param name="index">Byte position of the values within the array.</param>
    public virtual void SetInt64(byte[] data, long value, int index)
    {
      // Convert to byte array and copy
      SetValueType(data, BitConverter.GetBytes(value), index);
    }

    /// <summary>
    /// Reads a <see cref="UInt16"/> from a byte array.
    /// </summary>
    /// <param name="data">Binary data stream, within the value should be read.</param>
    /// <param name="index">Byte position of the values within the array.</param>
    /// <returns>Deserialized value</returns>
    public virtual ushort GetUInt16(byte[] data, int index)
    {
      // Convert buffer section into a UInt16
      return BitConverter.ToUInt16(GetConversionArray(data, index, sizeof(UInt16)), 0);
    }

    /// <summary>
    /// Writes a <see cref="UInt16"/> to a byte array.
    /// </summary>
    /// <param name="data">Binary data stream, in which should be written.</param>
    /// <param name="value">Value that should be seralized and written to the binary stream.</param>
    /// <param name="index">Byte position of the values within the array.</param>
    public virtual void SetUInt16(byte[] data, ushort value, int index)
    {
      // Convert to byte array and copy
      SetValueType(data, BitConverter.GetBytes(value), index);
    }

    /// <summary>
    /// Reads a <see cref="UInt32"/> from a byte array.
    /// </summary>
    /// <param name="data">Binary data stream, within the value should be read.</param>
    /// <param name="index">Byte position of the values within the array.</param>
    /// <returns>Deserialized value</returns>
    public virtual uint GetUInt32(byte[] data, int index)
    {
      // Convert buffer section into a UInt32
      return BitConverter.ToUInt32(GetConversionArray(data, index, sizeof(UInt32)), 0);
    }

    /// <summary>
    /// Writes a <see cref="UInt32"/> to a byte array.
    /// </summary>
    /// <param name="data">Binary data stream, in which should be written.</param>
    /// <param name="value">Value that should be seralized and written to the binary stream.</param>
    /// <param name="index">Byte position of the values within the array.</param>
    public virtual void SetUInt32(byte[] data, uint value, int index)
    {
      // Convert to byte array and copy
      SetValueType(data, BitConverter.GetBytes(value), index);
    }

    /// <summary>
    /// Reads a <see cref="UInt64"/> from a byte array.
    /// </summary>
    /// <param name="data">Binary data stream, within the value should be read.</param>
    /// <param name="index">Byte position of the values within the array.</param>
    /// <returns>Deserialized value</returns>
    public virtual ulong GetUInt64(byte[] data, int index)
    {
      // Convert buffer section into a UInt64
      return BitConverter.ToUInt64(GetConversionArray(data, index, sizeof(UInt64)), 0);
    }

    /// <summary>
    /// Writes a <see cref="UInt64"/> to a byte array.
    /// </summary>
    /// <param name="data">Binary data stream, in which should be written.</param>
    /// <param name="value">Value that should be seralized and written to the binary stream.</param>
    /// <param name="index">Byte position of the values within the array.</param>
    public virtual void SetUInt64(byte[] data, ulong value, int index)
    {
      // Convert to byte array and copy
      SetValueType(data, BitConverter.GetBytes(value), index);
    }

    /// <summary>
    /// Reads a <see cref="float"/> from a byte array.
    /// </summary>
    /// <param name="data">Binary data stream, within the value should be read.</param>
    /// <param name="index">Byte position of the values within the array.</param>
    /// <returns>Deserialized value</returns>
    public virtual float GetIeee754Float(byte[] data, int index)
    {
      // Convert buffer section into a float
      return BitConverter.ToSingle(GetConversionArray(data, index, sizeof(Single)), 0);
    }

    /// <summary>
    /// Writes a <see cref="float"/> to a byte array.
    /// </summary>
    /// <param name="data">Binary data stream, in which should be written.</param>
    /// <param name="value">Value that should be seralized and written to the binary stream.</param>
    /// <param name="index">Byte position of the values within the array.</param>
    public virtual void SetIeee754Float(byte[] data, float value, int index)
    {
      // Convert to byte array and copy
      SetValueType(data, BitConverter.GetBytes(value), index);
    }

    /// <summary>
    /// Reads a <see cref="double"/> from a byte array.
    /// </summary>
    /// <param name="data">Binary data stream, within the value should be read.</param>
    /// <param name="index">Byte position of the values within the array.</param>
    /// <returns>Deserialized value</returns>
    public virtual double GetIeee754Double(byte[] data, int index)
    {
      // Convert buffer section into a double
      return BitConverter.ToDouble(GetConversionArray(data, index, sizeof(double)), 0);
    }

    /// <summary>
    /// Writes a <see cref="double"/> to a byte array.
    /// </summary>
    /// <param name="data">Binary data stream, in which should be written.</param>
    /// <param name="value">Value that should be seralized and written to the binary stream.</param>
    /// <param name="index">Byte position of the values within the array.</param>
    public virtual void SetIeee754Double(byte[] data, double value, int index)
    {
      // Convert to byte array and copy
      SetValueType(data, BitConverter.GetBytes(value), index);
    }

    /// <summary>
    /// Reads a <see cref="string"/> from a byte array.
    /// </summary>
    /// <param name="data">Binary data stream, within the value should be read.</param>
    /// <param name="index">Byte position of the values within the array.</param>
    /// <param name="length">[Optional] Length of the string that should be read out of the byte stream. Default -1 - converts all bytes till to the end of the stream.</param>
    /// <param name="encoding">[Optional] Encoding for the conversion. Default ASCII</param>
    /// <returns>Deserialized value</returns>
    public virtual string GetString(byte[] data, int index, int length = -1, System.Text.Encoding encoding = null, string stringMaskRegex = "[^a-zA-Z0-9\\._\\-#\\+\\?! ]")
    {
      // Has to be this way, because default values has be compiler constants.
      if (encoding == null) encoding = Encoding.ASCII;
      Regex rgx = new Regex(stringMaskRegex);
      if (length < 0) length = data.Length - index;
      // Convert buffer section into a string
      if (data.Length > index)
      {
        if (data.Length >= (index + length))
          return rgx.Replace(encoding.GetString(data, index, length), "");
        else
          return rgx.Replace(encoding.GetString(data, index, data.Length - index), "");
      }
      else
      {
        return "";
      }
    }

    /// <summary>
    /// Writes a <see cref="string"/> to a byte array.
    /// </summary>
    /// <param name="data">Binary data stream, in which should be written.</param>
    /// <param name="value">Value that should be seralized and written to the binary stream.</param>
    /// <param name="index">Byte position of the values within the array.</param>
    /// <param name="length">[Optional] Length of the string that should be written to the byte stream. Default -1 - writes the whole string.</param>
    /// <param name="encoding">[Optional] Encoding for the conversion. Default ASCII</param>
    public virtual void SetString(byte[] data, string value, int index, Int32 length = -1, System.Text.Encoding encoding = null)
    {
      // Has to be this way, because default values has be compiler constants.
      if (encoding == null) encoding = Encoding.ASCII;

      // get max length
      int maxLen = ((length <= 0 || length > value.Length) ? value.Length : length);
      if (maxLen + index >= data.Length) maxLen = data.Length - index;

      // Convert to byte array and copy
      if (maxLen > 0) encoding.GetBytes(value, 0, maxLen, data, index);
    }

    /// <summary>
    /// Reads a <see cref="DateTime"/> from a byte array.
    /// </summary>
    /// <param name="data">Binary data stream, within the value should be read.</param>
    /// <param name="index">Byte position of the values within the array.</param>
    /// <returns>Deserialized value</returns>
    public virtual DateTime GetDateTime(byte[] data, int index)
    {
      // Convert buffer section into a Int64 (ticks) and create a date time
      return new DateTime(BitConverter.ToInt64(GetConversionArray(data, index, sizeof(Int64)), 0));
    }

    /// <summary>
    /// Writes a <see cref="DateTime"/> to a byte array.
    /// </summary>
    /// <param name="data">Binary data stream, in which should be written.</param>
    /// <param name="value">Value that should be seralized and written to the binary stream.</param>
    /// <param name="index">Byte position of the values within the array.</param>
    public virtual void SetDateTime(byte[] data, DateTime value, int index)
    {
      // Convert to byte array and copy
      SetValueType(data, BitConverter.GetBytes(value.Ticks), index);
    }

    /// <summary>
    /// Converts a part of the buffer into a BCD value
    /// </summary>
    /// <param name="data">Source byte stream</param>
    /// <param name="index">Begin of the conversion section in bytes</param>
    /// <param name="length">Length of the conversion section in bytes</param>
    /// <returns>The converted BCD val</returns>
    /// <remarks>
    /// If "Length" is negative or zero the rest of the stream is converted
    /// Buffer format is "big endian" (MSB first)
    /// </remarks>
    public virtual BcdValue GetBcd(byte[] data, int index, int length = -1)
    {
      long retVal = 0;    // return Value 
      byte valueHigh = 0;     // most significant Value (4Bit) of a byte
      byte valueLow = 0;      // least significant Value (4Bit) of a byte

      // Check length; set length to the rest length of the stream
      if (length <= 0) length = data.Length - index;

      // iterate through the stream
      // Attention: in S7 - BCD is swapped
      for (int i = 0; i < length; i++)
      {
        // Convert the least 4 Bit of the val
        valueLow = Convert.ToByte(data[index + i] & 0x0F);

        // Convert the first 4 Bit of the val
        valueHigh = Convert.ToByte((data[index + i] & 0xF0) >> 4);

        // Sum it to a decimal val
        retVal = retVal * 100 + valueHigh * 10 + valueLow;
      }

      return retVal;
    }

    /// <summary>
    /// Copies a BCD val into a byte stream, format is "big endian" (MSB first)
    /// </summary>
    /// <param name="Value">Value to be set to the buffer</param>
    /// <param name="index">Offset in bytes where the val should be insert</param>
    /// <param name="Length">Digits of the BCD val</param>
    public virtual void SetBcd(byte[] data, BcdValue value, int index, int length)
    {
      SetValueType(data, value.GetBytes(length), index);
    }

    /// <summary>
    /// Checks if the converter can handle that <see cref="Type"/>
    /// </summary>
    /// <param name="type">Type that should be converted.</param>
    /// <returns><i>True</i>, if the <see cref="Type"/> can be converted, otherwise <i>false</i>.</returns>
    public virtual bool CanConvert(Type type)
    {
      return _genericHandlers.ContainsKey(type);
    }

    /// <summary>
    /// Reads a generic value from a byte array.
    /// </summary>
    /// <typeparam name="T">Type of the value, that should be read from the binary stream.</typeparam>
    /// <param name="data">Binary data stream, within the value should be read.</param>
    /// <param name="index">Byte position of the values within the array.</param>
    /// <returns>Deserialized value</returns>
    public virtual T GetValue<T>(byte[] data, int index)
    {
      ConversionHandler<T> converter = _genericHandlers[typeof(T)] as ConversionHandler<T>;

      if (converter != null)
        return converter.GetMethod(data, index);
      else
        throw new ArgumentException();
    }

    /// <summary>
    /// Writes a generic value to a byte array.
    /// </summary>
    /// <param name="data">Binary data stream, in which should be written.</param>
    /// <param name="value">Value that should be seralized and written to the binary stream.</param>
    /// <param name="index">Byte position of the values within the array.</param>
    public virtual void SetValue<T>(byte[] data, T value, int index)
    {
      ConversionHandler<T> converter = _genericHandlers[typeof(T)] as ConversionHandler<T>;

      if (converter != null)
        converter.SetMethod(data, value, index);
      else
        throw new ArgumentException();
    }

    #endregion

    /// <summary>
    /// Get the part of the data stream, that is essential for the conversion. Swap the byte order if neccesary.
    /// </summary>
    /// <param name="data">Original binary data stream.</param>
    /// <param name="index">Index where the sub array starts.</param>
    /// <param name="length">Length of the sub array.</param>
    /// <returns>Sub array for conversion, that is swaped if configured.</returns>
    protected byte[] GetConversionArray(byte[] data, Int32 index, Int32 length)
    {
      // Get needed part of the buffer
      byte[] tempBuffer = new byte[length];

      // if smaller zero - copy the rest of the array
      if (length < 0) length = data.Length - index;

      Array.Copy(data, index, tempBuffer, 0, length);

      // Invert the the byte order if neccesary
      if (SwapByteOrder) Array.Reverse(tempBuffer);

      return tempBuffer;
    }

    /// <summary>
    /// Copies a value array to a data stream, swaps it before if neccesary.
    /// </summary>
    /// <param name="data">The original data stream (target of the copy).</param>
    /// <param name="value">The value array (source of the copy).</param>
    /// <param name="index">Start index in the target array.</param>
    protected void SetValueType(byte[] data, byte[] value, int index)
    {
      // Invert the the byte order if neccesary
      if (SwapByteOrder) Array.Reverse(value);

      // Copy bytes to buffer
      value.CopyTo(data, index);
    }

  }
}
