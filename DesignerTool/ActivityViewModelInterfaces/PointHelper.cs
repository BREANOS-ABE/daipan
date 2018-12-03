//-----------------------------------------------------------------------

// <copyright file="PointHelper.cs" company="Breanos GmbH">
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
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace ActivityViewModelInterfaces
{
    public class PointHelper
    {
        public static Point GetPointForConnector(FullyCreatedConnectorInfo connector)
        {
            Point point =new Point();

            switch(connector.Orientation)
            {
                case ConnectorOrientation.Top:
                    point = new Point(connector.DataItem.Left + (DesignerItemViewModelBase.ItemWidth / 2), connector.DataItem.Top - (ConnectorInfoBase.ConnectorHeight));
                    break;
                case ConnectorOrientation.Bottom:
                    point = new Point(connector.DataItem.Left + (DesignerItemViewModelBase.ItemWidth / 2), (connector.DataItem.Top + DesignerItemViewModelBase.ItemHeight) + (ConnectorInfoBase.ConnectorHeight / 2));
                    break;
                case ConnectorOrientation.Right:
                    point = new Point(connector.DataItem.Left + DesignerItemViewModelBase.ItemWidth + (ConnectorInfoBase.ConnectorWidth), connector.DataItem.Top + (DesignerItemViewModelBase.ItemHeight / 2));
                    break;
                case ConnectorOrientation.Left:
                    point = new Point(connector.DataItem.Left - ConnectorInfoBase.ConnectorWidth, connector.DataItem.Top + (DesignerItemViewModelBase.ItemHeight / 2));
                    break;
            }

            return point;
        }


    }
}
