﻿#region

using Microsoft.Win32;
//using Syncfusion.Drawing;
using Syncfusion.Windows.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

#endregion

// ReSharper disable once CheckNamespace
namespace CompteEstBon {

    public class ViewTirage : NotificationObject, ICommand {
        private readonly Stopwatch NotifyWatch = new();
        private readonly TimeSpan SolutionTimer = TimeSpan.FromSeconds(Properties.Settings.Default.SolutionTimer);

        private TimeSpan _duree;

        private Color _foreground = Colors.White;
        private bool _isUpdating;
        private bool _isBusy;
        private bool _popup;

        private string _result = "Résoudre";

        public CebBase _solution;
        private string _titre = "Jeu du Compte Est Bon";
        public DispatcherTimer dateDispatcher;
        public Stopwatch stopwatch;

        /// <summary>
        ///     Initialisation
        /// </summary>
        /// <returns>
        /// </returns>
        public ViewTirage() {

            stopwatch = Tirage.Watch; // new Stopwatch();


            dateDispatcher = new DispatcherTimer {
                Interval = TimeSpan.FromMilliseconds(Properties.Settings.Default.SolutionTimer)
            };
            dateDispatcher.Tick += (_, _) => {
                if (stopwatch.IsRunning) Duree = stopwatch.Elapsed;

                if (Popup && NotifyWatch.Elapsed > SolutionTimer) Popup = false;
                Titre = $"{Result} - {DateTime.Now:dddd dd MMMM yyyy à HH:mm:ss}";
            };

            Plaques.CollectionChanged += (_, e) => {
                if (e.Action != NotifyCollectionChangedAction.Replace) {
                    return;
                }

                var i = e.NewStartingIndex;
                Tirage.Plaques[i].Value = Plaques[i];
                ClearData();
            };
            _isUpdating = false;
            UpdateData();
            Titre = $"{Result} - {DateTime.Now:dddd dd MMMM yyyy à HH:mm:ss}";
            dateDispatcher.Start();
        }

        public static IEnumerable<int> ListePlaques => CebPlaque.AnyPlaques;

        public CebTirage Tirage { get; } = new CebTirage();

        public ObservableCollection<int> Plaques { get; } = new ObservableCollection<int> { 0, 0, 0, 0, 0, 0 };

        public IEnumerable<CebBase> _solutions;
        public IEnumerable<CebBase> Solutions {
            get => _solutions;
            set {
                _solutions = value;
                RaisePropertyChanged(nameof(Solutions));

                Count = _solutions?.Count() ?? 0;
            }
        }
        public TimeSpan Duree {
            get => _duree;
            set {
                _duree = value;
                RaisePropertyChanged(nameof(Duree));
            }
        }
        private bool _vertical;
        public bool Vertical {
            get => _vertical;
            set {
                _vertical = value;
                ModeView = value ? '⁞' : '…';
                RaisePropertyChanged(nameof(Vertical));
            }
        }
        // ⁞…
        private char _modeView = '…';
        public char ModeView {
            get => _modeView;
            set {
                _modeView = value;
                RaisePropertyChanged(nameof(ModeView));
            }
        }
        public CebBase Solution {
            get => _solution;
            set {
                _solution = value;
                RaisePropertyChanged(nameof(Solution));
            }
        }

        public int Search {
            get => Tirage.Search;
            set {
                Tirage.Search = value;
                RaisePropertyChanged(nameof(Search));
                ClearData();
            }
        }

        public Color Foreground {
            get => _foreground;
            set {
                _foreground = value;
                RaisePropertyChanged(nameof(Foreground));
            }
        }
        public string Result {
            get => _result;
            set {
                if (value == _result) return;
                _result = value;
                RaisePropertyChanged(nameof(Result));
            }
        }

        public bool IsBusy {
            get => _isBusy;
            set {
                _isBusy = value;
                RaisePropertyChanged(nameof(IsBusy));
            }
        }


        public bool IsComputed {
            get => Tirage.Status is CebStatus.CompteEstBon or CebStatus.CompteApproche;
            // ReSharper disable once ValueParameterNotUsed
            set => RaisePropertyChanged(nameof(IsComputed));
        }

