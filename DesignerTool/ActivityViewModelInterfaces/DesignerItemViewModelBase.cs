//-----------------------------------------------------------------------

// <copyright file="DesignerItemViewModelBase.cs" company="Breanos GmbH">
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
using System.Windows.Input;

namespace ActivityViewModelInterfaces
{

    public abstract class DesignerItemViewModelBase : SelectableDesignerItemViewModelBase
    {
        private double left;
        private double top;
        private bool showConnectors = false;
        private List<FullyCreatedConnectorInfo> connectors = new List<FullyCreatedConnectorInfo>(4);

        private static double itemWidth = 96;
        private static double itemHeight = 96;
        private static int _currentId = 0;
        public static void ResetCurrentId(int newCurrent)
        {
            _currentId = newCurrent;
        }
        public DesignerItemViewModelBase(int id, IDiagramViewModel parent, double left, double top) : base(id, parent)
        {
            this.left = left;
            this.top = top;
            Init();
        }

        public DesignerItemViewModelBase(IDiagramViewModel parent, string imagePath = null): base(_currentId++,parent)
        {
            ImagePath = imagePath;
            Init();
        }
        private string _imagePath;

        public string ImagePath
        {
            get
            {
                return _imagePath;
            }
            set { _imagePath = value; }
        }


        public FullyCreatedConnectorInfo TopConnector
        {
            get { return connectors[0]; }
            set { connectors[0] = value; }
        }


        public FullyCreatedConnectorInfo BottomConnector
        {
            get { return connectors[1]; }
            set { connectors[1] = value; }
        }


        public FullyCreatedConnectorInfo LeftConnector
        {
            get { return connectors[2]; }
            set { connectors[2] = value; }
        }


        public FullyCreatedConnectorInfo RightConnector
        {
            get { return connectors[3]; }
            set { connectors[3] = value; }
        }



        public static double ItemWidth
        {
            get { return itemWidth; }
        }

        public static double ItemHeight
        {
            get { return itemHeight; }
        }

        public bool ShowConnectors
        {
            get
            {
                return showConnectors;
            }
            set
            {
                if (showConnectors != value)
                {
                    showConnectors = value;
                    TopConnector.ShowConnectors = value;
                    BottomConnector.ShowConnectors = value;
                    RightConnector.ShowConnectors = value;
                    LeftConnector.ShowConnectors = value;
                    NotifyChanged("ShowConnectors");
                }
            }
        }


        public double Left
        {
            get
            {
                return left;
            }
            set
            {
                if (left != value)
                {
                    left = value;
                    NotifyChanged("Left");
                }
            }
        }

        public double Top
        {
            get
            {
                return top;
            }
            set
            {
                if (top != value)
                {
                    top = value;
                    NotifyChanged("Top");
                }
            }
        }


        private void Init()
        {
            connectors.Add(new FullyCreatedConnectorInfo(this, ConnectorOrientation.Top));
            connectors.Add(new FullyCreatedConnectorInfo(this, ConnectorOrientation.Bottom));
            connectors.Add(new FullyCreatedConnectorInfo(this, ConnectorOrientation.Left));
            connectors.Add(new FullyCreatedConnectorInfo(this, ConnectorOrientation.Right));
        }
        
    }
}
