#region

using CompteEstBon.Properties;
using Microsoft.Win32;
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
using System.Windows.Media.Animation;
using System.Windows.Threading;

#endregion

namespace CompteEstBon
{
    public class ViewTirage : NotificationObject
    {
        public static IEnumerable<int> ListePlaques { get; } = CebPlaque.ListePlaques.Distinct();
        private readonly TimeSpan SolutionTimer = TimeSpan.FromSeconds(Settings.Default.SolutionTimer);
        private string _duree;
        public Stopwatch stopwatch;
        private Storyboard _animation = null;

        public Storyboard Animation {
            get {
                if (_animation == null)
                {
                    _animation = Application.Current.MainWindow.Resources["AnimationResult"] as Storyboard;
                }
                return _animation;
            }
        }

        private Color _foreground = Colors.White;
        private Color _background = Colors.Navy;
        private bool _isBusy;

        private string _result = "Résoudre";

        private Visibility _visibility = Visibility.Collapsed;
        private int _notifyHeight = 0;
        public DelegateCommand<object> HasardCommand { get; set; }

        public DelegateCommand<object> ResolveCommand { get; set; }

        public DelegateCommand<object> ExportCommand { get; set; }

        public DelegateCommand<int> NotifyCommand { get; set; }

        public DispatcherTimer dateDispatcher;
        private readonly Stopwatch NotifyWatch = new Stopwatch();

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

        public bool IsCalculed => Tirage.Status == CebStatus.CompteEstBon || Tirage.Status == CebStatus.CompteApproche;

        public Visibility Visibility {
            get => _visibility;
            set {
                _visibility = value;
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
                if (_notifyHeight != 0)
                {
                    NotifyWatch.Start();
                }
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
            ResolveCommand = new DelegateCommand<object>(async (_) =>
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

            ExportCommand = new DelegateCommand<object>(_ =>
            {
                if (!IsCalculed) return;
                var dlg = new SaveFileDialog
                {
                    FileName = "Ceb", // Default file name
                    DefaultExt = ".xlsx", // Default file extension
                    Filter = "Classeurs xlsx|*.xlsx|Fichiers csv|*.csv" // Filter files by extension
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
                ws.ImportData(Solutions, 6, 1, true);
                for (var i = 1; i <= 5; i++)
                    ws.Range[6, i].Value2 = $"Opération {i}";

                var ls = ws.ListObjects.Create("Solutions", ws[$"A6:E{Solutions.Count + 6}"])
                    .BuiltInTableStyle = TableBuiltInStyles.TableStyleDark1;
                ws.UsedRange.AutofitColumns();
                ws.UsedRange.AutofitRows();
                ws.Range[4, 1].Value2 = Result;

                try
                {
                    if (dlg.FileName.EndsWith(".csv"))
                        workBook.SaveAs(dlg.FileName, ";");
                    else 
                        workBook.SaveAs(dlg.FileName);
                    Process.Start(dlg.FileName);
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
                if (NotifyHeight != 0 && NotifyWatch.Elapsed > SolutionTimer)
                {
                    HideNotify();
                }


                Titre = $"Le compte est bon - {DateTime.Now:dddd dd MMMM yyyy à HH:mm:ss}";
            };

            NotifyCommand = new DelegateCommand<int>(index =>
            {
                if (index < 0) return;
                Solution = Tirage.Solutions[index].ToString();
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
            Animation?.Stop();
            Duree = stopwatch.Elapsed.ToString();
            Solutions.Clear();
            Solution = "";
            Result = Tirage.Status != CebStatus.Erreur ? "" : "Tirage incorrect";
            HideNotify();
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
            Animation?.Begin();
            Result = "...Calcul...";
            (Background, Foreground) = (Colors.Green, Colors.White);
            stopwatch.Start();
            await Tirage.ResolveAsync();

            Result = Tirage.Status == CebStatus.CompteEstBon
                ? "Le Compte est bon"
                : (Tirage.Status == CebStatus.CompteApproche ?
                $"Compte approché: {Tirage.Found}, écart: {Tirage.Diff}" : "Tirage incorrect");

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
        private void ShowNotify()
        {
            NotifyHeight = 76;
        }
        private void HideNotify()
        {
            NotifyHeight = 0;
        }

    }
}