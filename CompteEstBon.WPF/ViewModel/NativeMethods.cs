using System;
using System.Diagnostics;
using System.Runtime.InteropServices;


namespace CompteEstBon.ViewModel {
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT {
        public int Left;        // position of upper-left corner
        public int Top;         // position of upper-top corner
        public int Right;       // position of lower-right corner
        public int Bottom;      // position of lower-bottom corner
    }

    static class NativeMethods {
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
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        internal static void SetFocusWindow(int hwnd) {
            GetWindowThreadProcessId(hwnd, out IntPtr ProcIdXL);
            ShowWindow(ProcIdXL, 5);
            SetForegroundWindow(Process.GetProcessById(ProcIdXL.ToInt32()).MainWindowHandle);
        }


        public static bool GetApplication(out Microsoft.Office.Interop.Excel.Application excel) {
            var isNew = false;
#if false
            try {
                excel = (Microsoft.Office.Interop.Excel.Application)Marshal.GetActiveObject("Excel.Application");
            } catch (Exception) {
                excel = new Microsoft.Office.Interop.Excel.Application();
                isNew = true;
            }
#else
            excel = new Microsoft.Office.Interop.Excel.Application();
            isNew = true;
#endif
            return isNew;
        }
        public static bool GetApplication(out Microsoft.Office.Interop.Word.Application word) {
            bool isNew = false;
#if false
            try {
                word = (Microsoft.Office.Interop.Word.Application)Marshal.GetActiveObject("Word.Application");
            } catch (Exception) {
                word = new Microsoft.Office.Interop.Word.Application();
                isNew = true;
            }
#else
            word = new Microsoft.Office.Interop.Word.Application();
            isNew = true;
#endif
            word.Visible = true;
            return isNew;
        }
    }
}

