//-----------------------------------------------------------------------

// <copyright file="WorkflowDefinition.cs" company="Breanos GmbH">
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
using System.Linq;
using ActivityViewModelInterfaces;

namespace DesignerTool.Model
{
    [XmlRoot(ElementName = "Workflow")]
    public class WorkflowDefinition
    {
        [XmlAttribute(AttributeName ="name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "description")]
        public string Description { get; set; }
        [XmlArrayItem(Type=typeof(Setting), ElementName = "Setting")]
        public Setting[] Settings { get; set; }
        [XmlArray(ElementName = "Activities")]
        [XmlArrayItem(Type = typeof(ActivityDefinition), ElementName = "Activity")]
        public ActivityDefinition[] Activities { get; set; }
        [XmlArray(ElementName = "Transitions")]
        [XmlArrayItem(Type = typeof(TransitionDefinition), ElementName = "Transition")]
        public TransitionDefinition[] Transitions { get; set; }
        public WorkflowDefinition()
        {

        }
        public static int IdMod(DesignerItemViewModelBase item)
        {

            return item?.Id??-999;
        }
        public WorkflowDefinition(IEnumerable<DesignerItemViewModelBase> items, IEnumerable<ConnectorViewModel> connectors, ActivityItemViewModel startActivity)
        {

            var activities = new List<ActivityDefinition>();
            foreach (var i in items)
            {
                var activityDefinition = new ActivityDefinition();
                var activityInfo = i as ActivityItemViewModel;
                activityDefinition.Id = i.Id;
                if (activityInfo != null)
                {
                    activityDefinition.Name = activityInfo.ActivityName;
                    activityDefinition.Guard = activityInfo.SelectedActivityGuardType.ToString();
                    activityDefinition.Settings = activityInfo.Settings.Select(s => new Setting()
                    {
                        Name = s.Key,
                        Value = s.Value
                    }).ToArray();
                }
                else
                {
                    activityDefinition.Name = "UnknownActivity_" + i.Id;
                }
                activities.Add(activityDefinition);
            }
            Activities = activities.ToArray();
            var transitions = connectors.Select(c => new TransitionDefinition()
            {
                Id = c.Id,
                ConditionText = c.ConditionText ?? "true",
                SourceActivityId = IdMod(c.SourceConnectorInfo.DataItem),
                TargetActivityId = IdMod((c.SinkConnectorInfo as FullyCreatedConnectorInfo)?.DataItem)
            }).ToList();
            if (startActivity != null)
            {
                var startTransition = new TransitionDefinition()
                {
                    ConditionText = "true",
                    SourceActivityId = -1,
                    TargetActivityId = startActivity.Id
                };
                long maxId=0;
                if (transitions!=null && transitions.Count > 0)
                {
                    maxId = transitions.Max(t => t.Id);
                }
                startTransition.Id = maxId + 1;
                transitions.Add(startTransition);
            }
            Transitions = transitions.ToArray();
            Settings = new Setting[]
            {
                new Setting()
                {
                    Name="statemachinetype",
                    Value="SomeStateMachineTypeClass"
                }
            };
            Name = "Registered Workflow 66";
        }
        
    }
}
