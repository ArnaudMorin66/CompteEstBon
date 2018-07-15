#region

using CompteEstBon;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

#endregion

namespace UwpCompteEstBon
{
    
    public class BindingTirage : INotifyPropertyChanged
    {
        private Brush _background;
        private double _duree;

        private Brush _foreground = new SolidColorBrush(Colors.White);

        private bool _isBusy;

        private bool _isCalculed = false;
        private DateTimeOffset _time;

        private string _result = "Résoudre";

        private Visibility _visibility = Visibility.Collapsed;
        public DispatcherTimer Dispatcher;
        public DispatcherTimer dateDispatcher;

        public CebTirage Tirage { get; } = new CebTirage();

        public ObservableCollection<int> Plaques { get; } = new ObservableCollection<int> { -1, -1, -1, -1, -1, -1 };

        public IEnumerable<int> ListePlaques { get; } = CebPlaque.ListePlaques.Distinct();

        public ObservableCollection<IList<string>> Solutions { get; } = new ObservableCollection<IList<string>>();
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

        public Brush Background {
            get => _background;
            set {
                _background = value;
                NotifiedChanged();
            }
        }

        public Brush Foreground {
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


        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Initialisation
        /// </summary>
        /// <returns></returns>
        public BindingTirage()
        {
            _background = new SolidColorBrush(Colors.Navy);
            Dispatcher = new DispatcherTimer
            {
                Interval = new TimeSpan(100)

            };
            Dispatcher.Tick += (sender, e) => {
                Duree = (DateTimeOffset.Now - _time).TotalSeconds;
            };
            dateDispatcher = new DispatcherTimer
            {
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

        private void DateDispatcher_Tick(object sender, object e)
        {
            throw new NotImplementedException();
        }

        private void UpdateColors()
        {
            switch (Tirage.Status)
            {
                case CebStatus.Valid:
                    SetBrush(Colors.Navy, Colors.Yellow);
                    break;

                case CebStatus.Erreur:
                    SetBrush(Colors.Red, Colors.White);
                    break;

                case CebStatus.CompteEstBon:
                    SetBrush(Colors.Green, Colors.Yellow);
                    break;

                case CebStatus.CompteApproche:
                    SetBrush(Colors.Salmon, Colors.White);
                    break;
                case CebStatus.EnCours:
                    SetBrush(Colors.Green, Colors.White);
                    break;
            }
        }

        private void NotifiedChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void ClearData()
        {
            Duree = 0;
            FirstSolutionString = "";
            Solutions.Clear();
            IsCalculed = false;

            if (Tirage.Status != CebStatus.Erreur)
            {
                Result = "Résoudre";
            }
            else
            {
                Result = "Tirage incorrect";
            }
            UpdateColors();
        }

        private void UpdateData()
        {
            lock (Plaques)
            {
                for (var i = 0; i < Tirage.Plaques.Count; i++)
                    Plaques[i] = Tirage.Plaques[i];
            }
            NotifiedChanged("Search");
        }

        #region Action

        public async Task ClearAsync()
        {
            await Tirage.ClearAsync();
            ClearData();
        }

        public async Task RandomAsync()
        {
            await Tirage.RandomAsync();
            FirstSolutionString = "";
            UpdateData();
        }

        public async Task<CebStatus> ResolveAsync()
        {
            IsBusy = true;

            Result = "...Calcul...";
            SetBrush(Colors.Green, Colors.White);
            _time = DateTimeOffset.Now;
            Dispatcher.Start();
            await Tirage.ResolveAsync();

            Result = Tirage.Status == CebStatus.CompteEstBon
                ? "Le Compte est bon"
                : (Tirage.Status == CebStatus.CompteApproche ?
                $"Compte approché: {Tirage.Found}, écart: {Tirage.Diff}" : "Tirage incorrect");
            IsCalculed = (Tirage.Status == CebStatus.CompteEstBon || Tirage.Status == CebStatus.CompteApproche);
            FirstSolutionString = Tirage.Solutions[0].ToString();
            Tirage.Solutions.ForEach(s => Solutions.Add(s.Operations));
            Dispatcher.Stop();
            Duree = (DateTimeOffset.Now - _time).TotalSeconds;
            UpdateColors();
            IsBusy = false;
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
        public void SetBrush(Color background, Color foreground)
        {
            LinearGradientBrush myLinearGradientBrush =
                new LinearGradientBrush
                {
                    StartPoint = new Windows.Foundation.Point(0, 0),
                    EndPoint = new Windows.Foundation.Point(0, 1)
                };

            myLinearGradientBrush.GradientStops.Add(new GradientStop
            {
                Color = background,
                Offset = 0.0
            });

            myLinearGradientBrush.GradientStops.Add(new GradientStop
            {
                Color = Colors.Black,
                Offset = 1.0
            });
            Background = myLinearGradientBrush;
            Foreground = new SolidColorBrush(foreground);
        }
    }
}