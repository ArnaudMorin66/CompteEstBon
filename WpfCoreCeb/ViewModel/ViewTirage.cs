﻿#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;

#endregion

namespace CompteEstBon.ViewModel {

    public class ViewTirage : INotifyPropertyChanged, ICommand {
        private readonly Stopwatch NotifyWatch = new Stopwatch();
        private readonly TimeSpan SolutionTimer = TimeSpan.FromSeconds(10);
        private Color _background;
        private TimeSpan _duree;
        private Color _foreground = Colors.White;
        private bool _isBusy;
        private bool _isUpdating;
        private char _modeView = '…';
        private bool _popup;

        private string _result = "Résoudre";

        private int _search;

        public CebBase _solution;
        private IEnumerable<CebBase> _solutions;
        private string _theme = "Dark";
        private string _titre = "Le compte est bon";
        private bool _vertical;

        public DispatcherTimer dateDispatcher;
        public Stopwatch stopwatch;

        /// <summary>
        ///     Initialisation
        /// </summary>
        /// <returns>
        /// </returns>
        public ViewTirage() {
            Solutions = null;
            stopwatch = new Stopwatch();

            dateDispatcher = new DispatcherTimer {
                Interval = TimeSpan.FromMilliseconds(10)
            };
            dateDispatcher.Tick += (sender, e) => {
                if (stopwatch.IsRunning) Duree = stopwatch.Elapsed;
                if (Popup && NotifyWatch.Elapsed > SolutionTimer)
                    Popup = false;
                Titre = $"Le compte est bon - {DateTime.Now:dddd dd MMMM yyyy à HH:mm:ss}";
            };
            Plaques.CollectionChanged += (sender, e) => {
                var i = e.NewStartingIndex;
                if (Tirage.Plaques[i].Value == Plaques[i]) return;
                Tirage.Plaques[i].Value = Plaques[i];
                ClearData();
            };


            Background = ThemeColors["Dark"];
            _isUpdating = false;
            UpdateData();
            UpdateColors();
            Titre = $"Le compte est bon - {DateTime.Now:dddd dd MMMM yyyy à HH:mm:ss}";
            dateDispatcher.Start();
        }

        //private readonly Storyboard WaitStory =
        //    Application.Current.MainWindow?.FindResource("WaitStoryboard") as Storyboard;
        public static Dictionary<string, Color> ThemeColors { get; } = new Dictionary<string, Color> {
            ["Dark"] = Color.FromArgb(0xFF, 0x13, 0x18, 0x18),
            ["Blue"] = Color.FromArgb(0xFF, 0x15, 0x25, 0x49),
            ["Black"] = Colors.Black,
            ["DarkBlue"] = Colors.DarkBlue,
            ["DarkSlateGray"] = Colors.DarkSlateGray,
            ["Green"] = Colors.Green
        };

        public string Theme {
            get => _theme;
            set {
                if (_theme == value) return;
                _theme = value;
                Background = ThemeColors[value];
                NotifiedChanged();
            }
        }


        public CebTirage Tirage { get; set; } = new CebTirage();
        public static IEnumerable<int> ListePlaques { get; } = CebPlaque.AnyPlaques;

        public ObservableCollection<int> Plaques { get; } =
            new ObservableCollection<int> {0, 0, 0, 0, 0, 0};

        public TimeSpan Duree {
            get => _duree;
            set {
                _duree = value;
                NotifiedChanged();
            }
        }

        public bool Vertical {
            get => _vertical;
            set {
                _vertical = value;
                ModeView = value ? '⁞' : '…';
                NotifiedChanged();
            }
        }

        public char ModeView {
            get => _modeView;
            set {
                _modeView = value;
                NotifiedChanged();
            }
        }

        public CebBase Solution {
            get => _solution;
            set {
                _solution = value;
                NotifiedChanged();
            }
        }

        public IEnumerable<CebBase> Solutions {
            get => _solutions;
            set {
                _solutions = value;

                Count = value == null ? 0 : _solutions.Count();
                NotifiedChanged();
            }
        }

        public int Search {
            get => _search;
            set {
                _search = value;
                NotifiedChanged();
                ClearData();
            }
        }

        public CebStatus Status => Tirage.Status;

