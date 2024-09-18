using System.ComponentModel;

using CompteEstBon;

using Syncfusion.Maui.DataGrid;
using Syncfusion.Maui.ListView;

namespace CebMaui;

public partial class MainPage : ContentPage {
    public MainPage() => InitializeComponent();

    private void SolutionsData_OnSelectionChanged(object? sender, DataGridSelectionChangedEventArgs e) {
        if (sender is SfDataGrid { SelectedRow: CebBase sol })
            ViewTirage.ShowPopup(sol);
    }

    private void GrilleVerticale_OnSelectionChanged(object? sender, SelectionChangedEventArgs e) {
        if (sender is CollectionView { SelectedItem: CebBase sol}) ViewTirage.ShowPopup(sol);
    }

    private void SelectableItemsView_OnSelectionChanged(object? sender, SelectionChangedEventArgs e) {
        if (sender is CollectionView { BindingContext: CebBase sol })
            ViewTirage.ShowPopup(sol);
    }


    private void GrilleOperations_OnItemSelected(object? sender, SelectedItemChangedEventArgs e) {
        if (sender is CollectionView  { BindingContext: CebBase sol })
            ViewTirage.ShowPopup(sol);
    }

    private void DropDownListBase_OnSelectionChanged(object? sender, Syncfusion.Maui.Inputs.SelectionChangedEventArgs e) {
        ViewTirage.Tirage.Clear();
    }


    private void BindableObject_OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        Console.WriteLine("modified");
    }
}