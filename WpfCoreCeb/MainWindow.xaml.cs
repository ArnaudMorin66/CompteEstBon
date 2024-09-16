//-----------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CompteEstBon;

/// <summary>
///     Logique d'interaction pour MainWindow.xaml
/// </summary>
public partial class MainWindow {
    private string _theme = "Dark";

    public MainWindow() => InitializeComponent();

    public static string DotnetVersion =>
        $"{RuntimeInformation.FrameworkDescription}-{Assembly.GetExecutingAssembly().GetName().Version}";

    public string Theme {
        get => _theme;
        set {
            if (!string.Equals(_theme, value, StringComparison.Ordinal))
                _theme = value;
        }
    }

    private void BtnMode_Click(object sender, RoutedEventArgs e) => ViewTirage.Vertical = !ViewTirage.Vertical;



    private void ListView_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
        if (sender is not UIElement element)
            return;
        e.Handled = true;
        element.RaiseEvent(
            new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta) { RoutedEvent = MouseWheelEvent });
    }

    private void SolutionsData_SelectionChanged(object sender, SelectionChangedEventArgs e) => ViewTirage.ShowNotify(
        SolutionsData.SelectedIndex);

    private void TbMoins_Click(object sender, RoutedEventArgs e) {
        ViewTirage.Tirage.Search = Math.Max(ViewTirage.Tirage.Search - 1, 100);
    }

    private void TbPlus_Click(object sender, RoutedEventArgs e) {
        ViewTirage.Tirage.Search = Math.Min(ViewTirage.Tirage.Search + 1, 999);
    }

    private void TxtSearch_LostFocus(object sender, RoutedEventArgs e) {
        TxtSearch.SelectAll();
    }

    private void TxtSearch_GotFocus(object sender, RoutedEventArgs e) {
        var textbox = sender as TextBox;
        var r = int.TryParse(textbox?.Text, out var v);
        if (r && v is >= 100 and <= 999)
            return;
        TxtSearch.SelectAll();
        e.Handled = true;
    }


    private void ListBox_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
        if (sender is not UIElement element)
            return;
        e.Handled = true;
        element.RaiseEvent(
            new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left) { RoutedEvent = MouseDownEvent });
    }


    private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
        if (sender is ListView { DataContext: CebBase sol }) {
            ViewTirage.ShowNotify(sol);
            return;
        }
        if (sender is ListView list)
            ViewTirage.ShowNotify(list.SelectedIndex);
    }
    }

