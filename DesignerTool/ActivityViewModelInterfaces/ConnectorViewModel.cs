//-----------------------------------------------------------------------

// <copyright file="ConnectorViewModel.cs" company="Breanos GmbH">
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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using ActivityViewModelInterfaces;


namespace ActivityViewModelInterfaces
{
    public class ConnectorViewModel : SelectableDesignerItemViewModelBase, ISupportDataChanges
    {
        private FullyCreatedConnectorInfo sourceConnectorInfo;
        private ConnectorInfoBase sinkConnectorInfo;
        private Point sourceB;
        private Point sourceA;
        private List<Point> connectionPoints;
        private Point endPoint;
        private Rect area;

        static int _currentConnectorId = 1;
        public ConnectorViewModel(int id, IDiagramViewModel parent, 
            FullyCreatedConnectorInfo sourceConnectorInfo, FullyCreatedConnectorInfo sinkConnectorInfo) : base(id,parent)
        {
            Init(sourceConnectorInfo, sinkConnectorInfo);
        }
        //public ConnectorViewModel()
        //{
            
        //}
        public static void ResetConnectorId(int start)
        {
            _currentConnectorId = start;
        }
        public ConnectorViewModel(FullyCreatedConnectorInfo sourceConnectorInfo, ConnectorInfoBase sinkConnectorInfo, IDiagramViewModel parent) : base(_currentConnectorId,parent)
        {
            if (sinkConnectorInfo != null && sinkConnectorInfo.Orientation != ConnectorOrientation.None)
                _currentConnectorId++;
            Init(sourceConnectorInfo, sinkConnectorInfo);
        }


        public static IPathFinder PathFinder { get; set; }

        public bool IsFullConnection
        {
            get { return sinkConnectorInfo is FullyCreatedConnectorInfo; }
        }

        public Point SourceA
        {
            get
            {
                return sourceA;
            }
            set
            {
                if (sourceA != value)
                {
                    sourceA = value;
                    UpdateArea();
                    NotifyChanged("SourceA");
                }
            }
        }

        public Point SourceB
        {
            get
            {
                return sourceB;
            }
            set
            {
                if (sourceB != value)
                {
                    sourceB = value;
                    UpdateArea();
                    NotifyChanged("SourceB");
                }
            }
        }

        public List<Point> ConnectionPoints
        {
            get
            {
                return connectionPoints;
            }
            set
            {
                if (connectionPoints != value)
                {
                    connectionPoints = value;
                    NotifyChanged("ConnectionPoints");
                }
            }
        }

        public Point EndPoint
        {
            get
            {
                return endPoint;
            }
            set
            {
                if (endPoint != value)
                {
                    endPoint = value;
                    NotifyChanged("EndPoint");
                }
            }
        }

        public Rect Area
        {
            get
            {
                return area;
            }
            set
            {
                if (area != value)
                {
                    area = value;
                    UpdateConnectionPoints();
                    NotifyChanged("Area");
                }
            }
        }

        public ConnectorInfo ConnectorInfo(ConnectorOrientation orientation, double left, double top, Point position)
        {

            return new ConnectorInfo()
            {
                Orientation = orientation,
                DesignerItemSize = new Size(DesignerItemViewModelBase.ItemWidth, DesignerItemViewModelBase.ItemHeight),
                DesignerItemLeft = left,
                DesignerItemTop = top,
                Position = position

            };
        }

        public FullyCreatedConnectorInfo SourceConnectorInfo
        {
            get
            {
                return sourceConnectorInfo;
            }
            set
            {
                if (sourceConnectorInfo != value)
                {

                    sourceConnectorInfo = value;
                    SourceA = PointHelper.GetPointForConnector(this.SourceConnectorInfo);
                    NotifyChanged("SourceConnectorInfo");
                    (sourceConnectorInfo.DataItem as INotifyPropertyChanged).PropertyChanged += new WeakINPCEventHandler(ConnectorViewModel_PropertyChanged).Handler;
                }
            }
        }

