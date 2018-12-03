//-----------------------------------------------------------------------

// <copyright file="DesignerItemsControlItemStyleSelector.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Monday, December 3, 2018 3:34:35 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using ActivityViewModelInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace DiagramDesigner
{
    public class DesignerItemsControlItemStyleSelector : StyleSelector
    {
        static DesignerItemsControlItemStyleSelector()
        {
            Instance = new DesignerItemsControlItemStyleSelector();
        }

        public static DesignerItemsControlItemStyleSelector Instance
        {
            get;
            private set;
        }


        public override Style SelectStyle(object item, DependencyObject container)
        {
            ItemsControl itemsControl = ItemsControl.ItemsControlFromItemContainer(container);
            if (itemsControl == null)
                throw new InvalidOperationException("DesignerItemsControlItemStyleSelector : Could not find ItemsControl");

            if(item is DesignerItemViewModelBase)
            {

                return (Style)itemsControl.FindResource("designerItemStyle");
            }
            
            if (item is ConnectorViewModel)
            {
                return (Style)itemsControl.FindResource("connectorItemStyle");
            }

            return null;
        }
    } 
}
