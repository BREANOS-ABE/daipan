//-----------------------------------------------------------------------

// <copyright file="ZoomBox.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Monday, December 3, 2018 3:34:35 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Input;
using ActivityViewModelInterfaces;

namespace DiagramDesigner
{
    public class ZoomBox : Control
    {
        private Thumb zoomThumb;
        private Canvas zoomCanvas;
        private Slider zoomSlider;
        private ScaleTransform scaleTransform;

        #region DPs

        #region ScrollViewer
        public ScrollViewer ScrollViewer
        {
            get { return (ScrollViewer)GetValue(ScrollViewerProperty); }
            set { SetValue(ScrollViewerProperty, value); }
        }

        public static readonly DependencyProperty ScrollViewerProperty =
            DependencyProperty.Register("ScrollViewer", typeof(ScrollViewer), typeof(ZoomBox));
        #endregion

        #region DesignerCanvas


        public static readonly DependencyProperty DesignerCanvasProperty =
            DependencyProperty.Register("DesignerCanvas", typeof(DesignerCanvas), typeof(ZoomBox),
                new FrameworkPropertyMetadata(null,
                    new PropertyChangedCallback(OnDesignerCanvasChanged)));


        public DesignerCanvas DesignerCanvas
        {
            get { return (DesignerCanvas)GetValue(DesignerCanvasProperty); }
            set { SetValue(DesignerCanvasProperty, value); }
        }


        private static void OnDesignerCanvasChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ZoomBox target = (ZoomBox)d;
            DesignerCanvas oldDesignerCanvas = (DesignerCanvas)e.OldValue;
            DesignerCanvas newDesignerCanvas = target.DesignerCanvas;
            target.OnDesignerCanvasChanged(oldDesignerCanvas, newDesignerCanvas);
        }


        protected virtual void OnDesignerCanvasChanged(DesignerCanvas oldDesignerCanvas, DesignerCanvas newDesignerCanvas)
        {
            if (oldDesignerCanvas != null)
            {
                newDesignerCanvas.LayoutUpdated -= new EventHandler(this.DesignerCanvas_LayoutUpdated);
                newDesignerCanvas.MouseWheel -= new MouseWheelEventHandler(this.DesignerCanvas_MouseWheel);
            }

            if (newDesignerCanvas != null)
            {
                newDesignerCanvas.LayoutUpdated += new EventHandler(this.DesignerCanvas_LayoutUpdated);
                newDesignerCanvas.MouseWheel += new MouseWheelEventHandler(this.DesignerCanvas_MouseWheel);
                newDesignerCanvas.LayoutTransform = this.scaleTransform;
            }
        }

        #endregion

        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (this.ScrollViewer == null)
                return;

            this.zoomThumb = Template.FindName("PART_ZoomThumb", this) as Thumb;
            if (this.zoomThumb == null)
                throw new Exception("PART_ZoomThumb template is missing!");

            this.zoomCanvas = Template.FindName("PART_ZoomCanvas", this) as Canvas;
            if (this.zoomCanvas == null)
                throw new Exception("PART_ZoomCanvas template is missing!");

            this.zoomSlider = Template.FindName("PART_ZoomSlider", this) as Slider;
            if (this.zoomSlider == null)
                throw new Exception("PART_ZoomSlider template is missing!");

            this.zoomThumb.DragDelta += new DragDeltaEventHandler(this.Thumb_DragDelta);
            this.zoomSlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(this.ZoomSlider_ValueChanged);
            this.scaleTransform = new ScaleTransform();
        }

        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double scale = e.NewValue / e.OldValue;
            double halfViewportHeight = this.ScrollViewer.ViewportHeight / 2;
            double newVerticalOffset = ((this.ScrollViewer.VerticalOffset + halfViewportHeight) * scale - halfViewportHeight);
            double halfViewportWidth = this.ScrollViewer.ViewportWidth / 2;
            double newHorizontalOffset = ((this.ScrollViewer.HorizontalOffset + halfViewportWidth) * scale - halfViewportWidth);
            this.scaleTransform.ScaleX *= scale;
            this.scaleTransform.ScaleY *= scale;
            this.ScrollViewer.ScrollToHorizontalOffset(newHorizontalOffset);
            this.ScrollViewer.ScrollToVerticalOffset(newVerticalOffset);
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            double scale, xOffset, yOffset;
            this.InvalidateScale(out scale, out xOffset, out yOffset);
            this.ScrollViewer.ScrollToHorizontalOffset(this.ScrollViewer.HorizontalOffset + e.HorizontalChange / scale);
            this.ScrollViewer.ScrollToVerticalOffset(this.ScrollViewer.VerticalOffset + e.VerticalChange / scale);
        }

        private void DesignerCanvas_LayoutUpdated(object sender, EventArgs e)
        {
            double scale, xOffset, yOffset;
            this.InvalidateScale(out scale, out xOffset, out yOffset);
            this.zoomThumb.Width = this.ScrollViewer.ViewportWidth * scale;
            this.zoomThumb.Height = this.ScrollViewer.ViewportHeight * scale;
            Canvas.SetLeft(this.zoomThumb, xOffset + this.ScrollViewer.HorizontalOffset * scale);
            Canvas.SetTop(this.zoomThumb, yOffset + this.ScrollViewer.VerticalOffset * scale);
        }

        private void DesignerCanvas_MouseWheel(object sender, EventArgs e)
        {
            MouseWheelEventArgs wheel = (MouseWheelEventArgs)e;
           
            //divide the value by 10 so that it is more smooth
            double value = Math.Max(0, wheel.Delta / 10);
            value = Math.Min(wheel.Delta, 10);
            value = Math.Max(value, -10);
            this.zoomSlider.Value += value;
        }

        private void InvalidateScale(out double scale, out double xOffset, out double yOffset)
        {
            double w = DesignerCanvas.ActualWidth * this.scaleTransform.ScaleX;
            double h = DesignerCanvas.ActualHeight * this.scaleTransform.ScaleY;

            // zoom canvas size
            double x = this.zoomCanvas.ActualWidth;
            double y = this.zoomCanvas.ActualHeight;
            double scaleX = x / w;
            double scaleY = y / h;
            scale = (scaleX < scaleY) ? scaleX : scaleY;
            xOffset = (x - scale * w) / 2;
            yOffset = (y - scale * h) / 2;
        }
    }
}

