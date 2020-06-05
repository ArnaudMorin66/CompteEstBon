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
using System.IO;
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
        private char _modeView = '\xE0';
        private bool _vertical;

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

        public IEnumerable<int> ListePlaques { get; } = CebPlaque.AnyPlaques;
        private int _nsolutions;
        public int NSolutions {
            get => _nsolutions;
            set {
                _nsolutions = value;
                NotifiedChanged();
            }
        }
        public bool Vertical {
            get => _vertical;
            set {
                _vertical = value;
                ModeView = value ? '\xE2' : '\xE0';
                NotifiedChanged();
            }
        }
        public char ModeView {
            get => _modeView;
            set {
                _modeView = value;
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
            var cmd = (obj as string)?.ToLower();
            var savePicker = new FileSavePicker {
                SuggestedStartLocation = PickerLocationId.Downloads,
                SuggestedFileName="Ceb",
               
            };
            // Dropdown of file types the user can save the file as
            switch (cmd) {
                case "excel":
                    savePicker.FileTypeChoices.Add("Excel", new List<string>() { ".xlsx" });
                    break;
                case "word":
                    savePicker.FileTypeChoices.Add("Word", new List<string>() { ".docx" });
                    break;
            }
            
          
           
            var file = await savePicker.PickSaveFileAsync();
            
            if (file != null) {
                // await file.DeleteAsync();

                using (var stream = await file.OpenStreamForWriteAsync()) {
                    

                    switch (cmd) {
                        case "excel":
                            await Tirage.ExportExcelAsync(stream);
                            break;
                        case "word":
                            await Tirage.ExportWordAsync(stream);
                            break;
                        default:
                            return;
                    }

                    stream.Close();
                }

                // Let Windows know that we're finished changing the file so the other app can update the remote version of the file.
                // Completing updates may require Windows to ask for user input.
                _ = await CachedFileManager.CompleteUpdatesAsync(file);

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
            CurrentSolution = Tirage.Solution(no);
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
            FirstSolutionString = Tirage.Solution(0);

            var frame = Window.Current.Content as Frame;
            var page = frame.Content as MainPage;
            page.SolutionsData.ItemsSource = Tirage.Solutions;
            NSolutions = Tirage.Solutions.Count;


            Dispatcher.Stop();
            Duree = Tirage.Duree.TotalSeconds;
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