        public ConnectorInfoBase SinkConnectorInfo
        {
            get
            {
                return sinkConnectorInfo;
            }
            set
            {
                
                if (sinkConnectorInfo != value)
                {

                    sinkConnectorInfo = value;
                    if (SinkConnectorInfo is FullyCreatedConnectorInfo)
                    {
                        if ((SinkConnectorInfo as FullyCreatedConnectorInfo).DataItem == null) return;
                        SourceB = PointHelper.GetPointForConnector((FullyCreatedConnectorInfo)SinkConnectorInfo);
                        (((FullyCreatedConnectorInfo)sinkConnectorInfo).DataItem as INotifyPropertyChanged).PropertyChanged += new WeakINPCEventHandler(ConnectorViewModel_PropertyChanged).Handler;
                    }
                    else
                    {

                        SourceB = ((PartCreatedConnectionInfo)SinkConnectorInfo).CurrentLocation;
                    }
                    NotifyChanged("SinkConnectorInfo");
                }
            }
        }

        public ICommand ShowDataChangeWindowCommand { get; private set; }
        private string _conditionText;

        public string ConditionText
        {
            get { return _conditionText; }
            set { _conditionText = value; NotifyChanged(nameof(ConditionText)); }
        }

        public void ExecuteShowDataChangeWindowCommand(object parameter)
        {
            ConnectorItemData data = new ConnectorItemData(ConditionText);
            if (visualiserService.ShowDialog(data) == true)
            {
                this.ConditionText= data.ConditionText;
            }
        }
        private void UpdateArea()
        {
            Area = new Rect(SourceA, SourceB); 
        }

        private void UpdateConnectionPoints()
        {
            ConnectionPoints = new List<Point>()
                                   {
                                       
                                       new Point( SourceA.X  <  SourceB.X ? 0d : Area.Width, SourceA.Y  <  SourceB.Y ? 0d : Area.Height ), 
                                       new Point(SourceA.X  >  SourceB.X ? 0d : Area.Width, SourceA.Y  >  SourceB.Y ? 0d : Area.Height)
                                   };

            ConnectorInfo sourceInfo = ConnectorInfo(SourceConnectorInfo.Orientation,
                                            ConnectionPoints[0].X,
                                            ConnectionPoints[0].Y,
                                            ConnectionPoints[0]);

            if(IsFullConnection)
            {
                EndPoint = ConnectionPoints.Last();
                ConnectorInfo sinkInfo = ConnectorInfo(SinkConnectorInfo.Orientation,
                                  ConnectionPoints[1].X,
                                  ConnectionPoints[1].Y,
                                  ConnectionPoints[1]);

                ConnectionPoints = PathFinder.GetConnectionLine(sourceInfo, sinkInfo, true);
            }
            else
            {
                ConnectionPoints = PathFinder.GetConnectionLine(sourceInfo, ConnectionPoints[1], ConnectorOrientation.Left);
                EndPoint = new Point();
            }
        }

        private void ConnectorViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Left":
                case "Top":
                    SourceA = PointHelper.GetPointForConnector(this.SourceConnectorInfo);
                    if (this.SinkConnectorInfo is FullyCreatedConnectorInfo)
                    {
                        SourceB = PointHelper.GetPointForConnector((FullyCreatedConnectorInfo)this.SinkConnectorInfo);
                    }
                    break;

            }
        }

        private void Init(FullyCreatedConnectorInfo sourceConnectorInfo, ConnectorInfoBase sinkConnectorInfo)
        {
            ShowDataChangeWindowCommand = new SimpleCommand(ExecuteShowDataChangeWindowCommand);
            visualiserService = ApplicationServicesProvider.Instance.Provider.VisualizerService;
            this.Parent = sourceConnectorInfo.DataItem.Parent;
            this.SourceConnectorInfo = sourceConnectorInfo;
            this.SinkConnectorInfo = sinkConnectorInfo;
            PathFinder = new OrthogonalPathFinder();
        }
        private IUIVisualizerService visualiserService;
    }

    public class ConnectorItemData
    {
        public string ConditionText { get; set; }
        public ConnectorItemData(string condition)
        {
            ConditionText = condition;
        }
    }
}
