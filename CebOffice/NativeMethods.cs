using System;
using System.Runtime.InteropServices;
// using Word = Microsoft.Office.Interop.Word;
// using Excel = Microsoft.Office.Interop.Excel;


namespace CompteEstBon.ViewModel {
      public static class NativeMethods {
        [DllImport("user32.dll")]
        public static extern nint SendMessage(nint hWnd, int msg, nint wp, nint lp);
        [DllImport("user32.dll")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1401:P/Invokes should not be visible", Justification = "<En attente>")]
        public static extern nint GetSystemMenu(nint hWnd, bool bRevert);
        [DllImport("user32.dll")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1401:P/Invokes should not be visible", Justification = "<En attente>")]
        public static extern int TrackPopupMenu(nint hMenu, uint uFlags, int x, int y,
           int nReserved, nint hWnd, nint prcRect);

       

    }
}