        public string Result {
            get => _result;
            set {
                if (value == _result) return;
                _result = value;
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

        public Color Background {
            get => _background;
            set {
                _background = value;
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

        public bool CanExecute(object parameter) {
            return true;
        }

        public async void Execute(object parameter) {
            try {
                var cmd = (parameter as string)?.ToLower();
                switch (cmd) {
                    case "random":
                        await RandomAsync();
                        break;
                    case "resolve": {
                        switch (Tirage.Status) {
                            case CebStatus.Valid:
                                if (IsBusy) return;
                                await ResolveAsync();
                                break;

                            case CebStatus.CompteEstBon:
                            case CebStatus.CompteApproche:
                                await ClearAsync();
                                break;

                            case CebStatus.Erreur:
                                await RandomAsync();
                                break;
                            case CebStatus.Indefini:
                                break;
                            case CebStatus.EnCours:
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        break;
                    }
                    case "xlsx":
                    case "docx":
                        await ExportAsync(cmd);
                        break;
                }
            }
            #pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e) {
                Console.WriteLine(e);
            }
            #pragma warning restore CA1031 // Do not catch general exception types
        }

        public event PropertyChangedEventHandler PropertyChanged;


        private void UpdateColors() {
            Foreground = Tirage.Status switch {
                CebStatus.Valid => Colors.White,
                CebStatus.Erreur => Colors.Red,
                CebStatus.CompteEstBon => Colors.YellowGreen,
                CebStatus.CompteApproche => Colors.Orange,
                CebStatus.EnCours => Colors.Yellow,
                _ => Colors.White
            };
        }

        private void NotifiedChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ClearData() {
            if (_isUpdating) return;
            // ReSharper disable once ExplicitCallerInfoArgument
            NotifiedChanged(nameof(Status));
            stopwatch.Reset();
            Duree = stopwatch.Elapsed;
            Solution = null;

            Solutions = null;

            Result = Tirage.Status != CebStatus.Erreur ? "" : "Tirage incorrect";
            Popup = false;
            UpdateColors();
        }

        private void UpdateData() {
            if (_isUpdating) return;
            _isUpdating = true;

            for (var i = 0; i < Tirage.Plaques.Count; i++)
                Plaques[i] = Tirage.Plaques[i].Value;
            _isUpdating = false;
            Search = Tirage.Search;
            // ClearData();

            // ReSharper disable once ExplicitCallerInfoArgument
        }

        public void ShowNotify(int index = 0) {
            if (index >= 0 && Tirage.Solutions.Any() && index < Tirage.Solutions.Count()) {
                Solution = Tirage.Solutions[index];
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


        public async Task ResolveAsync() {
            IsBusy = true;
            Result = "";

            stopwatch.Start();
            var data = await Tirage.ResolveAsync();
            Result = Tirage.Status switch {
                CebStatus.CompteEstBon => "Le Compte est bon",
                CebStatus.CompteApproche => $"Compte approché: {Tirage.Found}, écart: {Tirage.Diff}",
                _ => "Tirage incorrect"
            };
            stopwatch.Stop();
            Duree = Tirage.Duree;
            UpdateColors();

            Solution = Tirage.Solutions[0];
            Solutions = data.Solutions;
            IsBusy = false;
            ShowNotify();
            // ReSharper disable once ExplicitCallerInfoArgument
            NotifiedChanged(nameof(Status));
        }

        public static (bool select, string name) FileSaveName(string ty) {
            var dialog = new SaveFileDialog();
            (dialog.Filter, dialog.Title) = ty switch {
                "xlsx" => ("Excel (*.xlsx)| *.xlsx", "Fichiers Excel"),
                "docx" => ("Word (*.docx) | *.docx", "Fichiers Word"),
                _ => throw new NotImplementedException()
            };
            dialog.InitialDirectory = Environment.SpecialFolder.UserProfile + "\\Downloads";
            // ReSharper disable once PossibleInvalidOperationException
            return (bool) dialog.ShowDialog() ? (true, dialog.FileName) : (false, null);
        }

        public async Task ExportAsync(string ty) {
            var (select, name) = FileSaveName(ty);
            if (!select) return;
            #pragma warning disable IDE0007 // Utiliser un type implicite
            Action<Stream> export = ty switch
                #pragma warning restore IDE0007 // Utiliser un type implicite
                {
                    "xlsx" => Tirage.ExportExcel,
                    "docx" => Tirage.ExportWord,
                    _ => throw new NotImplementedException()
                };
            using var stream = new FileStream(name, FileMode.Create);
            export(stream);
            await stream.FlushAsync();
            await stream.DisposeAsync();
            OpenDocument(name);
        }

        public static void OpenDocument(string nom) {
            var info = new ProcessStartInfo {
                UseShellExecute = true,
                FileName = nom
            };
            Process.Start(info);
        }

        #endregion Action
    }
}