//-----------------------------------------------------------------------

// <copyright file="DataPackageConfiguration.cs" company="Breanos GmbH">
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
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Daipan.Core.Messaging.General
{
  public class DataPackageConfiguration
  {
    public List<DataFieldEntry> Header { get; protected set; }

    public List<DataPackage> KnownPackages { get; protected set; }

    public DataPackageConfiguration()
    {
      Header = new List<DataFieldEntry>();
      KnownPackages = new List<DataPackage>();
    }

    public int GetLength(PackagePart packagePart, DataPackage package = null)
    {
      int length = 0;

      if ((packagePart & PackagePart.Header) > 0)
        length += Header.Sum(x => x.Length);

      if (package != null)
        length += package.GetLength(packagePart);

      return length;
    }

    public void SaveToXml(string path) { SaveToXml(path, this); }


    public static DataPackageConfiguration LoadFromXml(string path)
    {
      DataPackageConfiguration configuration = null;

      XmlSerializer serializer = new XmlSerializer(typeof(DataPackageConfiguration), new Type[] {
              typeof(BoolDataFieldEntry),
              typeof(ByteDataFieldEntry),
              typeof(I16DataFieldEntry),
              typeof(I32DataFieldEntry),
              typeof(I64DataFieldEntry),
              typeof(UI16DataFieldEntry),
              typeof(UI32DataFieldEntry),
              typeof(UI64DataFieldEntry),
              typeof(Ieee754FloatDataFieldEntry),
              typeof(Ieee754DoubleDataFieldEntry),
              typeof(DateTimeDataFieldEntry),
              typeof(StringDataFieldEntry),
              typeof(BcdDataFieldEntry) });

      using (StreamReader sr = new StreamReader(path))
      {
        configuration = serializer.Deserialize(sr) as DataPackageConfiguration;
        sr.Close();
      }

      return configuration;
    }

    public static void SaveToXml(string path, DataPackageConfiguration configuration)
    {
      XmlSerializer serializer = new XmlSerializer(typeof(DataPackageConfiguration), new Type[] {
              typeof(BoolDataFieldEntry),
              typeof(ByteDataFieldEntry),
              typeof(I16DataFieldEntry),
              typeof(I32DataFieldEntry),
              typeof(I64DataFieldEntry),
              typeof(UI16DataFieldEntry),
              typeof(UI32DataFieldEntry),
              typeof(UI64DataFieldEntry),
              typeof(Ieee754FloatDataFieldEntry),
              typeof(Ieee754DoubleDataFieldEntry),
              typeof(DateTimeDataFieldEntry),
              typeof(StringDataFieldEntry),
              typeof(BcdDataFieldEntry) });

      using (StreamWriter sw = new StreamWriter(path))
      {
        serializer.Serialize(sw, configuration);
        sw.Flush();
        sw.Close();
      }
    }
  }
}
