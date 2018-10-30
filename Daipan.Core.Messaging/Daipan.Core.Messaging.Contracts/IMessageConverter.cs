//-----------------------------------------------------------------------

// <copyright file="IMessageConverter.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Tuesday, October 30, 2018 1:26:47 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using System;
using System.Text;

namespace Daipan.Core.Messaging.Contracts
{
  /// <summary>
  /// Basic functionality of a message en- or decoder.
  /// </summary>
  public interface IMessageConverter
  {
    /// <summary>
    /// Gets a single bit out of the byte array.
    /// </summary>
    /// <param name="data">Binary data stream, within the value should be read.</param>
    /// <param name="index">Index, of byte that should be read.</param>
    /// <param name="bitIndex">Index within the byte.</param>
    /// <returns></returns>
    bool GetBool(byte[] data, Int32 index, Int32 bitIndex);

    /// <summary>
    /// Sets a single bit in a byte array.
    /// </summary>
    /// <param name="data">Binary data stream, in that should be written.</param>
    /// <param name="value">Binary value that is written to the byte stream.</param>
    /// <param name="index">Index, of byte that should be written.</param>
    /// <param name="bitIndex">Index within the byte.</param>
    void SetBool(byte[] data, bool value, Int32 index, Int32 bitIndex);

    /// <summary>
    /// Reads a <see cref="Int16"/> from a byte array.
    /// </summary>
    /// <param name="data">Binary data stream, within the value should be read.</param>
    /// <param name="index">Byte position of the values within the array.</param>
    /// <returns>Deserialized value</returns>
    Int16 GetInt16(byte[] data, Int32 index);

    /// <summary>
    /// Writes a <see cref="Int16"/> to a byte array.
    /// </summary>
    /// <param name="data">Binary data stream, in which should be written.</param>
    /// <param name="value">Value that should be seralized and written to the binary stream.</param>
    /// <param name="index">Byte position of the values within the array.</param>
    void SetInt16(byte[] data, Int16 value, Int32 index);

    /// <summary>
    /// Reads a <see cref="Int32"/> from a byte array.
    /// </summary>
    /// <param name="data">Binary data stream, within the value should be read.</param>
    /// <param name="index">Byte position of the values within the array.</param>
    /// <returns>Deserialized value</returns>
    Int32 GetInt32(byte[] data, Int32 index);

    /// <summary>
    /// Writes a <see cref="Int32"/> to a byte array.
    /// </summary>
    /// <param name="data">Binary data stream, in which should be written.</param>
    /// <param name="value">Value that should be seralized and written to the binary stream.</param>
    /// <param name="index">Byte position of the values within the array.</param>
    void SetInt32(byte[] data, Int32 value, Int32 index);

    /// <summary>
    /// Reads a <see cref="Int64"/> from a byte array.
    /// </summary>
    /// <param name="data">Binary data stream, within the value should be read.</param>
    /// <param name="index">Byte position of the values within the array.</param>
    /// <returns>Deserialized value</returns>
    Int64 GetInt64(byte[] data, Int32 index);

    /// <summary>
    /// Writes a <see cref="Int64"/> to a byte array.
    /// </summary>
    /// <param name="data">Binary data stream, in which should be written.</param>
    /// <param name="value">Value that should be seralized and written to the binary stream.</param>
    /// <param name="index">Byte position of the values within the array.</param>
    void SetInt64(byte[] data, Int64 value, Int32 index);

    /// <summary>
    /// Reads a <see cref="UInt16"/> from a byte array.
    /// </summary>
    /// <param name="data">Binary data stream, within the value should be read.</param>
    /// <param name="index">Byte position of the values within the array.</param>
    /// <returns>Deserialized value</returns>
    UInt16 GetUInt16(byte[] data, Int32 index);

    /// <summary>
    /// Writes a <see cref="UInt16"/> to a byte array.
    /// </summary>
    /// <param name="data">Binary data stream, in which should be written.</param>
    /// <param name="value">Value that should be seralized and written to the binary stream.</param>
    /// <param name="index">Byte position of the values within the array.</param>
    void SetUInt16(byte[] data, UInt16 value, Int32 index);

    /// <summary>
    /// Reads a <see cref="UInt32"/> from a byte array.
    /// </summary>
    /// <param name="data">Binary data stream, within the value should be read.</param>
    /// <param name="index">Byte position of the values within the array.</param>
    /// <returns>Deserialized value</returns>
    UInt32 GetUInt32(byte[] data, Int32 index);

    /// <summary>
    /// Writes a <see cref="UInt32"/> to a byte array.
    /// </summary>
    /// <param name="data">Binary data stream, in which should be written.</param>
    /// <param name="value">Value that should be seralized and written to the binary stream.</param>
    /// <param name="index">Byte position of the values within the array.</param>
    void SetUInt32(byte[] data, UInt32 value, Int32 index);

    /// <summary>
    /// Reads a <see cref="UInt64"/> from a byte array.
    /// </summary>
    /// <param name="data">Binary data stream, within the value should be read.</param>
    /// <param name="index">Byte position of the values within the array.</param>
    /// <returns>Deserialized value</returns>
    UInt64 GetUInt64(byte[] data, Int32 index);

    /// <summary>
    /// Writes a <see cref="UInt64"/> to a byte array.
    /// </summary>
    /// <param name="data">Binary data stream, in which should be written.</param>
    /// <param name="value">Value that should be seralized and written to the binary stream.</param>
    /// <param name="index">Byte position of the values within the array.</param>
    void SetUInt64(byte[] data, UInt64 value, Int32 index);

