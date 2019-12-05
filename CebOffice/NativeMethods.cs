using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Word = Microsoft.Office.Interop.Word;
using Excel = Microsoft.Office.Interop.Excel;


namespace CompteEstBon {
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT {
        public int Left;        // position of upper-left corner
        public int Top;         // position of upper-top corner
        public int Right;       // position of lower-right corner
        public int Bottom;      // position of lower-bottom corner
    }

    public static class NativeMethods {
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


        public static bool GetApplication(out Excel.Application excel) {
            try {
                excel = new Excel.Application {
                    Visible = true
                };
                return true;
            } catch (Exception) {
                excel = null;
                return false; 
            }
            
        }
        public static bool GetApplication(out Word.Application word) {
            try {
                word = new Word.Application {
                    Visible = true
                };
                return true;
            } catch(Exception _) {
                word = null;
                return false;
            }
            
        }
    }
}

