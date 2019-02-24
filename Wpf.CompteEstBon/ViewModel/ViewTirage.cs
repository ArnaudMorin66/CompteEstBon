#region

using CompteEstBon;
using Microsoft.Win32;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Converter;
using Syncfusion.Windows.Shared;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

#endregion

namespace CompteEstBon
{

    public class ViewTirage : NotificationObject
    {
        public static IEnumerable<int> ListePlaques { get; } = CebPlaque.ListePlaques.Distinct();

        private string _duree;
        public Stopwatch stopwatch;

        private Color _foreground = Colors.White;
        private Color _background = Colors.Navy;
        private bool _isBusy;

        private bool _isCalculed = false;

        private string _result = "Résoudre";

        private Visibility _visibility = Visibility.Collapsed;
        private Visibility _notifyVisibility = Visibility.Hidden;
        public DelegateCommand<object> HasardCommand { get; set; }

        public DelegateCommand<object> ResolveCommand { get; set; }

        public DelegateCommand<SfDataGrid> ExportCommand { get; set; }

        public DelegateCommand<SfDataGrid> NotifyCommand { get; set; }

        public DispatcherTimer dateDispatcher;
        public DispatcherTimer notifyTimer;

        public CebTirage Tirage { get; } = new CebTirage();

        public ObservableCollection<string> Plaques { get; } = new ObservableCollection<string> { "", "", "", "", "", "" };

        public ObservableCollection<CebDetail> Solutions { get; } = new ObservableCollection<CebDetail>();

        public string Duree {
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

        public string _solution;

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

        public Visibility NotifyVisibility {
            get => _notifyVisibility;
            set {
                _notifyVisibility = value;
                NotifiedChanged();
            }
        }

        private string _titre = "Le compte est bon";

        public string Titre {
            get => _titre;
            set {
                _titre = value;
                NotifiedChanged();
            }
        }

        /// <summary>
        /// Initialisation
        /// </summary>
        /// <returns>
        /// </returns>
        public ViewTirage()
        {
            HasardCommand = new DelegateCommand<object>(async (_) => await RandomAsync());
            ResolveCommand = new DelegateCommand<object>(
                 async (_) =>
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

            ExportCommand = new DelegateCommand<SfDataGrid>((SfDataGrid grid) =>
            {
                if (!IsCalculed) return;
                var dlg = new SaveFileDialog
                {
                    FileName = "Ceb", // Default file name
                    DefaultExt = ".xlsx", // Default file extension
                    Filter = "Classeurs xlsx |*.xlsx" // Filter files by extension
                };
                if (dlg.ShowDialog(Application.Current.MainWindow) == false) return;
                var options = new ExcelExportingOptions
                {
                    ExcelVersion = ExcelVersion.Excel2016,
                    StartRowIndex = 6,
                    StartColumnIndex = 1
                };

                using var excelEngine = new ExcelEngine();
                IWorkbook workBook = excelEngine.Excel.Workbooks.Create();
                var ws = workBook.Worksheets[0];

                for (var i = 1; i <= 6; i++)
                {
                    ws.Range[1, i].Value2 = $"Plaque {i}";
                    ws.Range[2, i].Value2 = Plaques[i - 1];
                }

                ws.Range[1, 7].Value2 = "Recherche";
                ws.Range[2, 7].Value2 = Search;
                ws.ListObjects.Create("Data", ws["A1:G2"])
                    .BuiltInTableStyle = TableBuiltInStyles.TableStyleDark1;

                grid.ExportToExcel(grid.View, options, ws);
                var ls = ws.ListObjects.Create("Solutions", ws[$"A6:E{grid.View.Records.Count + 6}"])
                    .BuiltInTableStyle = TableBuiltInStyles.TableStyleDark1;
                ws.UsedRange.AutofitColumns();
                ws.UsedRange.AutofitRows();
                ws.Range[4, 1].Value2 = Result;

                try
                {
                    workBook.SaveAs(dlg.FileName);
                    System.Diagnostics.Process.Start(dlg.FileName);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Enregistrement impossible");
                }
            });

            stopwatch = new Stopwatch();
            dateDispatcher = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(10)
            };
            dateDispatcher.Tick += (sender, e) =>
            {
                if (stopwatch.IsRunning)
                {
                    Duree = stopwatch.Elapsed.ToString();
                }
                Titre = $"Le compte est bon - {DateTime.Now:dddd dd MMMM yyyy à HH:mm:ss}";
            };
            notifyTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(10)
            };
            notifyTimer.Tick += (sender, e) =>
            {
                NotifyVisibility = Visibility.Hidden;
                notifyTimer.Stop();
            };

            NotifyCommand = new DelegateCommand<SfDataGrid>(sf =>
            {
                if (sf.SelectedIndex < 0) return;
                Solution = Tirage.Solutions[sf.SelectedIndex].ToString();
                ShowNotify();
            });
            Plaques.CollectionChanged += (sender, e) =>
            {
                if (e.Action != NotifyCollectionChangedAction.Replace) return;
                var i = e.NewStartingIndex;
                Tirage.Plaques[i].Text = Plaques[i];
                ClearData();
            };
            UpdateData();
            UpdateColors();
            Titre = $"Le compte est bon - {DateTime.Now:dddd dd MMMM yyyy à HH:mm:ss}";
            dateDispatcher.Start();
        }

        private void UpdateColors()
        {
            switch (Tirage.Status)
            {
                case CebStatus.Valid:
                    (Background, Foreground) = (Colors.LightGreen, Colors.White);
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
                    (Background, Foreground) = (Colors.Yellow, Colors.White);
                    break;
            }
        }

        private void NotifiedChanged([CallerMemberName] string propertyName = "")
        {
            RaisePropertyChanged(propertyName);
        }

        private void ClearData()
        {
            stopwatch.Reset();
            Duree = stopwatch.Elapsed.ToString();
            Solutions.Clear();
            IsCalculed = false;
            Solution = "";
            Result = Tirage.Status != CebStatus.Erreur ? "Résoudre" : "Tirage incorrect";
            NotifyVisibility = Visibility.Hidden;
            UpdateColors();
        }

        private void UpdateData()
        {
            lock (Plaques)
            {
                for (var i = 0; i < Tirage.Plaques.Length; i++)
                    Plaques[i] = Tirage.Plaques[i].Text;
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
            (Background, Foreground) = (Colors.Green, Colors.White);
            stopwatch.Start();
            await Tirage.ResolveAsync();

            Result = Tirage.Status == CebStatus.CompteEstBon
                ? "Le Compte est bon"
                : (Tirage.Status == CebStatus.CompteApproche ?
                $"Compte approché: {Tirage.Found}, écart: {Tirage.Diff}" : "Tirage incorrect");

            IsCalculed = (Tirage.Status == CebStatus.CompteEstBon || Tirage.Status == CebStatus.CompteApproche);
            foreach (var s in Tirage.Solutions)
                Solutions.Add(s.ToCebDetail());
            stopwatch.Stop();
            Duree = stopwatch.Elapsed.ToString();
            Solution = Tirage.Solution.ToString();
            UpdateColors();
            IsBusy = false;
            Solution = Tirage.Solution.ToString();
            ShowNotify();
            return Tirage.Status;
        }

        #endregion Action

        public void ShowNotify()
        {
            if (NotifyVisibility == Visibility.Visible)
                return;
            NotifyVisibility = Visibility.Visible;
            notifyTimer.Start();
        }

    }
}