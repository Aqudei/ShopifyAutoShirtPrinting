using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace ShopifyEasyShirtPrinting.Views
{
    /// <summary>
    /// Interaction logic for OrderProcessingView.xaml
    /// </summary>
    public partial class Archived : UserControl
    {
        const int WM_MOUSEHWHEEL = 0x020E;

        public Archived()
        {
            InitializeComponent();


            Loaded += OrderProcessingView_Loaded;
        }

        private void OrderProcessingView_Loaded(object sender, RoutedEventArgs e)
        {
            var source = PresentationSource.FromVisual(OrdersDataGrid);
            ((HwndSource)source)?.AddHook(Hook);
        }

        /// <summary>
        /// Gets high bits values of the pointer.
        /// </summary>
        private static int HIWORD(IntPtr ptr)
        {
            unchecked
            {
                if (Environment.Is64BitOperatingSystem)
                {
                    var val64 = ptr.ToInt64();
                    return (short)((val64 >> 16) & 0xFFFF);
                }
                var val32 = ptr.ToInt32();
                return (short)((val32 >> 16) & 0xFFFF);
            }
        }

        /// <summary>
        /// Gets low bits values of the pointer.
        /// </summary>
        private static int LOWORD(IntPtr ptr)
        {
            unchecked
            {
                if (Environment.Is64BitOperatingSystem)
                {
                    var val64 = ptr.ToInt64();
                    return (short)(val64 & 0xFFFF);
                }

                var val32 = ptr.ToInt32();
                return (short)(val32 & 0xFFFF);
            }
        }

        private IntPtr Hook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_MOUSEHWHEEL:
                    int tilt = (short)HIWORD(wParam);
                    OnMouseTilt(tilt);
                    return (IntPtr)1;
            }
            // Handle window message here.
            return IntPtr.Zero;
        }


        private void OnMouseTilt(int tilt)
        {
            // Write your horizontal handling codes here.
            var element = OrdersDataGrid;

            if (element == null) return;

            var scrollViewer = FindVisualChild<ScrollViewer>(element);

            if (scrollViewer == null)
                return;

            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + tilt);
        }

        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            int numChildren = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numChildren; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                {
                    return typedChild;
                }

                var result = FindVisualChild<T>(child);
                if (result != null)
                {
                    return result;
                }
            }


            return null;
        }
    }
}
