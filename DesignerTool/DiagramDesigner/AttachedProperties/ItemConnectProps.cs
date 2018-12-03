//-----------------------------------------------------------------------

// <copyright file="ItemConnectProps.cs" company="Breanos GmbH">
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
using System.Windows.Input;

namespace DiagramDesigner
{
    public static class ItemConnectProps
    {
        #region EnabledForConnection

        public static readonly DependencyProperty EnabledForConnectionProperty =
            DependencyProperty.RegisterAttached("EnabledForConnection", typeof(bool), typeof(ItemConnectProps),
                new FrameworkPropertyMetadata((bool)false,
                    new PropertyChangedCallback(OnEnabledForConnectionChanged)));

        public static bool GetEnabledForConnection(DependencyObject d)
        {
            return (bool)d.GetValue(EnabledForConnectionProperty);
        }

        public static void SetEnabledForConnection(DependencyObject d, bool value)
        {
            d.SetValue(EnabledForConnectionProperty, value);
        }

        private static void OnEnabledForConnectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement fe = (FrameworkElement)d;


            if ((bool)e.NewValue)
            {
                fe.MouseEnter += Fe_MouseEnter;
            }
            else
            {
                fe.MouseEnter -= Fe_MouseEnter;
            }
        }

        #endregion

        static void Fe_MouseEnter(object sender, MouseEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is DesignerItemViewModelBase)
            {
                DesignerItemViewModelBase designerItem = (DesignerItemViewModelBase)((FrameworkElement)sender).DataContext;
                designerItem.ShowConnectors = true;
            }
        }




    }
}
