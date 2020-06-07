#region

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
#endregion

namespace CompteEstBon {
    public class ControlsTitleBar : ObservableCollection<object> { }
    public class ViewTirage : INotifyPropertyChanged, ICommand {
        private readonly Stopwatch NotifyWatch = new Stopwatch();
        private readonly TimeSpan SolutionTimer = TimeSpan.FromSeconds(CompteEstBon.Properties.Settings.Default.SolutionTimer);
        
        private TimeSpan _duree;

        private Color _foreground = Colors.White;
        private bool _isUpdating;
        private bool _isBusy;
        private bool _popup;

        private string _result = "Résoudre";

        public string _solution;
        private string _titre = "Le compte est bon";

        public DispatcherTimer dateDispatcher;
        public Stopwatch stopwatch;

        /// <summary>
        ///     Initialisation
        /// </summary>
        /// <returns>
        /// </returns>
        public ViewTirage() {

            stopwatch = new Stopwatch();
            

            dateDispatcher = new DispatcherTimer {
                Interval = TimeSpan.FromMilliseconds(Properties.Settings.Default.SolutionTimer)
            };
            dateDispatcher.Tick += (sender, e) => {
                if (stopwatch.IsRunning) Duree = stopwatch.Elapsed;

                if (Popup && NotifyWatch.Elapsed > SolutionTimer) Popup = false;
                Titre = $"Le compte est bon - {DateTime.Now:dddd dd MMMM yyyy à HH:mm:ss}";
            };

            Plaques.CollectionChanged += (sender, e) => {
                if (e.Action != NotifyCollectionChangedAction.Replace) return;
                var i = e.NewStartingIndex;
                Tirage.Plaques[i].Value = Plaques[i];
                ClearData();
            };
            _isUpdating = false;
            UpdateData();
            Titre = $"Le compte est bon - {DateTime.Now:dddd dd MMMM yyyy à HH:mm:ss}";
            dateDispatcher.Start();
        }

        public static IEnumerable<int> ListePlaques { get; } = CebPlaque.AnyPlaques;

        public CebTirage Tirage { get; } = new CebTirage();

        public ObservableCollection<int> Plaques { get; } = new ObservableCollection<int> { 0, 0, 0, 0, 0, 0 }; 
            
