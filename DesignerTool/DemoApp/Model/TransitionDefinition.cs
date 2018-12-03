//-----------------------------------------------------------------------

// <copyright file="TransitionDefinition.cs" company="Breanos GmbH">
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
using System.Text;
using System.Xml.Serialization;

namespace DesignerTool.Model
{
    public class TransitionDefinition
    {
        [XmlAttribute(AttributeName = "id")]
        public long Id { get; set; }
        [XmlAttribute(AttributeName = "from")]
        public long SourceActivityId { get; set; }
        [XmlAttribute(AttributeName = "to")]
        public long TargetActivityId { get; set; }
        [XmlAttribute(AttributeName = "condition")]
        public string ConditionText { get; set; }
        public override string ToString()
        {
            return $"TDef({ConditionText})_{{{SourceActivityId}->{TargetActivityId}}}";
        }
    }
}
