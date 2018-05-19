using System;
using Windows.System;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using CompteEstBon;
using Syncfusion.UI.Xaml.Grid.Converter;
using Syncfusion.XlsIO;
using Windows.Storage;
using Windows.Storage.Pickers;
using System.Collections.Generic;
using Windows.Storage.Provider;

// Pour plus d'informations sur le modèle d'élément Page vierge, consultez la page https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UwpCompteEstBon
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }
        private async void Hasard_Click(object sender, RoutedEventArgs e)
        {
            await Tirage.RandomAsync();
        }

        private async void Resoudre_Click(object sender, RoutedEventArgs e)
        {
            switch (Tirage.Tirage.Status)
            {
                case CebStatus.Valid:
                    await Tirage.ResolveAsync();
                    break;
                case CebStatus.CompteEstBon:
                case CebStatus.CompteApproche:
                    await Tirage.ClearAsync();
                    break;
                case CebStatus.Erreur:
                    await Tirage.RandomAsync();
                    break;
            }
        }
        private async System.Threading.Tasks.Task ExportGridToExcelAsync()
        {
            FileSavePicker savePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };
            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("Excel", new List<string>() { ".xlsx" });
            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = "Ceb";
            StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                // Prevent updates to the remote version of the file until we finish making changes and call CompleteUpdatesAsync.
                CachedFileManager.DeferUpdates(file);

                var options = new ExcelExportingOptions
                {
                    ExcelVersion = ExcelVersion.Excel2013,
                    ExportAllPages = true
                };
                var excelEngine = SolutionsData.ExportToExcel(SolutionsData.View, options);
                var workBook = excelEngine.Excel.Workbooks[0];
                await workBook.SaveAsAsync(file);
                // Let Windows know that we're finished changing the file so the other app can update the remote version of the file.
                // Completing updates may require Windows to ask for user input.
                FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
            }
        }

        private async void ExportExcel_Click(object sender, RoutedEventArgs e)
        {
            await ExportGridToExcelAsync();
        }
    }
}
