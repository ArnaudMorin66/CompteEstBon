using CompteEstBon.ViewModel;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CompteEstBon {
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow {
        
        public string DotnetVersion => RuntimeInformation.FrameworkDescription;
        public static Dictionary<string, Color> ThemeColors { get; } = new Dictionary<string, Color> {
            ["Dark"] = Color.FromArgb(0xFF, 0x13, 0x18, 0x18),
            ["Blue"] = Color.FromArgb(0xFF, 0x15, 0x25, 0x49),
            ["Black"] = Colors.Black,
            ["DarkBlue"] = Colors.DarkBlue,
            ["DarkSlateGray"] = Colors.DarkSlateGray,
            ["Green"] = Colors.Green
        };
        private string _theme = "Dark";
        public string Theme {
            get => _theme;
            set {
                if (_theme == value) return;
                _theme = value;
            }
        }
        public MainWindow() {
            InitializeComponent();
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("fr");
            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
            
        }
        // private ViewTirage viewTirage => DataContext  as ViewTirage;
        private void SolutionsData_SelectionChanged(object sender, SelectionChangedEventArgs e) => ViewTirage.ShowNotify(SolutionsData.SelectedIndex);

        private void TbPlus_Click(object sender, RoutedEventArgs e) {
            ViewTirage.Search = Math.Min(ViewTirage.Search + 1, 999);
        }
        private void TbMoins_Click(object sender, RoutedEventArgs e) {
            ViewTirage.Search = Math.Max(ViewTirage.Search - 1, 100);
        }
        private void BtnClose_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown(0);
        private void BtnSize_Click(object sender, RoutedEventArgs e) => WindowState = (WindowState == WindowState.Normal) ? WindowState.Maximized : WindowState.Normal;

        private void BtnMinimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();
        private void BtnMenu_Click(object sender, RoutedEventArgs e) => DisplaySystemMenu(sender);
        private void DisplaySystemMenu(object obj) {
            if (obj is UIElement elt) {
                var hWnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
                var hMenu = NativeMethods.GetSystemMenu(hWnd, false);
                var location = elt.PointToScreen(Mouse.GetPosition(elt));
                var cmd = NativeMethods.TrackPopupMenu(hMenu, 0x100,
                    (int) location.X, (int) location.Y, 0, hWnd, IntPtr.Zero);
                if (cmd > 0) NativeMethods.SendMessage(hWnd, 0x112, (IntPtr) cmd, IntPtr.Zero);
            }
        }
        private void TextBlock_MouseRightButtonUp(object sender, MouseButtonEventArgs e) => DisplaySystemMenu(sender);
        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e) => WindowState = (WindowState == WindowState.Normal) ? WindowState.Maximized : WindowState.Normal;
        private void btnMode_Click(object sender, RoutedEventArgs e) => ViewTirage.Vertical = !ViewTirage.Vertical;

        private void txtSearch_GotFocus(object sender, RoutedEventArgs e) {
            txtSearch.SelectAll();
        }

        private void ListView_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            if (sender is UIElement element) {
                e.Handled = true;
                element.RaiseEvent(new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta) {
                    RoutedEvent = MouseWheelEvent
                });
            }
        }

        private void TxtSearch_OnPreviewTextInput(object sender, TextCompositionEventArgs e) {
            var textbox = sender as TextBox;
            var fullText = textbox?.Text.Insert(textbox.SelectionStart, e.Text);
            if (!int.TryParse(fullText, out int value)) {
                e.Handled = true;
                return;
            }


        }

        private void txtSearch_LostFocus(object sender, RoutedEventArgs e) {
            var textbox = sender as TextBox;
            var r = int.TryParse(textbox?.Text, out int v);
            if (r==false || v < 100 || v > 999) {
                txtSearch.SelectAll();
                e.Handled = true;
            }
        }
    }
}