        private int _count;

        public int Count {
            get => _count;
            set {
                if (_count != value) {
                    _count = value;
                    RaisePropertyChanged(nameof(Count));
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
                RaisePropertyChanged(nameof(Popup));
            }
        }

        public string Titre {
            get => _titre;
            set {
                _titre = value;
                RaisePropertyChanged(nameof(Titre));
            }
        }

        public event EventHandler CanExecuteChanged {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter) => true;

        public async void Execute(object parameter) {
            var cmd = (parameter as string)?.ToLower();
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
                case "export":
                    if (Count != 0) {
                        await ExportAsync();
                    }
                    break;
            }
        }

        private async Task ExportAsync() {
            IsBusy = true;
            await Task.Run(() => ExportFichier());
            IsBusy = false;
        }

        private void ClearData() {
            if (_isUpdating) return;
            stopwatch.Reset();
            Duree = stopwatch.Elapsed;
            IsComputed = false;
            Solution = null;
            Solutions = null;
            UpdateForeground();
            Result = Tirage.Status != CebStatus.Erreur ? "Jeu du Compte Est Bon" : "Tirage incorrect";
            Popup = false;
        }

        private void UpdateData() {
            if (_isUpdating) return;
            _isUpdating = true;

            for (var i = 0; i < Tirage.Plaques.Count; i++)
                Plaques[i] = Tirage.Plaques[i].Value;

            _isUpdating = false;
            ClearData();

            RaisePropertyChanged(nameof(Search));
        }

        private void UpdateForeground() {
            Foreground = Tirage.Status switch {
                CebStatus.Indefini => Colors.Blue,
                CebStatus.Valid => Colors.White,
                CebStatus.EnCours => Colors.Aqua,
                CebStatus.CompteEstBon => Colors.ForestGreen,
                CebStatus.CompteApproche => Colors.Orange,
                CebStatus.Erreur => Colors.Red,
                _ => throw new NotImplementedException()
            };
        }
        public void ShowPopup(int index = 0) {
            if (index >= 0 && index < Tirage.Solutions.Count) {
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
        public async Task<CebStatus> ResolveAsync() {
            IsBusy = true;
            Result = "⏰ Calcul en cours...";
            // stopwatch.Start();
            Foreground = Colors.Aqua;

            await Tirage.ResolveAsync();
            Result = Tirage.Status switch {
                CebStatus.CompteEstBon => "😊 Compte est Bon",
                CebStatus.CompteApproche => $"😢 Compte approché: {Tirage.Found}, écart: {Tirage.Diff}",
                CebStatus.Erreur => $"🤬 Tirage incorrect",
                _ => "Jeu du Compte Est Bon"
            };

            Duree = Tirage.Watch.Elapsed;
            Solution = Tirage.Solutions[0];
            Solutions = Tirage.Solutions;
            UpdateForeground();
            IsBusy = false;
            RaisePropertyChanged(nameof(IsComputed));

            ShowPopup();
            return Tirage.Status;
        }

        #endregion Action
        public static (bool Ok, string Path) SaveFileName() {
            var dialog = new SaveFileDialog();
            (dialog.Filter, dialog.Title) = ("Excel (*.xlsx)| *.xlsx | Word (*.docx) | *.docx", "Export Excel-Word");
            // ReSharper disable once PossibleInvalidOperationException
            return ((bool)dialog.ShowDialog(), dialog.FileName);

        }

        private void ExportFichier() {
            var (Ok, Path) = SaveFileName();
            if (Ok) {
                FileInfo fi = new FileInfo(Path);
                if (fi.Exists)
                    fi.Delete();

                Action<Stream> ExportFunction = fi.Extension switch {
                    ".xlsx" => Tirage.ExportExcel,
                    ".docx" => Tirage.ExportWord,
                    _ => throw new NotImplementedException(),
                };
                var stream = new FileStream(Path!, FileMode.CreateNew);
                ExportFunction(stream);
                stream.Flush();
                stream.Close();
                OpenDocument(Path);
            }
        }


        public static void OpenDocument(string nom) => Process.Start(new ProcessStartInfo {
            UseShellExecute = true,
            FileName = nom
        });
    }

}