using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ExportCeb {
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT {
        public int Left;        // position of upper-left corner
        public int Top;         // position of upper-top corner
        public int Right;       // position of lower-right corner
        public int Bottom;      // position of lower-bottom corner
    }

    static class InteropClass {
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);
        [DllImport("user32.dll")]
        public static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        [DllImport("user32.dll")]
        public static extern int TrackPopupMenu(IntPtr hMenu, uint uFlags, int x, int y,
           int nReserved, IntPtr hWnd, IntPtr prcRect);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetFocus(IntPtr hWnd);
        [DllImport("user32.dll")]
        internal static extern IntPtr SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr GetWindowThreadProcessId(int hWnd, out IntPtr lpdwProcessId);
        [DllImport("user32.dll")]
        internal static extern IntPtr SetActiveWindow(IntPtr hWnd);
        internal static void SetFocusWindow(int hwnd) {
            GetWindowThreadProcessId(hwnd, out IntPtr ProcIdXL);
            SetForegroundWindow(Process.GetProcessById(ProcIdXL.ToInt32()).MainWindowHandle);
        }

        internal static bool GetApplication(out Microsoft.Office.Interop.Excel.Application excel) {

            bool isNew = false;
            try {
                excel = (Microsoft.Office.Interop.Excel.Application)Marshal.GetActiveObject("Excel.Application");
            }
            catch (Exception) {
                excel = new Microsoft.Office.Interop.Excel.Application();
                isNew = true;
            }
            excel.Visible = true;
            return isNew;
        }
        internal static bool GetApplication(out Microsoft.Office.Interop.Word.Application word) {

            bool isNew = false;

            try {
                word = (Microsoft.Office.Interop.Word.Application)Marshal.GetActiveObject("Excel.Application");
            }
            catch (Exception) {
                word = new Microsoft.Office.Interop.Word.Application();
                isNew = true;
            }
            word.Visible = true;

            return isNew;
        }
    }
}
