using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PrintAgent.Helpers
{
    public static class WindowHelper
    {
        [DllImport("user32.dll")]
        public static extern bool ShowWindowAsync(HandleRef hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr WindowHandle);
        public const int SW_RESTORE = 9;
        public const int SW_MAXIMIZE = 3;


        public static void FocusChrome()
        {
            FocusProcess("chrome", "ShipStation");
        }

        public static void FocusSelf()
        {
            IntPtr hWnd = IntPtr.Zero;
            hWnd = Process.GetCurrentProcess().MainWindowHandle;
            ShowWindowAsync(new HandleRef(null, hWnd), SW_MAXIMIZE);
            SetForegroundWindow(hWnd);
        }

        private static void FocusProcess(string procName, string windowName)
        {
            var objProcesses = System.Diagnostics.Process.GetProcessesByName(procName);

            foreach (Process proc in objProcesses)
            {
                if (proc.MainWindowTitle.Contains(windowName))
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
