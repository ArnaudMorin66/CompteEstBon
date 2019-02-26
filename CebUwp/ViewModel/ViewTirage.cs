using CompteEstBon;
using Microsoft.Toolkit.Uwp.UI.Controls;
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
using Syncfusion.XlsIO;
using Windows.System;
using Windows.UI.Xaml.Media.Animation;

namespace CompteEstBon
{

    internal class ViewTirage : INotifyPropertyChanged
    {
        private Color _background;
        private double _duree;

        private Color _foreground;

        private bool _isBusy;

        private bool _isCalculed;
        private DateTimeOffset _time;

        private string _result = "Résoudre";

        private Visibility _visibility = Visibility.Collapsed;
        public DelegateCommand<object> HasardCommand { get; set; }

        public DelegateCommand<object> ResolveCommand { get; set; }

        public DelegateCommand<object> ExportCommand { get; set; }


        public DispatcherTimer heureDispatcher;
        public DispatcherTimer dateDispatcher;

        public CebTirage Tirage { get; } = new CebTirage();

        public ObservableCollection<int> Plaques { get; } = new ObservableCollection<int> { -1, -1, -1, -1, -1, -1 };

        public IEnumerable<int> ListePlaques { get; } = CebPlaque.ListePlaques.Distinct();
        private Visibility _notifyVisibility = Visibility.Collapsed;
        public Visibility NotifyVisibility {
            get => _notifyVisibility;
            set {
                _notifyVisibility = value;
                NotifiedChanged();
            }
        }
        public DispatcherTimer NotifyTimer;
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

