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
using CompteEstBon.Properties;

#endregion

namespace CompteEstBon.ViewModel {
    public class ViewTirage : INotifyPropertyChanged, ICommand {
        private readonly Storyboard AnimationStory =
            Application.Current.MainWindow?.FindResource("AnimationResult") as Storyboard;

        private readonly Stopwatch NotifyWatch = new Stopwatch();
        private readonly TimeSpan SolutionTimer = TimeSpan.FromSeconds(Settings.Default.SolutionTimer);

        private readonly Storyboard WaitStory =
            Application.Current.MainWindow?.FindResource("WaitStoryboard") as Storyboard;

        private Color _background = Colors.Navy;

        private string _duree;

        private Color _foreground = Colors.White;
        private bool _isBusy;
        private bool _isUpdating;
        private int _notifyHeight;

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
                Interval = TimeSpan.FromMilliseconds(10)
            };
            dateDispatcher.Tick += (sender, e) => {
                if (stopwatch.IsRunning) Duree = stopwatch.Elapsed.ToString();
                if (NotifyHeight != 0 && NotifyWatch.Elapsed > SolutionTimer) HideNotify();
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

        public static IEnumerable<int> ListePlaques { get; } = CebPlaque.ListePlaques.Distinct();

        public CebTirage Tirage { get; } = new CebTirage();

        public ObservableCollection<string> Plaques { get; } =
            new ObservableCollection<string> { "", "", "", "", "", "" };

        public ObservableCollection<CebDetail> Solutions { get; } = new ObservableCollection<CebDetail>();

        public string Duree {
            get => _duree;
            set {
                _duree = value;
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
                    AnimationStory.Begin();
                }
                else {
                    WaitStory.Pause();
                }

                NotifiedChanged();
            }
        }

        public int NotifyHeight {
            get => _notifyHeight;
            set {
                if (_notifyHeight == value) return;
                _notifyHeight = value;
                NotifyWatch.Stop();
                NotifyWatch.Reset();
                if (_notifyHeight != 0) NotifyWatch.Start();
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
                    case "resolve":
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
                    case "excel":
                    case "word":
                        await Export((string) parameter);
                        break;
                }
            }
            catch (Exception) {
                // ignored
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private async Task Export(string fmt) {
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
            switch (Tirage.Status) {
                case CebStatus.Valid:
                    (Background, Foreground) = (Colors.DarkOliveGreen, Colors.White);
                    break;

                case CebStatus.Erreur:
                    (Background, Foreground) = (Colors.Red, Colors.White);
                    break;

                case CebStatus.CompteEstBon:
                    (Background, Foreground) = (Colors.LightGreen, Colors.Black);
                    break;

                case CebStatus.CompteApproche:
                    (Background, Foreground) = (Colors.Salmon, Colors.White);
                    break;

                case CebStatus.EnCours:
                    (Background, Foreground) = (Colors.Yellow, Colors.White);
                    break;
            }
        }

        private void NotifiedChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ClearData() {
            if (_isUpdating) return;
            AnimationStory.Stop();
            // ReSharper disable once ExplicitCallerInfoArgument
            NotifiedChanged("Status");
            stopwatch.Reset();
            Duree = stopwatch.Elapsed.ToString();
            Solutions.Clear();
            Solution = "";
            Result = Tirage.Status != CebStatus.Erreur ? "" : "Tirage incorrect";
            HideNotify();
            UpdateColors();
        }

        private void UpdateData() {
            if (_isUpdating) return;
            _isUpdating = true;

            for (var i = 0; i < Tirage.Plaques.Length; i++)
                Plaques[i] = Tirage.Plaques[i].Text;

            _isUpdating = false;
            ClearData();

            // ReSharper disable once ExplicitCallerInfoArgument
            NotifiedChanged("Search");
        }

        public void ShowNotify(int index = 0) {
            if (index >= 0 && Solutions.Count != 0 && index < Solutions.Count) {
                Solution = Tirage.Solutions[index].ToString();
                NotifyHeight = 84;
            }
        }

        private void HideNotify() {
            NotifyHeight = 0;
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

        

        public async Task<CebStatus> ResolveAsync() {
            IsBusy = true;
            Result = "...Calcul...";
            (Background, Foreground) = (Colors.Green, Colors.White);
            stopwatch.Start();
            await Tirage.ResolveAsync();
            Result = Tirage.Status == CebStatus.CompteEstBon
                ? "Le Compte est bon"
                : Tirage.Status == CebStatus.CompteApproche
                    ? $"Compte approché: {Tirage.Found}, écart: {Tirage.Diff}"
                    : "Tirage incorrect";

            foreach (var s in Tirage.Solutions)
                Solutions.Add(s.ToCebDetail());
            stopwatch.Stop();
            Duree = stopwatch.Elapsed.ToString();
            Solution = Tirage.Solution.ToString();
            UpdateColors();
            IsBusy = false;
            Solution = Tirage.Solution.ToString();
            // ReSharper disable once ExplicitCallerInfoArgument
            NotifiedChanged("Status");
            ShowNotify();
            return Tirage.Status;
        }

        #endregion Action
    }
}