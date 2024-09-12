using CompteEstBon;

using Syncfusion.Maui.DataGrid;
using Syncfusion.Maui.Themes;

namespace CebMaui {
    public partial class MainPage : ContentPage {

        public MainPage() {
            InitializeComponent();
        }
        private void ChangeSyncfusionControlsTheme(object sender, EventArgs e) {
            ICollection<ResourceDictionary> mergedDictionaries = Application.Current.Resources.MergedDictionaries;
            if (mergedDictionaries != null) {
                var theme = mergedDictionaries.OfType<SyncfusionThemeResourceDictionary>().FirstOrDefault();
                if (theme != null) {
                    if (theme.VisualTheme is SfVisuals.MaterialDark) {
                        theme.VisualTheme = SfVisuals.MaterialLight;
                        Application.Current.UserAppTheme = AppTheme.Light;
                    } else {
                        theme.VisualTheme = SfVisuals.MaterialDark;
                        Application.Current.UserAppTheme = AppTheme.Dark;
                    }
                }
            }
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
