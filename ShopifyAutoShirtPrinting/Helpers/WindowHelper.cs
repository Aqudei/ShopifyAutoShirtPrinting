using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyEasyShirtPrinting.Helpers
{
    public static class WindowHelper
    {
        [DllImport("user32.dll")]
        public static extern bool ShowWindowAsync(HandleRef hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr WindowHandle);
        public const int SW_RESTORE = 9;
        public const int SW_MAXIMIZE = 3;
        public const int SW_SHOW= 5;
        public const int SW_SHOWNORMAL = 1;
        public const int SW_HIDE = 0;

        public static void FocusChrome()
        {
            FocusProcess("chrome");
        }

        public static void FocusSelf(bool maximize = true)
        {
            IntPtr hWnd = IntPtr.Zero;
            hWnd = Process.GetCurrentProcess().MainWindowHandle;
            if (maximize)
                ShowWindowAsync(new HandleRef(null, hWnd), SW_MAXIMIZE);
            else
                ShowWindowAsync(new HandleRef(null, hWnd), SW_SHOW);
            SetForegroundWindow(hWnd);
        }

        private static void FocusProcess(string procName)
        {
            var objProcesses = System.Diagnostics.Process.GetProcessesByName(procName);

            foreach (Process proc in objProcesses)
            {
                if (proc.MainWindowTitle.Contains("ShipStation"))
                {
                    IntPtr hWnd = IntPtr.Zero;
                    hWnd = proc.MainWindowHandle;
                    ShowWindowAsync(new HandleRef(null, hWnd), SW_MAXIMIZE);
                    SetForegroundWindow(hWnd);
                    break;
                }

            }
        }
    }
}
