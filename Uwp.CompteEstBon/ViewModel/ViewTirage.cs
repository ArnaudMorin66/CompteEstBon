#region

using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Converter;
using Syncfusion.UI.Xaml.Utils;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

#endregion

namespace CompteEstBon {
    public class ViewTirage : INotifyPropertyChanged {
        private Color _background;
        private double _duree;
        private bool _popupIsOpen;

        private Color _foreground = Colors.White;


        private bool _isBusy;

        private bool _isCalculed = false;
        private DateTimeOffset _time;

        private string _result = "Résoudre";

        private Visibility _visibility = Visibility.Collapsed;
        public DispatcherTimer Dispatcher;
        public DispatcherTimer dateDispatcher;

        public DelegateCommand HasardCommand { get; set; }
        public DelegateCommand ResolveCommand { get; set; }
        public DelegateCommand ExportCommand { get; set; }

        public CebTirage Tirage { get; } = new CebTirage();

        public ObservableCollection<int> Plaques { get; } = new ObservableCollection<int> { -1, -1, -1, -1, -1, -1 };

        public IEnumerable<int> ListePlaques { get; } = CebPlaque.ListePlaques;
        private int _nsolutions;
        public int NSolutions {
            get => _nsolutions;
            set {
                _nsolutions = value;
                NotifiedChanged();
            }
        }


        // public ObservableCollection<CebDetail> Solutions { get; } = new ObservableCollection<CebDetail>();
        public string _date;

        public string Date {
            get => _date;
            set {
                _date = value;
                NotifiedChanged();
            }
        }

        public double Duree {
            get => _duree;
            set {
                _duree = value;
                NotifiedChanged();
            }
        }
        private string _symbol;
        public string Symbol {
            get => _symbol;
            set {
                _symbol = value;
                NotifiedChanged();
            }
        }


        public int Search {
            get => Tirage.Search;
            set {
                Tirage.Search = value;
                // ClearData();
                NotifiedChanged();
                ClearData();
            }
        }

        public string Result {
            get => _result;
            set {
                if (value == _result) return;
                _result = value;
                NotifiedChanged();
            }
        }

        public Color Background {
            get => _background;
            set {
                _background = value;
                NotifiedChanged();
            }
        }

        public Color Foreground {
            get => _foreground;
            set {
                _foreground = value;
                NotifiedChanged();
            }
        }

        public bool IsBusy {
            get => _isBusy;
            set {
                _isBusy = value;
                Visibility = _isBusy ? Visibility.Visible : Visibility.Collapsed;
                NotifiedChanged();
            }
        }

        public bool IsCalculed {
            get => _isCalculed;
            set {
                _isCalculed = value;
                NotifiedChanged();
            }
        }

        public Visibility Visibility {
            get => _visibility;
            set {
                _visibility = value;
                NotifiedChanged();
            }
        }

        public bool PopupIsOpen {
            get => _popupIsOpen;
            set {
                _popupIsOpen = value;
                NotifiedChanged();
            }
        }
        public DispatcherTimer NotifyTimer;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Initialisation
        /// </summary>
        /// <returns></returns>
        public ViewTirage() {
            _background = Colors.DarkSlateGray;
            HasardCommand = new DelegateCommand(Hasardcmd);
            ResolveCommand = new DelegateCommand(Resolvecmd);
            ExportCommand = new DelegateCommand(Exportcmd);

            NotifyTimer = new DispatcherTimer {
                Interval = TimeSpan.FromSeconds(5)
            };
            NotifyTimer.Tick += (sender, e) => {
                PopupIsOpen = false;
                NotifyTimer.Stop();
            };
            Dispatcher = new DispatcherTimer {
                Interval = new TimeSpan(100)

            };
            Dispatcher.Tick += (sender, e) => {
                Duree = (DateTimeOffset.Now - _time).TotalSeconds;
            };
            dateDispatcher = new DispatcherTimer {
                Interval = new TimeSpan(1000)
            };
            dateDispatcher.Tick += (sender, e) => {
                Date = Date = $"{DateTime.Now:dddd dd MMMM yyyy à HH:mm:ss}";
            };


            Plaques.CollectionChanged += (sender, e) => {
                if (e.Action != NotifyCollectionChangedAction.Replace) return;
                var i = e.NewStartingIndex;
                if (Tirage.Plaques[i].Value != Plaques[i]) {
                    Tirage.Plaques[i].Value = Plaques[i];
                    ClearData();
                }
            };

            UpdateData();
            UpdateColors();
            Date = $"{DateTime.Now:dddd MM yyyy, HH:mm:ss}";
            dateDispatcher.Start();

        }

