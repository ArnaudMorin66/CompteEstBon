//-----------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Globalization;
using System.Threading;
using System.Windows;

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
        Thread.CurrentThread.CurrentUICulture = new CultureInfo("fr");
        Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
    }

    public string VisualStyle {
        get => _style;
        set {
            if (_style == value) {
                return;
            }

            _style = value;
            SfSkinManagerExtension.SetTheme(this, new Theme(value));
        }
    }

    private void SolutionsData_SelectionChanged(object sender, GridSelectionChangedEventArgs e) => ViewTirage.ShowPopup(
        (sender as SfDataGrid)!.SelectedIndex);

    private void SolutionsData_QueryRowHeight(object sender, QueryRowHeightEventArgs e) {
        if (e.RowIndex <= 0)
            return;

        var gridRowResizingOptions = new GridRowSizingOptions();

        if ((sender as SfDataGrid)!.GridColumnSizer
            .GetAutoRowHeight(e.RowIndex, gridRowResizingOptions, out var autoHeight)) {
            e.Height = autoHeight;
            e.Handled = true;
        }
    }

    private void BtnMode_Click(object sender, RoutedEventArgs e) {
        ViewTirage.Vertical = !ViewTirage.Vertical;
        if (SolutionsData.GetRecordsCount() <= 0)
            return;

        for (var index = 0; index <= SolutionsData.GetRecordsCount(); index++) {
            SolutionsData.InvalidateRowHeight(index);
        }
    }
}