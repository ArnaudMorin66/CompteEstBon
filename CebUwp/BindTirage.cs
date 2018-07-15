using CompteEstBon;
using Microsoft.Toolkit.Uwp.UI.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace CebUwp
{

    internal class BindTirage : INotifyPropertyChanged
    {
        private Brush _background;
        private double _duree;

        private Brush _foreground;

        private bool _isBusy;

        private bool _isCalculed;
        private DateTimeOffset _time;

        private string _result = "Résoudre";

        private Visibility _visibility = Visibility.Collapsed;
        public DispatcherTimer heureDispatcher;
        public DispatcherTimer dateDispatcher;

        public CebTirage Tirage { get; } = new CebTirage();

        public ObservableCollection<int> Plaques { get; } = new ObservableCollection<int> { -1, -1, -1, -1, -1, -1 };

        public IEnumerable<int> ListePlaques { get; } = CebPlaque.ListePlaques.Distinct();

        public ObservableCollection<IList<string>> Solutions { get; } = new ObservableCollection<IList<string>>();
        public string _date;

        public string Date
        {
            get => _date;
            set
            {
                _date = value;
                NotifiedChanged();
            }
        }

        public double Duree
        {
            get => _duree;
            set
            {
                _duree = value;
                NotifiedChanged();
            }
        }

        private string _symbol;

        public string Symbol
        {
            get => _symbol;
            set
            {
                _symbol = value;
                NotifiedChanged();
            }
        }

        public int Search
        {
            get => Tirage.Search;
            set
            {
                Tirage.Search = value;
                // ClearData();
                NotifiedChanged();
                ClearData();
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

        public bool IsCalculed
        {
            get => _isCalculed;
            set
            {
                _isCalculed = value;
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

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Initialisation 
        /// </summary>
        /// <returns>
        /// </returns>
        public BindTirage()
        {
            Symbol = ListeSymbols[CebStatus.Valid];
            _background = new SolidColorBrush(Colors.Navy);
            _foreground = new SolidColorBrush(Colors.White);
            heureDispatcher = new DispatcherTimer
            {
                Interval = new TimeSpan(100)
            };
            heureDispatcher.Tick += (sender, e) => Duree = (DateTimeOffset.Now - _time).TotalSeconds;
            dateDispatcher = new DispatcherTimer
            {
                Interval = new TimeSpan(1000)
            };
            dateDispatcher.Tick += (sender, e) =>
            {
                Date = Date = $"{DateTime.Now:dddd dd MMMM yyyy à HH:mm:ss}";
            };

            Plaques.CollectionChanged += (sender, e) =>
            {
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

        private void UpdateColors()
        {
            Symbol = ListeSymbols[Tirage.Status];
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
            Solutions.Clear();
            IsCalculed = false;
            Result = (Tirage.Status != CebStatus.Erreur) ? "Résoudre" : "Tirage invalide";
            UpdateColors();
        }

        private void UpdateData()
        {
            lock (Plaques)
            {
                for (var i = 0; i < Tirage.Plaques.Count; i++)
                {
                    Plaques[i] = Tirage.Plaques[i];
                }
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
            UpdateData();
        }

        public async Task<CebStatus> ResolveAsync()
        {
            IsBusy = true;
            Result = "...Calcul...";
            Symbol = ListeSymbols[CebStatus.EnCours];
            SetBrush(Colors.Green, Colors.White);
            _time = DateTimeOffset.Now;
            heureDispatcher.Start();
            await Tirage.ResolveAsync();

            Result = Tirage.Status == CebStatus.CompteEstBon
                ? "Le Compte est bon"
                : (Tirage.Status == CebStatus.CompteApproche ?
                $"Compte approché: {Tirage.Found}, écart: {Tirage.Diff}" : "Tirage incorrect");
            IsCalculed = (Tirage.Status == CebStatus.CompteEstBon || Tirage.Status == CebStatus.CompteApproche);
            Tirage.Solutions.ForEach(s => Solutions.Add(s.Operations));
            heureDispatcher.Stop();
            Duree = (DateTimeOffset.Now - _time).TotalSeconds;
            UpdateColors();
            IsBusy = false;
            return Tirage.Status;
        }

        #endregion Action

        public string SolutionToString(int index = 0) => (index == -1) ? "" : Tirage.Solutions[index].ToString();

        public void SetBrush(Color background, Color foreground)
        {
            RadialGradientBrush brush = new RadialGradientBrush
            {
                AlphaMode = AlphaMode.Premultiplied,
                RadiusX = 0.6,
                RadiusY = 0.8
            };
            brush.SpreadMethod = GradientSpreadMethod.Reflect;
            brush.GradientStops.Add(new GradientStop
            {
                Color = background,
                Offset = 0.0
            });
            brush.GradientStops.Add(new GradientStop
            {
                Color = Colors.Black,
                Offset = 0.9
            });
            brush.GradientStops.Add(new GradientStop
            {
                Color = Colors.Transparent,
                Offset = 1.0
            });
            Background = brush;
            Foreground = new SolidColorBrush(foreground);
        }

        public string _currentSolution;

        public string CurrentSolution
        {
            get => _currentSolution;
            set
            {
                _currentSolution = value;
                NotifiedChanged();
            }
        }

        public Dictionary<CebStatus, string> ListeSymbols { get; } = new Dictionary<CebStatus, String>()
        {
            [CebStatus.CompteApproche] = "Dislike",
            [CebStatus.CompteEstBon] = "Like",
            [CebStatus.Valid] = "Play",
            [CebStatus.EnCours] = "Sync",
            [CebStatus.Erreur] = "ReportHacked"
        };

        public async Task ExportToCsvAsync()
        {
            FileSavePicker savePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };
            savePicker.FileTypeChoices.Add("csv", new List<string>() { ".csv" });
            savePicker.SuggestedFileName = "Ceb";
            StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                CachedFileManager.DeferUpdates(file);
                var tmp = string.Join(";", Plaques.Select(p => p.ToString()));
                await FileIO.WriteTextAsync(file, $"Plaques;{tmp};Recherche;{Tirage.Search}\n");
                await FileIO.AppendTextAsync(file, $"{Result};Nb solutions:{Solutions.Count};Durée:{Duree}\n\n");
                await FileIO.AppendTextAsync(file, "Operation 1;Operation 2;Operation 3;Operation 4;Operation 5\n");
                await FileIO.AppendLinesAsync(file, Solutions.Select((p) => string.Join(";", p)));
                FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
            }
        }
    }
}