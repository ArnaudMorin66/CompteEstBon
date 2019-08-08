using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Windows.UI.Xaml.Controls;

namespace CompteEstBon.ViewModel {

    internal class ViewTirage : INotifyPropertyChanged, ICommand {
        private ValueSet xlData;

        private Color _background;
        private double _duree;

        private Color _foreground;

        private bool _isBusy;

        public InAppNotification InAppNotification { get; set; }

        // private bool _isCalculed;
        private DateTimeOffset _time;

        private string _result = "Résoudre";


        private Visibility _visibility = Visibility.Collapsed;
        public DispatcherTimer heureDispatcher;
        public DispatcherTimer dateDispatcher;

        public CebTirage Tirage { get; } = new CebTirage();

        public ObservableCollection<int> Plaques { get; } = new ObservableCollection<int> { -1, -1, -1, -1, -1, -1 };

        public IEnumerable<int> ListePlaques { get; } = CebPlaque.ListePlaques.Distinct();

        public ObservableCollection<CebDetail> Solutions { get; } = new ObservableCollection<CebDetail>();
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

        public CebStatus Status {
            get => Tirage.Status;
        }
        public Visibility Visibility {
            get => _visibility;
            set {
                _visibility = value;
                NotifiedChanged();
            }
        }

        public Storyboard StoryBoard { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;
        #pragma warning disable CS0067
        public event EventHandler CanExecuteChanged;
        #pragma warning restore CS0067

        /// <summary>
        /// Initialisation 
        /// </summary>
        /// <returns>
        /// </returns>
        public ViewTirage() {
            
            App.AppServiceConnected += ViewTirage_AppServiceConnected;
            
            Symbol = ListeSymbols[CebStatus.Valid];
            _background = Colors.DarkSlateGray;
            _foreground = Colors.White;

            heureDispatcher = new DispatcherTimer {
                Interval = new TimeSpan(100)
            };
            heureDispatcher.Tick += (sender, e) => Duree = (DateTimeOffset.Now - _time).TotalSeconds;
            dateDispatcher = new DispatcherTimer {
                Interval = new TimeSpan(1000)
            };
            dateDispatcher.Tick += (sender, e) => {
                Date = Date = $"{DateTime.Now:dddd dd MMMM yyyy à HH:mm:ss}";
            };

            Plaques.CollectionChanged += (sender, e) => {
                if (e.Action != NotifyCollectionChangedAction.Replace) return;
                var i = e.NewStartingIndex;
                Tirage.Plaques[i].Value = Plaques[i];
                ClearData();
            };
            UpdateData();
            UpdateColors();
            Date = $"{DateTime.Now:dddd MM yyyy, HH:mm:ss}";
            dateDispatcher.Start();
        }

        private async void ViewTirage_AppServiceConnected(object sender, EventArgs e) {
            AppServiceResponse response = await App.Connection.SendMessageAsync(xlData);

            // check the result
            object result;

            MessageDialog dialog;
            if (!response.Message.TryGetValue("RESPONSE", out result)) {
                dialog = new MessageDialog("RESPONSE introuvable");
                await dialog.ShowAsync();
            } else if (result.ToString() != "SUCCESS") {
                dialog = new MessageDialog(result.ToString());
                await dialog.ShowAsync();
            }

            // no longer need the AppService connection
            App.AppServiceDeferral.Complete();
            IsBusy = false;
        }

        private void UpdateColors() {
            Symbol = ListeSymbols[Tirage.Status];
            
            (Background, Foreground) = Tirage.Status switch
            {
                CebStatus.Valid => (Colors.DarkSlateGray, Colors.Yellow),
                CebStatus.Erreur => (Colors.Red, Colors.White),
                CebStatus.CompteEstBon => (Colors.DarkGreen, Colors.Yellow),
                CebStatus.CompteApproche => (Colors.Salmon, Colors.White),
                CebStatus.EnCours => (Colors.Green, Colors.White),
                _ => (Colors.DarkSlateGray, Colors.Yellow)

            };

        }

        private void NotifiedChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void ClearData() {
            Duree = 0;
            StoryBoard?.Pause();

            InAppNotification?.Dismiss();
            Solutions.Clear();
            // ReSharper disable once ExplicitCallerInfoArgument
            NotifiedChanged("Status");


            Result = (Tirage.Status != CebStatus.Erreur) ? "Résoudre" : "Tirage invalide";
            UpdateColors();
        }

        private void UpdateData() {
            lock (Plaques) {
                for (var i = 0; i < Tirage.Plaques.Length; i++) {
                    Plaques[i] = Tirage.Plaques[i];
                }
            }
            // ReSharper disable once ExplicitCallerInfoArgument
            NotifiedChanged("Search");
        }

        #region Action

        public async Task ClearAsync() {
            await Tirage.ClearAsync();
            ClearData();
        }

        public async Task RandomAsync() {

            await Tirage.RandomAsync();
            ClearData();
            UpdateData();
        }

        public async Task<CebStatus> ResolveAsync() {
            IsBusy = true;
            Result = "...Calcul...";
            Symbol = ListeSymbols[CebStatus.EnCours];
            (Background, Foreground) = (Colors.Green, Colors.White);
            _time = DateTimeOffset.Now;
            heureDispatcher.Start();
            await Tirage.ResolveAsync();

            Result = Tirage.Status == CebStatus.CompteEstBon
                ? "Le Compte est bon"
                : (Tirage.Status == CebStatus.CompteApproche ?
                $"Compte approché: {Tirage.Found}, écart: {Tirage.Diff}" : "Tirage incorrect");
            // ReSharper disable once ExplicitCallerInfoArgument
            NotifiedChanged("Status");
            foreach (var s in Tirage.Solutions)
                Solutions.Add(s.Detail);
            heureDispatcher.Stop();
            Duree = (DateTimeOffset.Now - _time).TotalSeconds;
            UpdateColors();
            IsBusy = false;
            StoryBoard.Resume();
            ShowNotify(0);
            return Tirage.Status;
        }

        #endregion Action

        public string SolutionToString(int index = 0) => (index == -1) ? "" : Tirage.Solutions.ElementAt(index).ToString();


        public string _currentSolution;

        public string CurrentSolution {
            get => _currentSolution;
            set {
                _currentSolution = value;
                NotifiedChanged();
            }
        }
        public void ShowNotify(int no) {
            InAppNotification?.Dismiss();
            if (no < 0) {

                return;
            }

            CurrentSolution = Tirage.Solutions[no].ToString();
            InAppNotification?.Show(10000);

        }
        public Dictionary<CebStatus, string> ListeSymbols { get; } = new Dictionary<CebStatus, string>() {
            [CebStatus.CompteApproche] = "Dislike",
            [CebStatus.CompteEstBon] = "Like",
            [CebStatus.Valid] = "Play",
            [CebStatus.EnCours] = "Sync",
            [CebStatus.Erreur] = "ReportHacked"
        };

        public async Task ExportAsync(string cmd) {
            try {
                xlData = new ValueSet {
                    { "Command", cmd },
                    { "Plaques", Plaques.ToArray() },
                    { "Search", Search },
                    { "Result", Result },
                    { "Status", (int)Tirage.Status },
                    { "Solutions", Tirage.Solutions.Select(p=> p.ToString()).ToArray() }
                 };
            } catch (Exception e) {
                MessageDialog d = new MessageDialog(e.ToString());
                await d.ShowAsync();
                return;
            }
            if (ApiInformation.IsApiContractPresent("Windows.ApplicationModel.FullTrustAppContract", 1, 0)) {
                IsBusy = true;
                await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();
            } else {

                MessageDialog dialog = new MessageDialog("This feature is only available on Windows 10 Desktop SKU");
                await dialog.ShowAsync();
            }
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
                        }
                        break;
                    case "excel":
                        await ExportAsync("excel");
                        break;
                    case "word":
                        await ExportAsync("word");
                        break;
                }
            } catch (Exception) {
                // ignored
            }
        }

    }

}