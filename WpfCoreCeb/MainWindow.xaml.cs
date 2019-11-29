using CompteEstBon.ViewModel;
using System;
using System.Windows;
using System.Windows.Input;

namespace CompteEstBon {
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow {
        public MainWindow() {
            InitializeComponent();
            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

        private void SolutionsData_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            vwTirage.ShowNotify(SolutionsData.SelectedIndex);
        }

        private void TbPlus_Click(object sender, RoutedEventArgs e) {
            if (vwTirage.Search < 999) {
                vwTirage.Search++;
            }
        }

        private void TbMoins_Click(object sender, RoutedEventArgs e) {
            if (vwTirage.Search > 100) {
                vwTirage.Search--;
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) {
            Application.Current.Shutdown(0);
        }

        private void BtnSize_Click(object sender, RoutedEventArgs e) {
            if (WindowState == WindowState.Normal) {
                WindowState = WindowState.Maximized;
            } else {
                WindowState = WindowState.Normal;
            }
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e) {
            WindowState = WindowState.Minimized;
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            DragMove();
        }

        private void BtnMenu_Click(object sender, RoutedEventArgs e) {
            DisplaySystemMenu(sender);
        }
        private void DisplaySystemMenu(object obj) {
            var elt = obj as UIElement;
            IntPtr hWnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            IntPtr hMenu = NativeMethods.GetSystemMenu(hWnd, false);

            Point location = elt.PointToScreen(Mouse.GetPosition(elt)); // (new Point(0, 0));
            int cmd = NativeMethods.TrackPopupMenu(hMenu, 0x100, (int)location.X, (int)location.Y, 0, hWnd, IntPtr.Zero);
            if (cmd > 0) NativeMethods.SendMessage(hWnd, 0x112, (IntPtr)cmd, IntPtr.Zero);
        }

        private void TextBlock_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            DisplaySystemMenu(sender);
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if (WindowState == WindowState.Normal)
                WindowState = WindowState.Maximized;
            else
                WindowState = WindowState.Normal;
        }


    }
}
