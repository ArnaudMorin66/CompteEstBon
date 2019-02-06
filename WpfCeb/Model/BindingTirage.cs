#region

using CompteEstBon;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Converter;
using Syncfusion.Windows.Shared;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;

#endregion

namespace WpfCeb.Model {

    public class BindingTirage : NotificationObject { // INotifyPropertyChanged {
        private Brush _background;
        private double _duree;

        private Brush _foreground = new SolidColorBrush(Colors.White);

        private bool _isBusy;

        private bool _isCalculed = false;
        private DateTimeOffset _time;

        private string _result = "Résoudre";

        private Visibility _visibility = Visibility.Collapsed;
        public DelegateCommand<object> HasardCommand { get; set; }
        public DelegateCommand<object> ResolveCommand { get; set; }
        public DelegateCommand<SfDataGrid> ExportCommand { get; set; }

        public DispatcherTimer Dispatcher;
        public DispatcherTimer dateDispatcher;

        public CebTirage Tirage { get; } = new CebTirage();

        public ObservableCollection<int> Plaques { get; } = new ObservableCollection<int> { -1, -1, -1, -1, -1, -1 };

        public IEnumerable<int> ListePlaques { get; } = CebPlaque.ListePlaques.Distinct();

        // public ObservableCollection<CebDetail> Solutions { get; } = new ObservableCollection<CebDetail>();
        public ObservableCollection<List<string>> Solutions { get; } = new ObservableCollection<List<string>>();

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


        // public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Initialisation
        /// </summary>
        /// <returns></returns>
        public BindingTirage() {
            HasardCommand = new DelegateCommand<object>(cmdhasard);
            ResolveCommand = new DelegateCommand<object>(cmdResolve);
            ExportCommand = new DelegateCommand<SfDataGrid>(cmdExport);
            _background = new SolidColorBrush(Colors.Navy);
            Dispatcher = new DispatcherTimer {
                Interval = new TimeSpan(100)

            };
            Dispatcher.Tick += (sender, e) => {
                Duree = (DateTimeOffset.Now - _time).TotalSeconds;
            };
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

        private void cmdExport(SfDataGrid grid) {
            var dlg = new SaveFileDialog {
                FileName = "Ceb", // Default file name
                DefaultExt = ".xlsx", // Default file extension
                Filter = "Classeurs Excel (.xlsx)|*.xlsx" // Filter files by extension
            };
            if (dlg.ShowDialog() == false) return;
            var options = new ExcelExportingOptions {
                ExcelVersion = ExcelVersion.Excel2016
            };

            ExcelEngine excelEngine = new ExcelEngine();
            IWorkbook workBook = excelEngine.Excel.Workbooks.Create();
            var ws = workBook.Worksheets[0];
            grid.ExportToExcel(grid.View, options, ws);            
            ws.ListObjects.Create("Table1", ws[$"A1:E{grid.View.Records.Count + 1}"])
                .BuiltInTableStyle = TableBuiltInStyles.TableStyleMedium1;
            ws.InsertRow(1, 3);
            ws.Range["A1"].Value = "Plaques:";
            for(var i=0; i < 6; i++) {
                ws.Range[1, i + 2].Value2 = Plaques[i]; 
            }
            ws.Range["A2"].Value2 = "Cherche:";
            ws.Range["B2"].Value2 = Search;
            ws.Range["A3"].Value2 = Result;

            try {
                workBook.SaveAs(dlg.FileName);
                System.Diagnostics.Process.Start(dlg.FileName);
            } catch (Exception e) {
                MessageBox.Show(e.Message, "Enregistrement impossible");

            }
        }

        private async void cmdResolve(object _) {
            switch (Tirage.Status) {
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
        }

        private async void cmdhasard(object _) {
            await RandomAsync();
        }

        

        private void UpdateColors() {
            switch (Tirage.Status) {
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

        //private void NotifiedChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        private void NotifiedChanged([CallerMemberName] string propertyName = "") {
            RaisePropertyChanged(propertyName);
        }
        private void ClearData() {
            Duree = 0;
            FirstSolutionString = "";
            Solutions.Clear();
            IsCalculed = false;

            if (Tirage.Status != CebStatus.Erreur) {
                Result = "Résoudre";
            } else {
                Result = "Tirage incorrect";
            }
            UpdateColors();
        }

        private void UpdateData() {
            lock (Plaques) {
                for (var i = 0; i < Tirage.Plaques.Length; i++)
                    Plaques[i] = Tirage.Plaques[i];
            }
            NotifiedChanged("Search");
        }

        #region Action

        public async Task ClearAsync() {
            await Tirage.ClearAsync();
            ClearData();
        }

        public async Task RandomAsync() {
            await Tirage.RandomAsync();
            FirstSolutionString = "";
            UpdateData();
        }

        public async Task<CebStatus> ResolveAsync() {
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
            foreach (var s in Tirage.Solutions)
                Solutions.Add(s.Operations);
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
        public void SetBrush(Color background, Color foreground) {
            LinearGradientBrush myLinearGradientBrush =
                new LinearGradientBrush {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(0, 1)
                };

            myLinearGradientBrush.GradientStops.Add(new GradientStop {
                Color = background,
                Offset = 0.0
            });

            myLinearGradientBrush.GradientStops.Add(new GradientStop {
                Color = Colors.Black,
                Offset = 1.0
            });
            Background = myLinearGradientBrush;
            Foreground = new SolidColorBrush(foreground);
        }
    }
}