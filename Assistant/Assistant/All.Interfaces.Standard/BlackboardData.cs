//-----------------------------------------------------------------------

// <copyright file="BlackboardData.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Monday, December 3, 2018 3:34:35 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BIKSClassLibrary
{
    /// <summary>
    /// Data object sent and received from Blackboard
    /// </summary>
    [Serializable]
    [DataContract]
    [JsonObject(MemberSerialization.OptOut)]    
    public class BlackboardData
    {       
        [DataMember]
        byte[] IntBuffer { get; set; } = new byte[0];

        [DataMember]
        object buffer;

        public BlackboardData(object obj)
        {
            buffer = obj;
        }
        
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="size"></param>
        /// <param name="buffer"></param>
        public BlackboardData(byte[] buffer)
        {
            if (buffer != null)
            {
                int size = buffer.Length;
                IntBuffer = new byte[size];
                buffer.CopyTo(IntBuffer, 0);
            }
        }

        /// <summary>
        /// overload ToString method
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string part1 = IntBuffer.ToString();
            string part2 = string.Empty;

            if (part2.Length != 0)
                return part1 + " " + part2;

            return part1;
        }
    }
}
