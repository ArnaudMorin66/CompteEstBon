using CompteEstBon.ViewModel;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CompteEstBon {
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow {

        private string _theme = "Dark";

        public MainWindow() {
            InitializeComponent();
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("fr");
            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;

        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown(0);
        private void BtnMenu_Click(object sender, RoutedEventArgs e) => DisplaySystemMenu(sender);

        private void BtnMinimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void BtnMode_Click(object sender, RoutedEventArgs e) => ViewTirage.Vertical = !ViewTirage.Vertical;
        private void BtnSize_Click(object sender, RoutedEventArgs e) => WindowState = (WindowState == WindowState.Normal) ? WindowState.Maximized : WindowState.Normal;
        private void DisplaySystemMenu(object obj) {
            if (obj is not UIElement elt) return;
            var hWnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            var hMenu = NativeMethods.GetSystemMenu(hWnd, false);
            var location = elt.PointToScreen(Mouse.GetPosition(elt));
            var cmd = NativeMethods.TrackPopupMenu(hMenu, 0x100,
                (int)location.X, (int)location.Y, 0, hWnd, IntPtr.Zero);
            if (cmd > 0) NativeMethods.SendMessage(hWnd, 0x112, (IntPtr)cmd, IntPtr.Zero);
        }

        private void ListView_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            if (sender is not UIElement element) return;
            e.Handled = true;
            element.RaiseEvent(new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta) {
                RoutedEvent = MouseWheelEvent
            });
        }
        private void SolutionsData_SelectionChanged(object sender, SelectionChangedEventArgs e) => ViewTirage.ShowNotify(SolutionsData.SelectedIndex);
        private void TbMoins_Click(object sender, RoutedEventArgs e) {
            ViewTirage.Search = Math.Max(ViewTirage.Search - 1, 100);
        }

        private void TbPlus_Click(object sender, RoutedEventArgs e) {
            ViewTirage.Search = Math.Min(ViewTirage.Search + 1, 999);
        }
        private void TextBlock_MouseRightButtonUp(object sender, MouseButtonEventArgs e) => DisplaySystemMenu(sender);

        private void TxtSearch_GotFocus(object sender, RoutedEventArgs e) {
            txtSearch.SelectAll();
        }

        private void TxtSearch_LostFocus(object sender, RoutedEventArgs e) {
            var textbox = sender as TextBox;
            var r = int.TryParse(textbox?.Text, out var v);
            if (r != false && v is >= 100 and <= 999) return;
            txtSearch.SelectAll();
            e.Handled = true;
        }

        private void TxtSearch_OnPreviewTextInput(object sender, TextCompositionEventArgs e) {
            var textbox = sender as TextBox;
            var fullText = textbox?.Text.Insert(textbox.SelectionStart, e.Text);
            if (!int.TryParse(fullText, out _)) {
                e.Handled = true;
            }

        }
        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e) => WindowState = (WindowState == WindowState.Normal) ? WindowState.Maximized : WindowState.Normal;
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

        public static string DotnetVersion => $"{RuntimeInformation.FrameworkDescription}-{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}";
        public string Theme {
            get => _theme;
            set {
                if (!string.Equals(_theme, value, StringComparison.Ordinal)) _theme = value;
            }
        }
    }
}
