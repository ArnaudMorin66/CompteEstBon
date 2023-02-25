//-----------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;

using Syncfusion.SfSkinManager;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;

namespace CompteEstBon;

/// <summary>
/// Logique d'interaction pour MainWindow.xaml
/// </summary>
public partial class MainWindow {
    private string _style = "FluentDark";

    public MainWindow() {
        InitializeComponent();
        System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("fr");
        // Thread.CurrentThread.CurrentUICulture = new CultureInfo("fr");
        Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
        VisualStyle = _style;
    }


    public string VisualStyle {
        get => _style;
        set {
            var resource = Resources.MergedDictionaries.First(v => v.Source.OriginalString.Contains(_style));
            _style = value;

            SfSkinManager.SetVisualStyle(this, (VisualStyles)Enum.Parse(typeof(VisualStyles), value));
            resource.Source =
                    new Uri($"pack://application:,,,/Syncfusion.Themes.{value}.WPF;component/Common/Brushes.xaml");
        }
    }

    private void SolutionsData_SelectionChanged(object sender, GridSelectionChangedEventArgs e) => ViewTirage.ShowPopup(
        SolutionsData.SelectedIndex);

    private void SolutionsData_QueryRowHeight(object sender, QueryRowHeightEventArgs e) {
        if(e.RowIndex <= 0)
            return;
        var gridRowResizingOptions = new GridRowSizingOptions();
        if(!SolutionsData.GridColumnSizer.GetAutoRowHeight(e.RowIndex, gridRowResizingOptions, out var autoHeight))
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
        if(sender is UIElement element) {
            e.Handled = true;

            var e2 = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta) { RoutedEvent = MouseWheelEvent };
            element.RaiseEvent(e2);
        }
    }
}
