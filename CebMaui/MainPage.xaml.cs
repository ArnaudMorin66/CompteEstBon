using CompteEstBon;

using Syncfusion.Maui.DataGrid;

namespace CebMaui {
    public partial class MainPage : ContentPage {

        public MainPage() {
            InitializeComponent();
        }

        private void SolutionsData_OnSelectionChanged(object? sender, DataGridSelectionChangedEventArgs e) {
            if( SolutionsData.SelectedRow is CebBase sol) Tirage.ShowPopup(sol);
        }

        private void GrilleVerticale_OnSelectionChanged(object? sender, SelectionChangedEventArgs e) {
            if (GrilleVerticale.SelectedItem is CebBase sol) Tirage.ShowPopup(sol);
        }

        private void SelectableItemsView_OnSelectionChanged(object? sender, SelectionChangedEventArgs e) {
            if (sender is CollectionView { BindingContext: CebBase sol })
                Tirage.ShowPopup(sol);
        }
    }

}
