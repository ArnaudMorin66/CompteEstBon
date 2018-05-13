
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


namespace CebUwp
{
    class BindTirage : INotifyPropertyChanged
    {
        public MainPage Main { get; set; }
        private Brush _background;

        private double _duree;

        private Brush _foreground = new SolidColorBrush(Colors.White);

        private bool _isBusy;

        private bool _isEnabled = true;
        private DateTime _time;

        private string _result = "Résoudre";

        private Visibility _visibility = Visibility.Collapsed;
        public DispatcherTimer Dispatcher;

        public CebTirage Tirage { get; } = new CebTirage();

        public ObservableCollection<int> Plaques { get; } = new ObservableCollection<int> { -1, -1, -1, -1, -1, -1 };

        public IEnumerable<int> listePlaques { get; } = CebPlaque.ListePlaques.Distinct();

        public ObservableCollection<IList<string>> Solutions { get; } = new ObservableCollection<IList<string>>();

        public double Duree
        {
            get => _duree;
            set
            {
                _duree = value;
                NotifiedChanged();
            }
        }
        private int _search;
        public int Search
        {
            get => _search;
            set
            {
                if (_search == value) return;
                _search = value;
                Tirage.Search = value;
                NotifyChangedAndClear();
            }
        }

        public string Result
        {
            get => _result;
            set
            {
                if (value == _result) return;
                _result = value;
                NotifiedChanged();
            }
        }

        public Brush Background
        {
            get => _background;
            set
            {
                _background = value;
                NotifiedChanged();
            }
        }

        public Brush Foreground
        {
            get => _foreground;
            set
            {
                _foreground = value;
                NotifiedChanged();
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                Visibility = _isBusy ? Visibility.Visible : Visibility.Collapsed;
                NotifiedChanged();
            }
        }

        public Visibility Visibility
        {
            get => _visibility;
            set
            {
                _visibility = value;
                NotifiedChanged();
            }
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                UpdateColors();
                NotifiedChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Initialisation 
        /// </summary>
        /// <returns></returns>
        public BindTirage()
        {
            _background = new SolidColorBrush(Colors.Navy);
            Dispatcher = new DispatcherTimer
            {
                Interval = new TimeSpan(100)
            };
            Dispatcher.Tick += (sender, e) =>
            {
                Duree = (DateTime.Now - _time).TotalSeconds;
            };
            Plaques.CollectionChanged += (sender, e) =>
            {
                if (e.Action != NotifyCollectionChangedAction.Replace) return;
                var i = e.NewStartingIndex;
                Tirage.Plaques[i].Value = Plaques[i];
                NotifyChangedAndClear("Plaques");
            };
            UpdateData();
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
                    SetBrush(Colors.Firebrick, Colors.White);
                    break;
            }
        }

        private void NotifiedChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void NotifyChangedAndClear([CallerMemberName] string propertyName = "")
        {
            Duree = 0;
            Solutions.Clear();

            if (Tirage.Status != CebStatus.Erreur)
            {
                Result = "Résoudre";
                IsEnabled = true;
            }
            else
            {
                Result = "Tirage incorrect";
                IsEnabled = false;
            }
            NotifiedChanged(propertyName);
        }

        private void UpdateData()
        {
            for (var i = 0; i < Tirage.Plaques.Count; i++)
                Plaques[i] = Tirage.Plaques[i];
            NotifiedChanged("Plaques");
            Search = Tirage.Search;
        }

        #region Action

        public async Task ClearAsync()
        {
            await Tirage.ClearAsync();
            NotifiedChanged("Search");
            Solutions.Clear();
            NotifyChangedAndClear("Solutions");
        }

        public async Task RandomAsync()
        {
            await Tirage.RandomAsync();
            UpdateData();
        }

        public async Task<CebStatus> ResolveAsync()
        {
            IsBusy = true;
            _time = DateTime.Now;
            Result = "...Calcul...";
            SetBrush(Colors.Green, Colors.White);
            Dispatcher.Start();
            await Tirage.ResolveAsync();
            Result = Tirage.Status == CebStatus.CompteEstBon
                ? "Le Compte est bon"
                : (Tirage.Status == CebStatus.CompteApproche ? $"Compte approché: {Tirage.Found}" : "Tirage incorrect");
            Tirage.Solutions.ForEach(s => Solutions.Add(s.Operations));
            Dispatcher.Stop();
            Duree = (DateTime.Now - _time).TotalSeconds;
            SendToast();
            IsEnabled = false;
            IsBusy = false;
            return Tirage.Status;
        }

        #endregion Action

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

        public void SendToast()
        {
            var msg = $"Recherche: {Tirage.Search} - Plaques: ";
            for (var ip = 0; ip < Plaques.Count; ip++)
            {
                if (ip > 0)
                {
                    msg += ", ";
                }
                msg += Plaques[ip];
            }
            msg += "\n" + (Tirage.Status == CebStatus.CompteEstBon
                ? "Le Compte est bon"
                : (Tirage.Status == CebStatus.CompteApproche ? $"Compte approché: {Tirage.Found}" : "Tirage incorrect"));

            if (Solutions.Count > 0)
            {
                var sl = string.Empty;
                var i = true;
                foreach (var op in Solutions[0])
                {
                    var sep = (sl != string.Empty ? (i ? "\n" : ", ") : string.Empty);
                    sl += $"{sep}{op}";
                    i = !i;
                }
                msg += $"\n{sl}";
            }
            msg += $"\nNb de solutions: {Solutions.Count}  - Durée: {Duree} s";

            ToastTemplateType toastTemplate = ToastTemplateType.ToastText01;
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);

            XmlNodeList toastTextElements = toastXml.GetElementsByTagName("text");
            toastTextElements[0].AppendChild(toastXml.CreateTextNode(msg));

            ToastNotificationManager.CreateToastNotifier().Show(new ToastNotification(toastXml));
        }
    }
}
