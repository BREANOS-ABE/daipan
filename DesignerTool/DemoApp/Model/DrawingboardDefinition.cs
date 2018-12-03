//-----------------------------------------------------------------------

// <copyright file="DrawingboardDefinition.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Monday, December 3, 2018 3:34:35 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using ActivityViewModelInterfaces;
using DiagramDesigner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DesignerTool.Model
{
    public class DrawingboardDefinition
    {
        public WorkflowDefinition Workflow { get; set; }
        [Obsolete]
        public int StartActivityId { get; set; }
        public DrawingboardActivityItem[] ActivityItems { get; set; }
        public DrawingboardConnectorItem[] ConnectorItems { get; set; }
        
        public DrawingboardDefinition()
        {

        }
        public DrawingboardDefinition(DiagramViewModel dvm) : this(dvm.Items.OfType<DesignerItemViewModelBase>(), dvm.Items.OfType<ConnectorViewModel>(), dvm.StartActivity)
        {
            Workflow.Settings = dvm.WorkflowSettings.Select(s => new Setting()
            {
                Name = s.Key,
                Value = s.Value
            }).ToArray();
            Workflow.Name = dvm.WorkflowName;
            Workflow.Id = ""+dvm.WorkflowId;
            Workflow.Description = dvm.WorkflowDescription;
        }
        private DrawingboardDefinition(IEnumerable<DesignerItemViewModelBase> items, IEnumerable<ConnectorViewModel> connectors, ActivityItemViewModel startActivity)
        {
            Workflow = new WorkflowDefinition(items, connectors, startActivity);
            var activityItems = items.Select(i => i as ActivityItemViewModel).Where(i => i != null);
            ActivityItems = activityItems.Select(i => new DrawingboardActivityItem()
            {
                TypeName = i.GetType().FullName,
                IconPath=i.IconPath,
                ActivityName=i.ActivityName,
                X=i.Left,
                Y=i.Top,
                ActivityId=WorkflowDefinition.IdMod(i),
                BottomConnectorId=i.BottomConnector?.DataItem?.Id??-1,
                LeftConnectorId=i.LeftConnector?.DataItem?.Id??-1,
                RightConnectorId=i.RightConnector?.DataItem?.Id??-1,
                TopConnectorId=i.TopConnector?.DataItem?.Id??-1
            }).ToArray();
            ConnectorItems = connectors.Select(c => new DrawingboardConnectorItem()
            {
                ConnectorId=c.Id,
                SourceB=c.SourceB,
                Area=c.Area,
                SourceA=c.SourceA,
                EndPoint=c.EndPoint,
                Points=c.ConnectionPoints,
                SourceOrientation=c.SourceConnectorInfo.Orientation,
                TargetOrientation=c.SinkConnectorInfo?.Orientation??ConnectorOrientation.None,
                SourceId=WorkflowDefinition.IdMod(c.SourceConnectorInfo.DataItem),
                TargetId= WorkflowDefinition.IdMod((c.SinkConnectorInfo as FullyCreatedConnectorInfo)?.DataItem),
            }).ToArray();
        }
        public (IEnumerable<DesignerItemViewModelBase> items, IEnumerable<ConnectorViewModel> connectors) ToVM(DiagramViewModel dvm)
        {
            var dataItems = ActivityItems.Select(ai =>
            {
                Enum.TryParse<ActivityGuardType>(Workflow.Activities.First(adef => adef.Id == ai.ActivityId).Guard, out var selectedGuard);
                var avm = new ActivityItemViewModel(ai.ActivityId, dvm, ai.X, ai.Y)
                {
                    ActivityName = ai.ActivityName,
                    IconPath = ai.IconPath,
                    SelectedActivityGuardType = selectedGuard,
                    Settings = (Workflow.Activities.First(a => a.Id == ai.ActivityId).Settings == null) ? (new System.Collections.ObjectModel.ObservableCollection<SettingInfo>()) : (new System.Collections.ObjectModel.ObservableCollection<SettingInfo>(Workflow.Activities.First(a => a.Id == ai.ActivityId).Settings?.Select(s => new SettingInfo()
                    {
                        Key = s.Name,
                        Value = s.Value
                    })))
                };
                return avm;
            }).ToArray();
            
            dvm.WorkflowName = Workflow.Name;
            //dvm.WorkflowId = Workflow.Id;
            int.TryParse(Workflow.Id, out var wfId);
            dvm.WorkflowId = wfId;
            dvm.WorkflowDescription = Workflow.Description;
            
            dvm.WorkflowSettings = new System.Collections.ObjectModel.ObservableCollection<SettingInfo>(Workflow.Settings.Select(s => new SettingInfo()
            {
                Key = s.Name,
                Value = s.Value
            }));
            var connectors = new List<ConnectorViewModel>();
            foreach (var c in ConnectorItems)
            {
                if (c.SourceId == -1) continue; // start transition, specially handled
                if (c.TargetId == -999) continue; //unfinished transition, ignore
                var fcciSrc = new FullyCreatedConnectorInfo(dataItems.Where(di => di.Id == c.SourceId).FirstOrDefault(), c.SourceOrientation);
                var fcciTgt = new FullyCreatedConnectorInfo(dataItems.Where(di => di.Id == c.TargetId).FirstOrDefault(), c.TargetOrientation);
                var cvm = new ConnectorViewModel(c.ConnectorId,fcciSrc.DataItem.Parent,fcciSrc, fcciTgt)
                {
                    SourceA = c.SourceA,
                    SourceB = c.SourceB,
                    EndPoint = c.EndPoint,
                    Area = c.Area,
                    ConnectionPoints = c.Points,
                    ConditionText = Workflow.Transitions.FirstOrDefault(t => t.Id == c.ConnectorId)?.ConditionText
                };
                connectors.Add(cvm);
            }
            var vm = (dataItems, connectors);
            foreach (var di in dataItems)
            {
                foreach (var connector in connectors)
                {
                    var src = connector.SourceConnectorInfo;
                    var tgt = connector.SinkConnectorInfo as FullyCreatedConnectorInfo;
                    
                    if (src.DataItem == di)
                    {
                        var orientation = src.Orientation;
                        switch (orientation)
                        {
                            case ConnectorOrientation.None:
                                throw new Exception("Bad Orientation found on connector source");
                            case ConnectorOrientation.Left:
                                di.LeftConnector = src;
                                break;
                            case ConnectorOrientation.Top:
                                di.TopConnector = src;
                                break;
                            case ConnectorOrientation.Right:
                                di.RightConnector = src;
                                break;
                            case ConnectorOrientation.Bottom:
                                di.BottomConnector = src;
                                break;
                            default:
                                break;
                        }
                    }
                    if (tgt.DataItem.Id == di.Id)
                    {
                        var orientation = tgt.Orientation;
                        switch (orientation)
                        {
                            case ConnectorOrientation.None:
                                throw new Exception("Bad Orientation found on connector target");
                            case ConnectorOrientation.Left:
                                di.LeftConnector = tgt;
                                break;
                            case ConnectorOrientation.Top:
                                di.TopConnector = tgt;
                                break;
                            case ConnectorOrientation.Right:
                                di.RightConnector = tgt;
                                break;
                            case ConnectorOrientation.Bottom:
                                di.BottomConnector = tgt;
                                break;
                            default:
                                break;
                        }
                    }
                }

            }
            return vm;

        }
    }
    public class DrawingboardActivityItem
    {
        public int ActivityId { get; set; }
        public string ActivityName { get; set; }
        public string IconPath { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public string TypeName { get; set; }
        public int TopConnectorId { get; set; }
        public int BottomConnectorId { get; set; }
        public int LeftConnectorId { get; set; }
        public int RightConnectorId { get; set; }
    }
    public class DrawingboardConnectorItem
    {
        public int ConnectorId { get; set; }
        public List<Point> Points { get; set; }
        public Point SourceA { get; set; }
        public Point SourceB { get; set; }
        public Point EndPoint { get; set; }
        public Rect Area { get; set; }
        public int SourceId { get; set; }
        public int TargetId { get; set; }
        public ConnectorOrientation SourceOrientation { get; set; }
        public ConnectorOrientation TargetOrientation { get; set; }

    }
}
