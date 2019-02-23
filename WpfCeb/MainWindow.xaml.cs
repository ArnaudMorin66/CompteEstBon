using Syncfusion.SfSkinManager;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.Windows.Shared;
using Syncfusion.Windows.Tools.Controls;
using System;
using System.Windows;

namespace WpfCeb
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow:  ChromelessWindow {
        public MainWindow() {
            InitializeComponent();
            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
            SfSkinManager.SetVisualStyle(this, VisualStyles.Blend);
         }

        private void SolutionsData_SelectionChanged(object sender, Syncfusion.UI.Xaml.Grid.GridSelectionChangedEventArgs e)
        {
            tirage.NotifyCommand.Execute(SolutionsData);
        }
    }
}
