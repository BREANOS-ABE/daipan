//-----------------------------------------------------------------------

// <copyright file="DerivedDataFields.cs" company="Breanos GmbH">
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
  public class BoolDataFieldEntry : BaseDataFieldEntry<Byte>
  {
    public int BitAddress { get; set; }

    public BoolDataFieldEntry() : this(null, 0, 0) { }

    public BoolDataFieldEntry(string name, Int32 address, Int32 bitAddress) : base(name, address, sizeof(byte))
    {
      BitAddress = bitAddress;
    }

    public override void Decode(byte[] data, IMessageConverter converter, Dictionary<string, object> packageInfo)
    {
        if (!packageInfo.ContainsKey(Name))
          packageInfo.Add(Name, converter.GetBool(data, Address, BitAddress));

    }

    public override void Encode(ref byte[] data, object value, IMessageConverter converter)
    {
      converter.SetBool(data, Convert.ToBoolean(value), Address, BitAddress);
    }
  }

  public class ByteDataFieldEntry : BaseDataFieldEntry<byte>
  {
    public ByteDataFieldEntry() : this(null, 0) { }

    public ByteDataFieldEntry(string name, Int32 address) : base(name, address, sizeof(byte)) { }
  }

  public class I16DataFieldEntry : BaseDataFieldEntry<Int16>
  {
    public I16DataFieldEntry() : this(null, 0) { }

    public I16DataFieldEntry(string name, Int32 address) : base(name, address, sizeof(Int16)) { }
  }

  public class I32DataFieldEntry : BaseDataFieldEntry<Int32>
  {
    public I32DataFieldEntry() : this(null, 0) { }

    public I32DataFieldEntry(string name, Int32 address) : base(name, address, sizeof(Int32)) { }
  }

  public class I64DataFieldEntry : BaseDataFieldEntry<Int64>
  {
    public I64DataFieldEntry() : this(null, 0) { }

    public I64DataFieldEntry(string name, Int32 address) : base(name, address, sizeof(Int64)) { }
  }

  public class UI16DataFieldEntry : BaseDataFieldEntry<UInt16>
  {
    public UI16DataFieldEntry() : this(null, 0) { }

    public UI16DataFieldEntry(string name, Int32 address) : base(name, address, sizeof(UInt16)) { }
  }

  public class UI32DataFieldEntry : BaseDataFieldEntry<UInt32>
  {
    public UI32DataFieldEntry() : this(null, 0) { }

    public UI32DataFieldEntry(string name, Int32 address) : base(name, address, sizeof(UInt32)) { }
  }

  public class UI64DataFieldEntry : BaseDataFieldEntry<UInt64>
  {
    public UI64DataFieldEntry() : this(null, 0) { }

    public UI64DataFieldEntry(string name, Int32 address) : base(name, address, sizeof(UInt64)) { }
  }

  public class Ieee754FloatDataFieldEntry : BaseDataFieldEntry<float>
  {
    public Ieee754FloatDataFieldEntry() : this(null, 0) { }

    public Ieee754FloatDataFieldEntry(string name, Int32 address) : base(name, address, sizeof(float)) { }
  }

  public class Ieee754DoubleDataFieldEntry : BaseDataFieldEntry<double>
  {
    public Ieee754DoubleDataFieldEntry() : this(null, 0) { }

    public Ieee754DoubleDataFieldEntry(string name, Int32 address) : base(name, address, sizeof(double)) { }
  }

  public class StringDataFieldEntry : BaseDataFieldEntry<string>
  {
    public StringDataFieldEntry() : this(null, 0, 0) { }

    public StringDataFieldEntry(string name, Int32 address, int length) : base(name, address, length) { }

    public override void Encode(ref byte[] data, object value, IMessageConverter converter)
    {
      if (value.GetType() == typeof(string))
        converter.SetString(data, Convert.ToString(value), Address, Length);
    }

    public override void Decode(byte[] data, IMessageConverter converter, Dictionary<string, object> packageInfo)
    {
      if (!packageInfo.ContainsKey(Name))
        packageInfo.Add(Name, converter.GetString(data, Address, Length));
    }
  }

  public class DateTimeDataFieldEntry : BaseDataFieldEntry<DateTime>
  {
    public DateTimeDataFieldEntry() : this(null, 0, 0) { }

    public DateTimeDataFieldEntry(string name, Int32 address, int length) : base(name, address, length) { }
  }

  public class BcdDataFieldEntry : BaseDataFieldEntry<string>
  {
    public BcdDataFieldEntry() : this(null, 0, 0) { }

    public BcdDataFieldEntry(string name, Int32 address, int length) : base(name, address, length) { }

    public override void Encode(ref byte[] data, object value, IMessageConverter converter)
    {
      if (value.GetType() == typeof(string))
        converter.SetBcd(data, (BcdValue)value, Address, Length);
    }

    public override void Decode(byte[] data, IMessageConverter converter, Dictionary<string, object> packageInfo)
    {
      if (!packageInfo.ContainsKey(Name))
        packageInfo.Add(Name, converter.GetBcd(data, Address, Length));
    }
  }
}
