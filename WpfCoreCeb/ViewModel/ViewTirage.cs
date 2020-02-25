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

namespace CompteEstBon.ViewModel {
    public class ViewTirage : INotifyPropertyChanged, ICommand {
        private readonly Stopwatch NotifyWatch = new Stopwatch();
        private readonly TimeSpan SolutionTimer = TimeSpan.FromSeconds(10);

        private readonly Storyboard WaitStory =
            Application.Current.MainWindow?.FindResource("WaitStoryboard") as Storyboard;

        private Color _background = Colors.Navy;
        private char _modeView = '\xE0';
        private Visibility _vertical = Visibility.Hidden;
        private TimeSpan _duree;
        private Color _foreground = Colors.White;
        private bool _isBusy;
        private bool _isUpdating;
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
            Solutions = new ObservableCollection<CebBase>();
            stopwatch = new Stopwatch();

            dateDispatcher = new DispatcherTimer {
                Interval = TimeSpan.FromMilliseconds(10)
            };
            dateDispatcher.Tick += (sender, e) => {
                if (stopwatch.IsRunning) Duree = stopwatch.Elapsed;
                if (Popup && NotifyWatch.Elapsed > SolutionTimer)
                    Popup = false;
                Titre = $"Le compte est bon - {DateTime.Now:dddd dd MMMM yyyy à HH:mm:ss}";
            };
            Plaques.CollectionChanged += (sender, e) => {
                if (e.Action != NotifyCollectionChangedAction.Replace) return;
                var i = e.NewStartingIndex;
                Tirage.Plaques[i].Text = Plaques[i];
                ClearData();
            };
            _isUpdating = false;
            UpdateData();
            UpdateColors();
            Titre = $"Le compte est bon - {DateTime.Now:dddd dd MMMM yyyy à HH:mm:ss}";
            dateDispatcher.Start();

        }


        public CebTirage Tirage { get; } = new CebTirage();
        public static IEnumerable<int> ListePlaques { get; } = CebPlaque.AnyPlaques;



        public ObservableCollection<string> Plaques { get; } =
            new ObservableCollection<string> { "", "", "", "", "", "" };
        public TimeSpan Duree {
            get => _duree;
            set {
                _duree = value;
                NotifiedChanged();
            }
        }
        public Visibility Vertical {
            get => _vertical;
            set {
                _vertical = value;
                ModeView = (value == Visibility.Visible) ? '\xE2' : '\xE0'; 
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
        public string Solution {
            get => _solution;
            set {
                _solution = value;
                NotifiedChanged();
            }
        }
        public ObservableCollection<CebBase> Solutions { get; set; }
          
        public int Search {
            get => Tirage.Search;
            set {
                Tirage.Search = value;
                NotifiedChanged();
                ClearData();
            }
        }

        public CebStatus Status => Tirage.Status;

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
                if (_isBusy) {
                    WaitStory.Begin();
                } else {
                    WaitStory.Pause();
                }
                NotifiedChanged();
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

        public bool CanExecute(object parameter) {
            return true;
        }

        public async void Execute(object parameter) {
            try {
                switch ((parameter as string)?.ToLower()) {
                    case "random":
                        await RandomAsync();
                        break;
                    case "resolve": {
                        switch (Tirage.Status) {
                            case CebStatus.Valid:
                                if (IsBusy) return;
                                await ResolveAsync();
                                break;

                            case CebStatus.CompteEstBon:
                            case CebStatus.CompteApproche:
                                await ClearAsync();
                                break;

                            case CebStatus.Erreur:
                                await RandomAsync();
                                break;
                            case CebStatus.Indefini:
                                break;
                            case CebStatus.EnCours:
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        break;
                    }
                    case "excel":
                    case "word":
                        await ExportAsync((string)parameter);
                        break;
                }
            } catch (Exception e) {
                Console.WriteLine(e);
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
                CebStatus.Valid => (Colors.Transparent, Colors.White),
                CebStatus.Erreur => (Colors.Red, Colors.White),
                CebStatus.CompteEstBon => (Colors.DarkSlateGray, Colors.GhostWhite),
                CebStatus.CompteApproche => (Colors.OrangeRed, Colors.White),
                CebStatus.EnCours => (Colors.White, Colors.White),
                _ => (Colors.Red, Colors.White)

            };
        }

        private void NotifiedChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ClearData() {
            if (_isUpdating) return;
            NotifiedChanged("Status");
            stopwatch.Reset();
            Duree = stopwatch.Elapsed;
            Solution = "";
            Count = 0;
            Solutions.Clear();
            Result = Tirage.Status != CebStatus.Erreur ? "" : "Tirage incorrect";
            Popup = false;
            UpdateColors();
        }

        private void UpdateData() {
            if (_isUpdating) return;
            _isUpdating = true;

            for (var i = 0; i < Tirage.Plaques.Count; i++)
                Plaques[i] = Tirage.Plaques[i].Text;
            _isUpdating = false;
            ClearData();

            // ReSharper disable once ExplicitCallerInfoArgument
            NotifiedChanged("Search");
        }

        public void ShowNotify(int index = 0) {
            if (index >= 0 && Tirage.Solutions.Count() != 0 && index < Tirage.Solutions.Count()) {
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

        public async Task ResolveAsync() {
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
            Duree = stopwatch.Elapsed;
            UpdateColors();
            IsBusy = false;
            Solution = Tirage.Solution();
            Tirage.Solutions.ForEach(s => Solutions.Add(s));
            Count = Tirage.Count;
            NotifiedChanged("Status");
            ShowNotify();
        }

        #endregion Action
    }
}