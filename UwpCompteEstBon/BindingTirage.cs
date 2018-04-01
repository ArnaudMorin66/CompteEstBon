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
        private Brush _background = new SolidColorBrush(Colors.CornflowerBlue);

        private double _duree;
       
        private Brush _foreground = new SolidColorBrush(Colors.White);

        private bool _isBusy;

        private bool _isEnabled = true;
        private DateTime _time;

        private string _result = "Résoudre";

        private Visibility _visibility = Visibility.Collapsed;
        public DispatcherTimer Dispatcher;

        public BindingTirage()
        {
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
                // ReSharper disable once ExplicitCallerInfoArgument
                NotifyChangedAndClear("Plaques");
            };
            UpdateData();
        }

        public CebTirage Tirage { get; } = new CebTirage();

        public ObservableCollection<int> Plaques { get; } = new ObservableCollection<int> { -1, -1, -1, -1, -1, -1 };

        // ReSharper disable once InconsistentNaming
        public IEnumerable<int> listePlaques { get; } = CebPlaque.ListePlaques.Distinct();

        public ObservableCollection<CebBase> Solutions { get; } = new ObservableCollection<CebBase>();

        public double Duree
        {
            get => _duree;
            set
            {
                _duree = value;
                NotifiedChanged();
            }
        }

        public int Search
        {
            get => Tirage.Search;
            set
            {
                if (Tirage.Search == value) return;
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

        private void UpdateColors()
        {
            switch (Tirage.Status)
            {
                case CebStatus.Valid:
                    Background = new SolidColorBrush(Colors.Navy);
                    Foreground = new SolidColorBrush(Colors.White);
                    break;

                case CebStatus.Erreur:
                    Background = new SolidColorBrush(Colors.LightGray);
                    Foreground = new SolidColorBrush(Colors.Black);
                    break;

                case CebStatus.CompteEstBon:
                    Background = new SolidColorBrush(Colors.Blue);
                    Foreground = new SolidColorBrush(Colors.Yellow);
                    break;

                case CebStatus.CompteApproche:
                    Background = new SolidColorBrush(Colors.BlueViolet);
                    Foreground = new SolidColorBrush(Colors.Yellow);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void NotifiedChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

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
            lock (Plaques)
            {
                for (var i = 0; i < Tirage.Plaques.Count; i++)
                    Plaques[i] = Tirage.Plaques[i];
            }
            Solutions.Clear();
         }

        #region Action

        public async Task ClearAsync()
        {
            await Tirage.ClearAsync();
            // ReSharper disable once ExplicitCallerInfoArgument
            NotifiedChanged("Search");
            UpdateData();
        }

        public async Task RandomAsync()
        {
            await Tirage.RandomAsync();
            // ReSharper disable once ExplicitCallerInfoArgument
            NotifiedChanged("Search");
            UpdateData();
        }

        public async Task<CebStatus> ResolveAsync()
        {
            IsBusy = true;
            _time = DateTime.Now;
            Result = "...Calcul...";
            Background = new SolidColorBrush(Colors.DarkGreen);
            Dispatcher.Start();
            await Tirage.ResolveAsync();
            Result = Tirage.Status == CebStatus.CompteEstBon
                ? "Le Compte est bon"
                : (Tirage.Status == CebStatus.CompteApproche ? $"Compte approché: {Tirage.Found}" : "Tirage incorrect");

            Tirage.Solutions.ForEach(s => Solutions.Add(s));
          
            Dispatcher.Stop();
            Duree = (DateTime.Now - _time).TotalSeconds;
            SendToast();
            IsEnabled = false;
            IsBusy = false;
            return Tirage.Status;
        }

        #endregion Action

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
                var sl = "";
                var i = true;
                foreach (var op in Solutions[0].Operations)
                {
                    var sep = (sl != "" ? (i ? "\n" : ", ") : "");
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