//-----------------------------------------------------------------------

// <copyright file="BaseDataFieldEntry.cs" company="Breanos GmbH">
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

namespace Daipan.Core.Messaging.General
{
  public class BaseDataFieldEntry<T> : DataFieldEntry
  {
    public BaseDataFieldEntry()
    {
      base.DataType = typeof(T).FullName;
    }

    public BaseDataFieldEntry(Int32 length)
      : this()
    {
      base.Length = length;
    }

    public BaseDataFieldEntry(string name, Int32 address, Int32 length) : this(length)
    {
      base.Name = name;
      base.Address = address;
    }


    public override void Encode(ref byte[] data, object value, IMessageConverter converter)
    {
      if (converter.CanConvert(typeof(T)) && value is T)
        converter.SetValue<T>(data, (T)value, Address);

      ///Todo: localized error text - what happend?
      else throw new NotSupportedException();
    }

    public override void Decode(byte[] data, IMessageConverter converter, Dictionary<string, object> packageInfo)
    {
      if (converter.CanConvert(typeof(T)))
      {
        if (!packageInfo.ContainsKey(Name))
          packageInfo.Add(Name, converter.GetValue<T>(data, Address));
      }

      ///Todo: localized error text - what happend?
      else throw new NotSupportedException();
    }
  }

}
