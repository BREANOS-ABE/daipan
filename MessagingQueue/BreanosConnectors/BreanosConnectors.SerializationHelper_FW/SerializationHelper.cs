//-----------------------------------------------------------------------

// <copyright file="SerializationHelper.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Tuesday, October 30, 2018 1:26:47 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using System;
using System.IO;
using System.IO.Compression;
//using System.IO.Compression;
using System.Text;
using System.Xml.Serialization;

namespace BreanosConnectors
{
    public static class SerializationHelper
    {
        /// <summary>
        /// Serializes a given object into Xml
        /// If the object cannot be serialized, this will throw an exception
        /// <para>Reasons why the object cannot be serialized may include</para>
        /// <para>You have a Dictionary property in your object. Use a backing array of (string,SomeClass), i.e. fields in C# 7.0 which you can use as a dictionary via a Property Getter with XmlIgnoreAttribute</para>
        /// <para>You are using interfaces instead of classes for your properties, e.g. IEnumerable, IMyInterface instead of List, MyConcreteImplementation. We would discourage the usage of complex inheritance patterns for DTOs</para>
        /// <para>You have a property of a type which internally violates one of the rules above</para>
        /// </summary>
        /// <typeparam name="T">usually inferred automatically, the type of the object you wish to serialize</typeparam>
        /// <param name="o">the object you wish to serialize</param>
        /// <returns>a serialized representation of the object you wish to serialize</returns>
        public static string Serialize<T>(T o)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, o);
                return writer.ToString();
            }
        }
        /// <summary>
        /// Attempts to deserialize a given Xml-compliant string into an object of a class
        /// If the object cannot be serialized, an exception may be thrown.
        /// <para>Reasons why the object cannot be deserialized may include</para>
        /// <para>You have a Dictionary property in your object. Use a backing array of (string,SomeClass), i.e. fields in C# 7.0 which you can use as a dictionary via a Property Getter with XmlIgnoreAttribute</para>
        /// <para>You are using interfaces instead of classes for your properties, e.g. IEnumerable, IMyInterface instead of List, MyConcreteImplementation. We would discourage the usage of complex inheritance patterns for DTOs</para>
        /// <para>You have a property of a type which internally violates one of the rules above</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="s"></param>
        /// <param name="o"></param>
        /// <returns></returns>
        public static bool TryDeserialize<T>(string s, out T o)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (var reader = new StringReader(s))
            {
                o = (T)serializer.Deserialize(reader);
                return true;
            }
        }
        /// <summary>
        /// Takes a string, compresses it using Gzip and converts the byte array to base64
        /// </summary>
        /// <param name="uncompressedInput"></param>
        /// <returns>the base64 representation of the compressed stream</returns>
        public static string Compress(string uncompressedInput)
        {
            byte[] buffer;
            using (MemoryStream uncompressedStream = new MemoryStream(Encoding.UTF8.GetBytes(uncompressedInput)))
            using (MemoryStream compressedStream = new MemoryStream())
            {
                using (GZipStream compressor = new GZipStream(compressedStream, CompressionMode.Compress))
                {
                    uncompressedStream.CopyTo(compressor);
                }
                buffer = compressedStream.ToArray();
            }
            return Convert.ToBase64String(buffer);
        }
        /// <summary>
        /// Takes a Base64-string, turns it into a byte array and attempts to decompress that into an uncompressed string
        /// </summary>
        /// <param name="compressedInput">a Base64-compliant string containing data that can be decompressed using Gzip</param>
        /// <returns>the uncompressed string</returns>
        public static string Decompress(string compressedInput)
        {
            byte[] buffer;

            using (MemoryStream compressedStream = new MemoryStream(Convert.FromBase64String(compressedInput)))
            using (MemoryStream uncompressedStream = new MemoryStream())
            {
                using (GZipStream compressor = new GZipStream(compressedStream, CompressionMode.Decompress))
                {
                    compressor.CopyTo(uncompressedStream);
                }
                buffer = uncompressedStream.ToArray();
            }
            return Encoding.UTF8.GetString(buffer);
        }
        /// <summary>
        /// Encapsulates Serialization and Compression 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string Pack<T>(T obj)
        {
            return Compress(Serialize(obj));
        }
        /// <summary>
        /// Encapsulates Decompression and Deserialization
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool TryUnpack<T>(string input, out T obj)
        {
            return TryDeserialize(Decompress(input), out obj);
        }
    }
}