        public IEnumerable<CebBase> _solutions;
        public IEnumerable<CebBase> Solutions {
            get => _solutions;
            set {
                _solutions = value;
                NotifiedChanged();

                Count = _solutions == null ? 0 : _solutions.Count();
            }
        }
        public TimeSpan Duree {
            get => _duree;
            set {
                _duree = value;
                NotifiedChanged();
            }
        }
        private bool _vertical = false;
        public bool Vertical {
            get => _vertical;
            set {
                _vertical = value;
                ModeView = value ? '⁞' : '…';
                NotifiedChanged();
            }
        }
        // ⁞…
        private char _modeView = '…';
        public char ModeView {
            get => _modeView;
            set {
                _modeView = value;
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

        public Color Foreground {
            get => _foreground;
            set {
                _foreground = value;
                NotifiedChanged();
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

        public bool IsBusy {
            get => _isBusy;
            set {
                _isBusy = value;
                NotifiedChanged();
            }
        }


        public bool IsComputed {
            get => Tirage.Status == CebStatus.CompteEstBon || Tirage.Status == CebStatus.CompteApproche;
            set {
                NotifiedChanged();
            }
        }

        private int _count;

        public int Count {
            get => _count;
            set {
                if (_count != value) {
                    _count = value;
                    NotifiedChanged();
                }
            }
        }
        public bool Popup {
            get => _popup;
            set {
                if (_popup == value) return;
                _popup = value;
                NotifyWatch.Stop();
                NotifyWatch.Reset();
                if (_popup) NotifyWatch.Start();
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

        public bool CanExecute(object parameter) => true;

        public async void Execute(object parameter) {
            var cmd = (parameter as string).ToLower(); 
            switch (cmd) {
                case "random":
                    await RandomAsync();
                    break;
                case "resolve":
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
                    break;
                case "xlsx":
                case "docx":
                    await ExportAsync(cmd);
                    break;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private async Task ExportAsync(string fmt) {
            IsBusy = true;
            await Task.Run(() => ExportFichier(fmt)); 
            IsBusy = false;
        }



        private void NotifiedChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ClearData() {
            if (_isUpdating) return;
            stopwatch.Reset();
            Duree = stopwatch.Elapsed;
            IsComputed = false;
            Solution = "";
            Solutions = null;
            UpdateForeground();
            Result = Tirage.Status != CebStatus.Erreur ? "" : "Tirage incorrect";
            Popup = false;
        }

        private void UpdateData() {
            if (_isUpdating) return;
            _isUpdating = true;

            for (var i = 0; i < Tirage.Plaques.Count; i++)
                Plaques[i] = Tirage.Plaques[i].Value;

            _isUpdating = false;
            ClearData();

            NotifiedChanged(nameof(Search));
        }

        private void UpdateForeground() {
#pragma warning disable CS8509 // L'expression switch ne prend pas en charge toutes les valeurs possibles de son type d'entrée (elle n'est pas exhaustive).
            Foreground = Tirage.Status switch {
#pragma warning restore CS8509 // L'expression switch ne prend pas en charge toutes les valeurs possibles de son type d'entrée (elle n'est pas exhaustive).
                CebStatus.Indefini => Colors.Blue, 
                CebStatus.Valid => Colors.White, 
                CebStatus.EnCours => Colors.Aqua, 
                CebStatus.CompteEstBon => Colors.GreenYellow, 
                CebStatus.CompteApproche => Colors.Orange, 
                CebStatus.Erreur => Colors.Red };
        }
        public void ShowPopup(int index = 0) {
            if (index >= 0 && index < Tirage.Solutions.Count()) {
                Solution = Tirage.Solution(index);
                Popup = true;
            }
        }

        #region Action

        public async Task ClearAsync() {
            await Tirage.ClearAsync();
            ClearData();
        }

        public async Task RandomAsync() {
            await Tirage.RandomAsync();
            UpdateData();
        }
        public async Task<CebStatus> ResolveAsync() {
            IsBusy = true;
            Result = "";
            stopwatch.Start();
            await Tirage.ResolveAsync();
            Result = Tirage.Status == CebStatus.CompteEstBon
                ? "Le Compte est bon"
                : Tirage.Status == CebStatus.CompteApproche
                    ? $"Compte approché: {Tirage.Found}, écart: {Tirage.Diff}"
                    : "Tirage incorrect";

            stopwatch.Stop();
            Duree = stopwatch.Elapsed;
            Solution = Tirage.Solution();
            Solutions = Tirage.Solutions;
            UpdateForeground();
            IsBusy = false;
            NotifiedChanged(nameof(IsComputed));
            
            ShowPopup();
            return Tirage.Status;
        }

        #endregion Action
        public (bool Ok, string Path) FileSaveName(string ext) {
            var dialog = new SaveFileDialog();
            (dialog.Filter, dialog.Title) = ext switch
            {
                "xlsx" => ("Excel (*.xlsx)| *.xlsx", "Fichiers Excel"),
                "docx" => ("Word (*.docx) | *.docx", "Fichiers Word"),
                _ => ("Tous (*.*) | *.*", "Tous les fichiers")
            };
            if ((bool)dialog.ShowDialog()) {
                return (true, dialog.FileName);
            }
            return (false, null);

        }

        private void ExportFichier(string ext) {
            var (Ok, Path) = FileSaveName(ext);
            if (Ok) {
                if (File.Exists(Path)) {
                    File.Delete(Path);
                }
                Action<Stream> ExportFunction = ext switch
                {
                    "xlsx" => Tirage.ExportExcel,
                    "docx" => Tirage.ExportWord,
                    _ => throw new NotImplementedException(),
                };
                var stream = new FileStream(Path, FileMode.CreateNew);
                ExportFunction(stream);
                stream.Flush();
                stream.Close();
                OpenDocument(Path);
            }
        }


        public static void OpenDocument(string nom) {
            var info = new ProcessStartInfo {
                UseShellExecute = true,
                FileName = nom
            };
            Process.Start(info);
        }
    }
    
}