//-----------------------------------------------------------------------

// <copyright file="Connection.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Monday, December 3, 2018 3:34:35 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DesignerTool.Persistence.Common
{
    public class Connection : PersistableItemBase
    {
        public Connection(int id, int sourceId, Orientation sourceOrientation, 
            Type sourceType, int sinkId, Orientation sinkOrientation, Type sinkType) : base(id)
        {
            this.SourceId = sourceId;
            this.SourceOrientation = sourceOrientation;
            this.SourceType = sourceType;
            this.SinkId = sinkId;
            this.SinkOrientation = sinkOrientation;
            this.SinkType = sinkType;
        }

        public int SourceId { get; private set; }
        public Orientation SourceOrientation { get; private set; }
        public Type SourceType { get; private set; }
        public int SinkId { get; private set; }
        public Orientation SinkOrientation { get; private set; }
        public Type SinkType { get; private set; }
    }

}