        public Storyboard storyBoard { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Initialisation 
        /// </summary>
        /// <returns>
        /// </returns>
        public ViewTirage()
        {
            Symbol = ListeSymbols[CebStatus.Valid];
            _background = Colors.Navy;
            _foreground = Colors.White;
            HasardCommand = new DelegateCommand<object>(async _ => await RandomAsync());
            ResolveCommand = new DelegateCommand<object>(async _ =>
            {
              
                switch (Tirage.Status)
                {
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
            });

            ExportCommand = new DelegateCommand<object>(async _ => await ExportToExcelAsync());

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
            NotifyTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            NotifyTimer.Tick += (sender, e) =>
             {
                 NotifyVisibility = Visibility.Collapsed;
                 NotifyTimer.Stop();
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
                    (Background, Foreground) = (Colors.Navy, Colors.Yellow);
                    break;

                case CebStatus.Erreur:
                    (Background, Foreground) = (Colors.Red, Colors.White);
                    break;

                case CebStatus.CompteEstBon:
                    (Background, Foreground) = (Colors.Green, Colors.Yellow);
                    break;

                case CebStatus.CompteApproche:
                    (Background, Foreground) = (Colors.Salmon, Colors.White);
                    break;

                case CebStatus.EnCours:
                    (Background, Foreground) = (Colors.Green, Colors.White);
                    break;
            }
        }

        private void NotifiedChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void ClearData()
        {
            Duree = 0;
            storyBoard?.Pause();
            if (NotifyTimer.IsEnabled)
            {
                NotifyTimer.Stop();
                NotifyVisibility = Visibility.Collapsed;
            }
            Solutions.Clear();
            IsCalculed = false;
            Result = (Tirage.Status != CebStatus.Erreur) ? "Résoudre" : "Tirage invalide";
            UpdateColors();
        }

        private void UpdateData()
        {
            lock (Plaques)
            {
                for (var i = 0; i < Tirage.Plaques.Length; i++)
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
            ClearData();
            UpdateData();
        }

        public async Task<CebStatus> ResolveAsync()
        {
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
            IsCalculed = (Tirage.Status == CebStatus.CompteEstBon || Tirage.Status == CebStatus.CompteApproche);
            foreach (var s in Tirage.Solutions)
                Solutions.Add(s.Operations);
            heureDispatcher.Stop();
            Duree = (DateTimeOffset.Now - _time).TotalSeconds;
            UpdateColors();
            IsBusy = false;
            storyBoard.Resume();
            ShowNotify(0);
            return Tirage.Status;
        }

        #endregion Action

        public string SolutionToString(int index = 0) => (index == -1) ? "" : Tirage.Solutions[index].ToString();

        //public void SetBrush(Color background, Color foreground)
        //{
        //    RadialGradientBrush brush = new RadialGradientBrush
        //    {
        //        AlphaMode = AlphaMode.Premultiplied,
        //        RadiusX = 0.6,
        //        RadiusY = 0.8
        //    };
        //    brush.SpreadMethod = GradientSpreadMethod.Reflect;
        //    brush.GradientStops.Add(new GradientStop
        //    {
        //        Color = background,
        //        Offset = 0.0
        //    });
        //    brush.GradientStops.Add(new GradientStop
        //    {
        //        Color = Colors.Black,
        //        Offset = 0.9
        //    });
        //    brush.GradientStops.Add(new GradientStop
        //    {
        //        Color = Colors.Transparent,
        //        Offset = 1.0
        //    });
        //    Background = brush;
        //    Foreground = new SolidColorBrush(foreground);
        //}

        public string _currentSolution;

        public string CurrentSolution {
            get => _currentSolution;
            set {
                _currentSolution = value;
                NotifiedChanged();
            }
        }
        public void ShowNotify(int no)
        {
            if (no < 0)
            {
                NotifyVisibility = Visibility.Collapsed;
                return;
            }
            if (NotifyVisibility == Visibility.Visible)
                return;
            CurrentSolution = Tirage.Solutions[no].ToString();
            NotifyVisibility = Visibility.Visible;
            NotifyTimer.Start();
        }
        public Dictionary<CebStatus, string> ListeSymbols { get; } = new Dictionary<CebStatus, String>()
        {
            [CebStatus.CompteApproche] = "Dislike",
            [CebStatus.CompteEstBon] = "Like",
            [CebStatus.Valid] = "Play",
            [CebStatus.EnCours] = "Sync",
            [CebStatus.Erreur] = "ReportHacked"
        };

        public async Task ExportToExcelAsync()
        {
            FileSavePicker savePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };
            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("Excel", new List<string>() { ".xlsx" });
            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = "Ceb";
            StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                // Prevent updates to the remote version of the file until we finish making changes and call CompleteUpdatesAsync.
                CachedFileManager.DeferUpdates(file);
                using (var excelEngine = new ExcelEngine())
                {
                    var xlApp = excelEngine.Excel;
                    xlApp.DefaultVersion = ExcelVersion.Excel2016;

                    var workBook = xlApp.Workbooks.Create(1);
                    var ws = workBook.Worksheets[0];
                   
                    for (var i = 0; i < 6; i++)
                    {
                        ws.Range[1, i + 1].Value2 = $"Plaque {i + 1}";
                        ws.Range[2, i + 1].Value2 = Plaques[i];
                    }
                    
                    ws.Range[1, 7].Value2 = "Cherche";
                    ws.Range[2, 7].Value2 = Search;
                    ws.ListObjects.Create("Table1", ws["A1:G2"])
                       .BuiltInTableStyle = TableBuiltInStyles.TableStyleMedium1;

                    ws.Range[4, 1].Value2 = Result;

                    ws.ImportData(Tirage.Solutions.Select(s => s.ToCebDetail()), 7, 1, true);
                    for (var i = 1; i < 6; i++)
                    {
                        ws.Range[7, i].Value2 = $"Opération {i}";
                    }

                    ws.ListObjects.Create("Table1", ws[$"A7:E{Solutions.Count + 7}"])
                        .BuiltInTableStyle = TableBuiltInStyles.TableStyleMedium1;
                    await workBook.SaveAsAsync(file);
                }
                // Let Windows know that we're finished changing the file so the other app can update the remote version of the file.
                // Completing updates may require Windows to ask for user input.
                FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);

                await Launcher.LaunchFileAsync(file);

            }
        }
    }
}