        private async void Hasardcmd(object obj) {
            await RandomAsync();

        }
        private async void Resolvecmd(object obj) {
            switch (Tirage.Status) {
                case CebStatus.Valid:
                    await ResolveAsync();
                    break;
                case CebStatus.CompteEstBon:
                case CebStatus.CompteApproche:
                    await ClearAsync();
                    break;
                case CebStatus.Erreur:
                    await RandomAsync();
                    break;
            }
        }
        private async void Exportcmd(object obj) {
            var SolutionsData = (SfDataGrid)obj;
            FileSavePicker savePicker = new FileSavePicker {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };
            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("Excel", new List<string>() { ".xlsx" });
            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = "Ceb";
            StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null) {
                // Prevent updates to the remote version of the file until we finish making changes and call CompleteUpdatesAsync.
                CachedFileManager.DeferUpdates(file);

                var options = new ExcelExportingOptions {
                    ExcelVersion = ExcelVersion.Excel2016,
                    ExportAllPages = true
                };
                var excelEngine = SolutionsData.ExportToExcel(SolutionsData.View, options);
                var workBook = excelEngine.Excel.Workbooks[0];
                var ws = workBook.Worksheets[0];
                ws.ListObjects.Create("Table1", ws[$"A1:E{SolutionsData.View.Records.Count + 1}"])
                    .BuiltInTableStyle = TableBuiltInStyles.TableStyleMedium1;
                ws.InsertRow(1, 3);
                ws.Range["A1"].Value = "Plaques:";
                for (var i = 0; i < 6; i++) {
                    ws.Range[1, i + 2].Value2 = Tirage.Plaques[i].Value;
                }
                ws.Range["A2"].Value2 = "Cherche:";
                ws.Range["B2"].Value2 = Search;
                ws.Range["A3"].Value2 = Result;

                await workBook.SaveAsAsync(file);
                // Let Windows know that we're finished changing the file so the other app can update the remote version of the file.
                // Completing updates may require Windows to ask for user input.
                FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);

                await Launcher.LaunchFileAsync(file);

            }
        }

        private void UpdateColors() {

            (Background, Foreground) = Tirage.Status switch
            {
                CebStatus.Valid => (Colors.DarkSlateGray, Colors.Yellow),
                CebStatus.Erreur => (Colors.Red, Colors.White),
                CebStatus.CompteEstBon => (Colors.DarkGreen, Colors.Yellow),
                CebStatus.CompteApproche => (Colors.Chocolate, Colors.White),
                CebStatus.EnCours => (Colors.Green, Colors.White),
                _ => (Colors.DarkSlateGray, Colors.Yellow)

            };

        }

        private void NotifiedChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void ClearData() {
            PopupIsOpen = false;
            NotifyTimer.Stop();

            Duree = 0;
            NSolutions = 0;
            FirstSolutionString = "";
            var frame = Window.Current.Content as Frame;

            if (frame.Content is MainPage page) {
                page.SolutionsData.ItemsSource = null;
            }


            IsCalculed = false;

            Result = Tirage.Status != CebStatus.Erreur ? "Résoudre" : "Tirage incorrect";
            UpdateColors();
        }
        public string _currentSolution;

        public string CurrentSolution {
            get => _currentSolution;
            set {
                _currentSolution = value;
                NotifiedChanged();
            }
        }
        /// <summary>
        /// 
        public void ShowNotify(int no) {
            if (PopupIsOpen) {
                NotifyTimer.Stop();
                PopupIsOpen = false;
            }
            if (no < 0) no = 0;
            CurrentSolution = Tirage.SolutionIndex(no);
            PopupIsOpen = true; // Visibility.Visible;
            NotifyTimer.Start();
        }
        private void UpdateData() {
            lock (Plaques) {
                for (var i = 0; i < Tirage.Plaques.Count; i++)
                    Plaques[i] = Tirage.Plaques[i];
            }
            ClearData();
            NotifiedChanged("Search");
        }

        #region Action

        public async Task ClearAsync() {
            await Tirage.ClearAsync();
            ClearData();
        }

        public async Task RandomAsync() {
            PopupIsOpen = false;
            await Tirage.RandomAsync();

            FirstSolutionString = "";
            UpdateData();
        }

        public async Task<CebStatus> ResolveAsync() {

            IsBusy = true;

            Result = "...Calcul...";
            // SetBrush((Colors.Green, Colors.White));
            _time = DateTimeOffset.Now;
            Dispatcher.Start();
            await Tirage.ResolveAsync();

            Result = Tirage.Status == CebStatus.CompteEstBon
                ? "Le Compte est bon"
                : (Tirage.Status == CebStatus.CompteApproche ?
                $"Compte approché: {Tirage.Found}, écart: {Tirage.Diff}" : "Tirage incorrect");
            IsCalculed = (Tirage.Status == CebStatus.CompteEstBon || Tirage.Status == CebStatus.CompteApproche);
            FirstSolutionString = Tirage.SolutionIndex(0);

            var frame = Window.Current.Content as Frame;
            var page = frame.Content as MainPage;
            page.SolutionsData.ItemsSource = Tirage.Details;
            NSolutions = Tirage.Solutions.Count;


            Dispatcher.Stop();
            Duree = (DateTimeOffset.Now - _time).TotalSeconds;
            UpdateColors();
            IsBusy = false;
            ShowNotify(0);
            return Tirage.Status;
        }


        #endregion Action
        public string _firstSolutionString;

        public string FirstSolutionString {
            get {
                return _firstSolutionString;
            }
            set {
                _firstSolutionString = value;
                NotifiedChanged();
            }
        }

    }
}