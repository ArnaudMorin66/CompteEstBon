#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using CompteEstBon.Properties;
using Microsoft.Win32;
// using Syncfusion.XlsIO;
// using Microsoft.Office.Interop.Excel;
using Excel = Microsoft.Office.Interop.Excel;
#endregion

namespace CompteEstBon
{
    public class ViewTirage : INotifyPropertyChanged, ICommand
    {
        private readonly Stopwatch NotifyWatch = new Stopwatch();
        private readonly TimeSpan SolutionTimer = TimeSpan.FromSeconds(Settings.Default.SolutionTimer);
        private Storyboard _animation;
        private Color _background = Colors.Navy;
        private string _duree;

        private Color _foreground = Colors.White;
        private bool _isUpdating;
        private bool _isBusy;
        private bool _isComputed;
        private int _notifyHeight;

        private string _result = "Résoudre";

        public string _solution;
        private string _titre = "Le compte est bon";

        private Visibility _computing = Visibility.Collapsed;
        // public DelegateCommand<string> CebCommand { get; set; }

        public DispatcherTimer dateDispatcher;
        public Stopwatch stopwatch;

        /// <summary>
        ///     Initialisation
        /// </summary>
        /// <returns>
        /// </returns>
        public ViewTirage()
        {
            stopwatch = new Stopwatch();

            dateDispatcher = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(10)
            };
            dateDispatcher.Tick += (sender, e) =>
            {
                if (stopwatch.IsRunning) Duree = stopwatch.Elapsed.ToString();
                if (NotifyHeight != 0 && NotifyWatch.Elapsed > SolutionTimer) HideNotify();
                Titre = $"Le compte est bon - {DateTime.Now:dddd dd MMMM yyyy à HH:mm:ss}";
            };

            Plaques.CollectionChanged += (sender, e) =>
            {
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

        public Storyboard Animation =>
            _animation ??
            (_animation = Application.Current.MainWindow.Resources["AnimationResult"] as Storyboard);

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
                Computing = _isBusy ? Visibility.Visible : Visibility.Collapsed;
                NotifiedChanged();
            }
        }

        public bool IsComputed {
            get => _isComputed;
            set {
                if (value == _isComputed) return;
                _isComputed = value;
                NotifiedChanged();
            }
        }

        public Visibility Computing {
            get => _computing;
            set {
                _computing = value;
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

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            switch ((parameter as string).ToLower())
            {
                case "random":
                    await RandomAsync();
                    break;
                case "resolve":
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
                    break;
                case "export":
                    ExportData();
                    break;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public static Excel.Application GetExcelApplication()
        {
            Excel.Application xl;
            try
            {
                xl = (Excel.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Excel.Application");
            }
            catch (Exception)
            {
                xl = new Excel.Application();
            }
            return xl;
        }
        private void ExportData()
        {
            Excel.Application xl = ViewTirage.GetExcelApplication();
            xl.Visible = false;
            
            var workBook = xl.Workbooks.Add();
            
            var ws = workBook.Worksheets[1];
            for (var i = 1; i <= 6; i++)
            {
                ws.Cells(1, i).Value = $"Plaque {i}";
                ws.Cells(2, i).Value = Plaques[i - 1];
            }
            ws.Cells(1, 7).Value = "Recherche";
            ws.Cells(2, 7).Value = Search;
            ws.ListObjects.Add(Excel.XlListObjectSourceType.xlSrcRange, ws.Range["A1"].CurrentRegion, null, Excel.XlYesNoGuess.xlYes);

            for (var c = 1; c < 6; c++)
            {
                ws.Cells(6, c).Value = $"Opération {c}";
            }
            var l = 7;
            
            foreach(var s in Tirage.Solutions)
            {
                var op = s.Operations.ToArray();
                ws.Range[ws.Cells(l,1), ws.Cells(l,op.Length)].Value = op;
                l++;
            }
            ws.ListObjects.Add(Excel.XlListObjectSourceType.xlSrcRange, ws.Range["A6"].CurrentRegion, null, Excel.XlYesNoGuess.xlYes);
            ws.Range["A4"].Value = Result;
            if (Tirage.Status == CebStatus.CompteEstBon)
            {
                ws.Range["A4"].Interior.ThemeColor = Excel.XlThemeColor.xlThemeColorAccent6;
                ws.Range["A4"].Font.ThemeColor = Excel.XlThemeColor.xlThemeColorDark1;
                ws.Range["A4"].Font.Bold = true;
            }
            else
            {
                ws.Range["A4"].Interior.ThemeColor = Excel.XlThemeColor.xlThemeColorAccent4;
                ws.Range["A4"].Font.ThemeColor = Excel.XlThemeColor.xlThemeColorLight1;
                ws.Range["A4"].Font.Bold = true;
            }
            ws.Range["A4:E4"].MergeCells = true;
            ws.Range["A4:E4"].HorizontalAlignment = Excel.Constants.xlCenter;
            xl.Visible = true;
            
        }

        private void UpdateColors()
        {
            switch (Tirage.Status)
            {
                case CebStatus.Valid:
                    (Background, Foreground) = (Colors.DarkOliveGreen, Colors.White);
                    break;

                case CebStatus.Erreur:
                    (Background, Foreground) = (Colors.Red, Colors.White);
                    break;

                case CebStatus.CompteEstBon:
                    (Background, Foreground) = (Colors.LightGreen, Colors.Yellow);
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
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void ClearData()
        {
            if (_isUpdating) return;
            stopwatch.Reset();
            Animation?.Stop();
            Duree = stopwatch.Elapsed.ToString();
            IsComputed = false;
            Solutions.Clear();
            Solution = "";
            Result = Tirage.Status != CebStatus.Erreur ? "(...)" : "Tirage incorrect";
            HideNotify();
            UpdateColors();
        }

        private void UpdateData()
        {
            if (_isUpdating) return;
            _isUpdating = true;

            for (var i = 0; i < Tirage.Plaques.Length; i++)
                Plaques[i] = Tirage.Plaques[i].Text;

            _isUpdating = false;
            ClearData();

            NotifiedChanged("Search");
        }

        public void ShowNotify(int index = 0)
        {
            if (index >= 0 && index < Solutions.Count)
            {
                Solution = Tirage.Solutions.ElementAt(index).ToString();
                NotifyHeight = 76;
            }
        }

        private void HideNotify()
        {
            NotifyHeight = 0;
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
                : Tirage.Status == CebStatus.CompteApproche
                    ? $"Compte approché: {Tirage.Found}, écart: {Tirage.Diff}"
                    : "Tirage incorrect";

            foreach (var s in Tirage.Solutions)
                Solutions.Add(s.ToCebDetail);
            stopwatch.Stop();
            Duree = stopwatch.Elapsed.ToString();
            Solution = Tirage.Solution.ToString();
            UpdateColors();
            IsBusy = false;
            Solution = Tirage.Solution.ToString();
            IsComputed = Tirage.Status == CebStatus.CompteEstBon || Tirage.Status == CebStatus.CompteApproche;
            ShowNotify();
            return Tirage.Status;
        }

        #endregion Action
    }
}