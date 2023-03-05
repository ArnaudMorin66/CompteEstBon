//-----------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Windows;
using System.Windows.Input;

using Syncfusion.SfSkinManager;

using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.Windows.Shared;

namespace CompteEstBon;

/// <summary>
/// Logique d'interaction pour MainWindow.xaml
/// </summary>
public partial class MainWindow {
    private string _style = "FluentDark";

    public MainWindow() {
        InitializeComponent();
        System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("fr");
        Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
    }


    public string VisualStyle {
        get => _style;
        set {
            if(_style == value)
                return;
            _style = value;
            SfSkinManagerExtension.SetTheme(this, new Theme(value));
        }
    }

    private void SolutionsData_SelectionChanged(object sender, GridSelectionChangedEventArgs e) => ViewTirage.ShowPopup(
        (sender as SfDataGrid)!.SelectedIndex);

    private void SolutionsData_QueryRowHeight(object sender, QueryRowHeightEventArgs e) {
        if(e.RowIndex <= 0)
            return;
        var gridRowResizingOptions = new GridRowSizingOptions();

        if(!(sender as SfDataGrid)!.GridColumnSizer
            .GetAutoRowHeight(e.RowIndex, gridRowResizingOptions, out var autoHeight))
            return;
        e.Height = autoHeight;
        e.Handled = true;
    }


    private void BtnMode_Click(object sender, RoutedEventArgs e) {
        ViewTirage.Vertical = !ViewTirage.Vertical;
        if(SolutionsData.GetRecordsCount() <= 0)
            return;
        for(var index = 0; index <= SolutionsData.GetRecordsCount(); index++)
            SolutionsData.InvalidateRowHeight(index);
    }

    private void ListBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
        if (sender is not UIElement element) return;
        e.Handled = true;

        var e2 = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta) { RoutedEvent = MouseWheelEvent };
        element.RaiseEvent(e2);
    }
}
