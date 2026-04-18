using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ShopifyEasyShirtPrinting.Behaviours
{
    public static class ImageZoomPanBehavior
    {
        public static bool GetEnableZoomPan(DependencyObject obj) =>
            (bool)obj.GetValue(EnableZoomPanProperty);

        public static void SetEnableZoomPan(DependencyObject obj, bool value) =>
            obj.SetValue(EnableZoomPanProperty, value);

        public static readonly DependencyProperty EnableZoomPanProperty =
            DependencyProperty.RegisterAttached(
                "EnableZoomPan",
                typeof(bool),
                typeof(ImageZoomPanBehavior),
                new PropertyMetadata(false, OnEnableZoomPanChanged));

        private static void OnEnableZoomPanChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                if ((bool)e.NewValue)
                {
                    element.PreviewMouseWheel += Element_PreviewMouseWheel;
                    element.PreviewMouseLeftButtonDown += Element_PreviewMouseLeftButtonDown;
                    element.PreviewMouseLeftButtonUp += Element_PreviewMouseLeftButtonUp;
                    element.PreviewMouseMove += Element_PreviewMouseMove;

                    element.ManipulationDelta += Element_ManipulationDelta;
                    element.IsManipulationEnabled = true; // Enable touch manipulations
                }
                else
                {
                    element.PreviewMouseWheel -= Element_PreviewMouseWheel;
                    element.PreviewMouseLeftButtonDown -= Element_PreviewMouseLeftButtonDown;
                    element.PreviewMouseLeftButtonUp -= Element_PreviewMouseLeftButtonUp;
                    element.PreviewMouseMove -= Element_PreviewMouseMove;

                    element.ManipulationDelta -= Element_ManipulationDelta;
                    element.IsManipulationEnabled = false;
                }
            }
        }

        // Internal state for mouse drag
        private static Point _start;
        private static bool _isDragging = false;

        #region Mouse Handlers

        private static void Element_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                var transform = EnsureTransformGroup(element, out ScaleTransform scale, out TranslateTransform translate);

                double zoom = e.Delta > 0 ? 1.1 : 1 / 1.1;
                scale.ScaleX = Math.Max(0.1, Math.Min(10, scale.ScaleX * zoom));
                scale.ScaleY = Math.Max(0.1, Math.Min(10, scale.ScaleY * zoom));
            }
        }

        private static void Element_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is UIElement element)
            {
                _start = e.GetPosition(element);
                _isDragging = true;
                element.CaptureMouse();
            }
        }

        private static void Element_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is UIElement element)
            {
                _isDragging = false;
                element.ReleaseMouseCapture();
            }
        }

        private static void Element_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging || sender is not FrameworkElement element) return;

            var transform = EnsureTransformGroup(element, out ScaleTransform scale, out TranslateTransform translate);

            Point current = e.GetPosition(element);
            translate.X += current.X - _start.X;
            translate.Y += current.Y - _start.Y;
            _start = current;
        }

        #endregion

        #region Touch Handler

        private static void Element_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                var transform = EnsureTransformGroup(element, out ScaleTransform scale, out TranslateTransform translate);

                // Scale (pinch-to-zoom)
                scale.ScaleX = Math.Max(0.1, Math.Min(10, scale.ScaleX * e.DeltaManipulation.Scale.X));
                scale.ScaleY = Math.Max(0.1, Math.Min(10, scale.ScaleY * e.DeltaManipulation.Scale.Y));

                // Pan (touch move)
                translate.X += e.DeltaManipulation.Translation.X;
                translate.Y += e.DeltaManipulation.Translation.Y;

                e.Handled = true;
            }
        }

        #endregion

        // Helper: ensures Image has a Scale + Translate in a TransformGroup
        private static TransformGroup EnsureTransformGroup(FrameworkElement element, out ScaleTransform scale, out TranslateTransform translate)
        {
            if (element.RenderTransform is TransformGroup tg)
            {
                scale = tg.Children.OfType<ScaleTransform>().FirstOrDefault() ?? new ScaleTransform(1, 1);
                translate = tg.Children.OfType<TranslateTransform>().FirstOrDefault() ?? new TranslateTransform();
                if (!tg.Children.Contains(scale)) tg.Children.Add(scale);
                if (!tg.Children.Contains(translate)) tg.Children.Add(translate);
                return tg;
            }
            else
            {
                scale = new ScaleTransform(1, 1);
                translate = new TranslateTransform();
                tg = new TransformGroup();
                tg.Children.Add(scale);
                tg.Children.Add(translate);
                element.RenderTransform = tg;
                element.RenderTransformOrigin = new Point(0.5, 0.5);
                return tg;
            }
        }
    }
}
