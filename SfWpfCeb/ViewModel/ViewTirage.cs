#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
#endregion

namespace CompteEstBon {
    public class ViewTirage : INotifyPropertyChanged, ICommand {
        private readonly Stopwatch NotifyWatch = new Stopwatch();
        private readonly TimeSpan SolutionTimer = TimeSpan.FromSeconds(CompteEstBon.Properties.Settings.Default.SolutionTimer);
        private Color _background = Colors.Black;
        private string _duree;

        private Color _foreground = Colors.White;
        private bool _isUpdating;
        private bool _isBusy;
        private bool _isComputed;
        private bool _popup;

        private string _result = "Résoudre";

        public string _solution;
        private string _titre = "Le compte est bon";

        public DispatcherTimer dateDispatcher;
        public Stopwatch stopwatch;

        /// <summary>
        ///     Initialisation
        /// </summary>
        /// <returns>
        /// </returns>
        public ViewTirage() {

            stopwatch = new Stopwatch();

            dateDispatcher = new DispatcherTimer {
                Interval = TimeSpan.FromMilliseconds(Properties.Settings.Default.SolutionTimer)
            };
            dateDispatcher.Tick += (sender, e) => {
                if (stopwatch.IsRunning) Duree = stopwatch.Elapsed.ToString();

                if (Popup && NotifyWatch.Elapsed > SolutionTimer) Popup = false;
                Titre = $"Le compte est bon - {DateTime.Now:dddd dd MMMM yyyy à HH:mm:ss}";
            };

            Plaques.CollectionChanged += (sender, e) => {
                if (e.Action != NotifyCollectionChangedAction.Replace) return;
                var i = e.NewStartingIndex;
                Tirage.Plaques[i].Value = Plaques[i];
                ClearData();
            };
            _isUpdating = false;
            UpdateData();
            UpdateColors();
            Titre = $"Le compte est bon - {DateTime.Now:dddd dd MMMM yyyy à HH:mm:ss}";
            dateDispatcher.Start();
        }

        public static IEnumerable<int> ListePlaques { get; } = CebPlaque.AnyPlaques;

        public CebTirage Tirage { get; } = new CebTirage();

        public ObservableCollection<int> Plaques { get; } =
            new ObservableCollection<int> { 0, 0, 0, 0, 0, 0 };
        public HashSet<CebBase> _solutions = new HashSet<CebBase>();
        public HashSet<CebBase> Solutions {
            get => _solutions;
            set {
                _solutions = value;
                NotifiedChanged();
            }
        }
        public string Duree {
            get => _duree;
            set {
                _duree = value;
                NotifiedChanged();
            }
        }
        private bool _vertical = false;
        public bool Vertical {
            get => _vertical;
            set {
                _vertical = value;
                ModeView =  value ? '\xE2' : '\xE0';
                NotifiedChanged();
            }
        }

        private char _modeView = '\xE0';
        public char ModeView {
            get => _modeView;
            set {
                _modeView = value;
                NotifiedChanged();
            }
        }
        public string Solution {
            get => _solution;
            set {
                _solution = value;
                NotifiedChanged();
            }
        }

        public int Search {
            get => Tirage.Search;
            set {
                Tirage.Search = value;
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
                NotifiedChanged();
            }
        }
      

        public bool IsComputed {
            get => _isComputed;
            set {
                if (value == _isComputed) return;
                _isComputed = value;
                NotifiedChanged();
            }
        }

        private int _count;

        public int Count {
            get => _count;
            set {
                if (_count != value) {
                    _count = value;
                    NotifiedChanged();
                }
            }
        }
        public bool Popup {
            get => _popup;
            set {
                if (_popup == value) return;
                _popup = value;
                NotifyWatch.Stop();
                NotifyWatch.Reset();
                if (_popup) NotifyWatch.Start();
                NotifiedChanged();
            }
        }

        public string Titre {
            get => _titre;
            set {
                _titre = value;
                NotifiedChanged();
            }
        }

        public event EventHandler CanExecuteChanged {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter) => true;

        public async void Execute(object parameter) {
            switch ((parameter as string).ToLower()) {
                case "random":
                    await RandomAsync();
                    break;
                case "resolve":
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
                    break;
                case "excel":
                case "word":
                    await ExportAsync((string)parameter);
                    break;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private async Task ExportAsync(string fmt) {
            IsBusy = true;
            await Task.Run(() => {
                switch (fmt.ToLower()) {
                    case "excel":
                        Tirage.ToExcel();
                        break;
                    case "word":
                        Tirage.ToWord();
                        break;
                }
            });
            IsBusy = false;
        }



        private void UpdateColors() {
            (Background, Foreground) = Tirage.Status switch
            {
                CebStatus.Valid => (Colors.DarkSlateGray, Colors.White),
                CebStatus.Erreur => (Colors.Red, Colors.Navy),
                CebStatus.CompteEstBon => (Colors.ForestGreen, Colors.GhostWhite),
                CebStatus.CompteApproche => (Colors.Salmon, Colors.Black),
                CebStatus.EnCours => (Colors.Gray, Colors.White),
                _ => (Colors.Red, Colors.White)

            };
        }

        private void NotifiedChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ClearData() {
            if (_isUpdating) return;
            stopwatch.Reset();
            Duree = stopwatch.Elapsed.ToString();
            IsComputed = false;
            Solution = "";
            Solutions = null;
            Count = 0;
            Result = Tirage.Status != CebStatus.Erreur ? "" : "Tirage incorrect";
            Popup = false;
            
            UpdateColors();
        }

        private void UpdateData() {
            if (_isUpdating) return;
            _isUpdating = true;

            for (var i = 0; i < Tirage.Plaques.Count; i++)
                Plaques[i] = Tirage.Plaques[i].Value;

            _isUpdating = false;
            ClearData();

            NotifiedChanged("Search");
        }

        public void ShowPopup(int index = 0) {
            if (index >= 0 && index < Tirage.Solutions.Count()) {
                Solution = Tirage.Solution(index);
                Popup = true;
            }
        }

        #region Action

        public async Task ClearAsync() {
            await Tirage.ClearAsync();
            ClearData();
        }

        public async Task RandomAsync() {
            await Tirage.RandomAsync();
            UpdateData();
        }
        // public static MainWindow Window => Application.Current.MainWindow as MainWindow;
        public async Task<CebStatus> ResolveAsync() {
            IsBusy = true;
            Result = "";
            (Background, Foreground) = (Colors.Green, Colors.White);
            stopwatch.Start();
            await Tirage.ResolveAsync();
            Result = Tirage.Status == CebStatus.CompteEstBon
                ? "Le Compte est bon"
                : Tirage.Status == CebStatus.CompteApproche
                    ? $"Compte approché: {Tirage.Found}, écart: {Tirage.Diff}"
                    : "Tirage incorrect";

            stopwatch.Stop();
            Duree = stopwatch.Elapsed.ToString();
            Solution = Tirage.Solution();
            Solutions = new HashSet<CebBase>( Tirage.Solutions);
            Count = Tirage.Count;

            UpdateColors();
            IsBusy = false;
            IsComputed = Tirage.Status == CebStatus.CompteEstBon || Tirage.Status == CebStatus.CompteApproche;
            ShowPopup();
            return Tirage.Status;
        }

        #endregion Action
    }
}