    /// <summary>
    /// Reads a <see cref="float"/> from a byte array.
    /// </summary>
    /// <param name="data">Binary data stream, within the value should be read.</param>
    /// <param name="index">Byte position of the values within the array.</param>
    /// <returns>Deserialized value</returns>
    float GetIeee754Float(byte[] data, Int32 index);

    /// <summary>
    /// Writes a <see cref="float"/> to a byte array.
    /// </summary>
    /// <param name="data">Binary data stream, in which should be written.</param>
    /// <param name="value">Value that should be seralized and written to the binary stream.</param>
    /// <param name="index">Byte position of the values within the array.</param>
    void SetIeee754Float(byte[] data, float value, Int32 index);

    /// <summary>
    /// Reads a <see cref="double"/> from a byte array.
    /// </summary>
    /// <param name="data">Binary data stream, within the value should be read.</param>
    /// <param name="index">Byte position of the values within the array.</param>
    /// <returns>Deserialized value</returns>
    double GetIeee754Double(byte[] data, Int32 index);

    /// <summary>
    /// Writes a <see cref="double"/> to a byte array.
    /// </summary>
    /// <param name="data">Binary data stream, in which should be written.</param>
    /// <param name="value">Value that should be seralized and written to the binary stream.</param>
    /// <param name="index">Byte position of the values within the array.</param>
    void SetIeee754Double(byte[] data, double value, Int32 index);

    /// <summary>
    /// Reads a <see cref="string"/> from a byte array.
    /// </summary>
    /// <param name="data">Binary data stream, within the value should be read.</param>
    /// <param name="index">Byte position of the values within the array.</param>
    /// <param name="length">[Optional] Length of the string that should be read out of the byte stream. Default -1 - converts all bytes till to the end of the stream.</param>
    /// <param name="encoding">[Optional] Encoding for the conversion. Default ASCII</param>
    /// <returns>Deserialized value</returns>
    string GetString(byte[] data, Int32 index, Int32 length = -1, Encoding encoding = null, string stringMaskRegex = "[^a-zA-Z0-9\\._\\-#\\+\\?! ]");

    /// <summary>
    /// Writes a <see cref="string"/> to a byte array.
    /// </summary>
    /// <param name="data">Binary data stream, in which should be written.</param>
    /// <param name="value">Value that should be seralized and written to the binary stream.</param>
    /// <param name="index">Byte position of the values within the array.</param>
    /// <param name="length">[Optional] Length of the string that should be written to the byte stream. Default -1 - writes the whole string.</param>
    /// <param name="encoding">[Optional] Encoding for the conversion. Default ASCII</param>
    void SetString(byte[] data, string value, Int32 index, Int32 length = -1, Encoding encoding = null);

    /// <summary>
    /// Reads a <see cref="DateTime"/> from a byte array.
    /// </summary>
    /// <param name="data">Binary data stream, within the value should be read.</param>
    /// <param name="index">Byte position of the values within the array.</param>
    /// <returns>Deserialized value</returns>
    DateTime GetDateTime(byte[] data, Int32 index);

    /// <summary>
    /// Writes a <see cref="DateTime"/> to a byte array.
    /// </summary>
    /// <param name="data">Binary data stream, in which should be written.</param>
    /// <param name="value">Value that should be seralized and written to the binary stream.</param>
    /// <param name="index">Byte position of the values within the array.</param>
    void SetDateTime(byte[] data, DateTime value, Int32 index);

    /// <summary>
    /// Reads a <see cref="BcdValue"/> from a byte array.
    /// </summary>
    /// <param name="data">Binary data stream, within the value should be read.</param>
    /// <param name="index">Byte position of the values within the array.</param>
    /// <param name="length">[Optional] Length of the BCD value that should be read out of the byte stream. Default -1 - converts all bytes till to the end of the stream.</param>
    /// <returns>Deserialized value</returns>
    BcdValue GetBcd(byte[] data, Int32 index, Int32 length = -1);

    /// <summary>
    /// Writes a <see cref="BcdValue"/> to a byte array.
    /// </summary>
    /// <param name="data">Binary data stream, in which should be written.</param>
    /// <param name="value">Value that should be seralized and written to the binary stream.</param>
    /// <param name="index">Byte position of the values within the array.</param>
    /// /// <param name="length">Digits of the BCD value</param>
    void SetBcd(byte[] data, BcdValue value, Int32 index, Int32 length);

    /// <summary>
    /// Checks if the converter can handle that <see cref="Type"/>
    /// </summary>
    /// <param name="type">Type that should be converted.</param>
    /// <returns><i>True</i>, if the <see cref="Type"/> can be converted, otherwise <i>false</i>.</returns>
    bool CanConvert(Type type);

    /// <summary>
    /// Reads a generic value from a byte array.
    /// </summary>
    /// <typeparam name="T">Type of the value, that should be read from the binary stream.</typeparam>
    /// <param name="data">Binary data stream, within the value should be read.</param>
    /// <param name="index">Byte position of the values within the array.</param>
    /// <returns>Deserialized value</returns>
    T GetValue<T>(byte[] data, Int32 index);

    /// <summary>
    /// Writes a generic value to a byte array.
    /// </summary>
    /// <param name="data">Binary data stream, in which should be written.</param>
    /// <param name="value">Value that should be seralized and written to the binary stream.</param>
    /// <param name="index">Byte position of the values within the array.</param>
    void SetValue<T>(byte[] data, T value, Int32 index);
  }
}
