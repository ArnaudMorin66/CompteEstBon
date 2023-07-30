//-----------------------------------------------------------------------
// <copyright file="ViewTirage.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#region Using
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

using arnaud.morin.outils;

using CompteEstBon.Properties;

using Microsoft.Win32;

using Syncfusion.Windows.Shared;

#endregion
// ReSharper disable EnforceIfStatementBraces
namespace CompteEstBon.ViewModel {
    public class ViewTirage : NotificationObject, ICommand {
        private readonly Stopwatch _notifyWatch = new();
        private readonly TimeSpan _solutionTimer = TimeSpan.FromSeconds(Settings.Default.SolutionTimer);

        private bool _auto;
        private Color _background = Color.FromRgb(22, 22, 22);

        private Color _foreground = Colors.White;

        private bool _isBusy;

        // ⁞…
        private char _modeView = '⋯';

        private bool _mongodb;
        private bool _popup;

        private string _result = "Résoudre";

        private CebBase _solution;

        private string _titre = "Jeu du Compte Est Bon";

        private bool _vertical;
        public DispatcherTimer DateDispatcher;

        /// <summary>
        /// Initialisation
        /// </summary>
        /// <returns>
        ///
        /// </returns>
        public ViewTirage() {
            DateDispatcher =
                new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(Settings.Default.SolutionTimer) };
            DateDispatcher.Tick += (_, _) => {
                if (Popup && _notifyWatch.Elapsed > _solutionTimer)
                    Popup = false;

                Titre = $"{Result} - {DateTime.Now:dddd dd MMMM yyyy à HH:mm:ss}";
            };

            Plaques.CollectionChanged += async (_, e) => {
                if (e.Action != NotifyCollectionChangedAction.Replace || IsBusy)
                    return;

                var i = e.NewStartingIndex;
                Tirage.Plaques[i].Value = Plaques[i];
                await ClearAsync();

                if (Auto)
                    await ResolveAsync();
            };
            Auto = Settings.Default.AutoCalcul;
            MongoDb = Settings.Default.MongoDB;
            UpdateData();
            Titre = $"{Result} - {DateTime.Now:dddd dd MMMM yyyy à HH:mm:ss}";
            DateDispatcher.Start();
        }

        public static string DotnetVersion => $"Version: {Assembly.GetExecutingAssembly().GetName().Version}, {RuntimeInformation.FrameworkDescription}";

        public Color Background {
            get => _background;
            set {
                _background = value;
                RaisePropertyChanged();
            }
        }

        public static IEnumerable<int> ListePlaques => CebPlaque.DistinctPlaques;

        public CebTirage Tirage { get; } = new();

        public ObservableCollection<int> Plaques { get; } = new() { 0, 0, 0, 0, 0, 0 };

        public IEnumerable<CebBase> Solutions {
            get => Tirage.Solutions;
            // ReSharper disable once ValueParameterNotUsed
            set => RaisePropertyChanged(nameof(Solutions), nameof(Count));
        }

        public TimeSpan Duree {
            get => Tirage.Duree;
            // ReSharper disable once ValueParameterNotUsed
            set => RaisePropertyChanged(nameof(Duree));
        }

        public bool Vertical {
            get => _vertical;
            set {
                _vertical = value;
                ModeView = value ? '⁝' : '⋯';
                RaisePropertyChanged(nameof(Vertical));
            }
        }

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
                if (Tirage.Search == value)
                    return;

                Tirage.Search = value;
                RaisePropertyChanged(nameof(Search));
                ClearData();
                if (Auto && Tirage.Status == CebStatus.Valide)
                    Task.Run(ResolveAsync);
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
                if (value == _result)
                    return;

                _result = value;
                RaisePropertyChanged(nameof(Result));
            }
        }

        public bool IsBusy {
            get => _isBusy;
            set {
                if (_isBusy == value)
                    return;

                _isBusy = value;
                RaisePropertyChanged(nameof(IsBusy));
            }
        }

        public bool Auto {
            get => _auto;
            set {
                if (_auto == value)
                    return;

                _auto = value;
                RaisePropertyChanged(nameof(Auto));
                if (Tirage.Status == CebStatus.Valide && Auto)
                    Task.Run(ResolveAsync);
            }
        }

        public bool MongoDb {
            get => _mongodb;
            set {
                if (_mongodb == value)
                    return;

                _mongodb = value;
                RaisePropertyChanged(nameof(MongoDb));
            }
        }

        public bool IsComputed {
            get => Tirage.Status is CebStatus.CompteEstBon or CebStatus.CompteApproche;
            // ReSharper disable once ValueParameterNotUsed
            set => RaisePropertyChanged(nameof(IsComputed));
        }

        public int Count {
            get => Tirage.Count;
            // ReSharper disable once ValueParameterNotUsed
            set => RaisePropertyChanged(nameof(Count));
        }

        public bool Popup {
            get => _popup;
            set {
                if (_popup == value)
                    return;

                _popup = value;
                _notifyWatch.Stop();
                _notifyWatch.Reset();
                if (_popup)
                    _notifyWatch.Start();

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
                        case CebStatus.Valide:
                            await ResolveAsync();
                            break;

                        case CebStatus.CompteEstBon or CebStatus.CompteApproche:
                            await ClearAsync();
                            break;

                        case CebStatus.Invalide:
                            await RandomAsync();
                            break;
                    }
                    break;

                case "export":
                    if (Count != 0)
                        await Task.Run(ExportFichier);

                    break;
            }
        }

        private void ClearData() {
            Solution = null;
            UpdateForeground();
            Result = Tirage.Status != CebStatus.Invalide ? "Jeu du Compte Est Bon" : "Tirage invalide";
            Popup = false;
            RaisePropertyChanged(nameof(IsComputed), nameof(Duree), nameof(Solutions), nameof(Duree));
        }

        private void UpdateData() {
            IsBusy = true;
            lock (Plaques) {
                foreach (var (p, i) in Tirage.Plaques.Indexed()) Plaques[i] = p.Value;
            }

            RaisePropertyChanged(nameof(Search));
            IsBusy = false;
            ClearData();
        }

        private void UpdateForeground() => Foreground =
            Tirage.Status switch
            {
                CebStatus.Indefini => Colors.Blue,
                CebStatus.Valide => Colors.White,
                CebStatus.EnCours => Colors.Aqua,
                CebStatus.CompteEstBon => Colors.SpringGreen,
                CebStatus.CompteApproche => Colors.Orange,
                CebStatus.Invalide => Colors.Red,
                _ => throw new NotImplementedException()
            };

        public void ShowPopup(int index = 0) {
            if (index < 0)
                return;
            Solution = Tirage.Solutions![index];
            Popup = true;
        }

        public static (bool Ok, string Path) SaveFileName() {
            var dialog = new SaveFileDialog
            {
                Title = "Exporter vers...",
                Filter =
                    "Excel (*.xlsx)| *.xlsx | Word (*.docx) | *.docx | Json (*.json) | *.json | XML (*.xml) | *.xml ",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                DefaultExt = ".xlsx",
                FileName = "*.xlsx"
            };
            // ReSharper disable once PossibleInvalidOperationException
            return ((bool)dialog.ShowDialog(), dialog.FileName);
        }

        private void ExportFichier() {
            var (ok, path) = SaveFileName();
            if (!ok)
                return;

            IsBusy = true;
            if (Tirage.Export(new FileInfo(path)))
                path.OpenDocument();
            IsBusy = false;
        }

        #region Action
        public async ValueTask ClearAsync() {
            var old = IsBusy;
            await Tirage.ClearAsync();
            ClearData();
            IsBusy = old;
        }

        public async ValueTask RandomAsync() {
            await Tirage.RandomAsync();
            UpdateData();
            if (Auto)
                await ResolveAsync();
        }

        public async ValueTask<CebStatus> ResolveAsync() {
            if (IsBusy)
                return Tirage.Status;

            IsBusy = true;
            Result = "⏰ Calcul en cours...";
            Foreground = Colors.Aqua;
            await Tirage.ResolveAsync();
            Result = Tirage.Status switch
            {
                CebStatus.CompteEstBon => "😊 Compte est Bon",
                CebStatus.CompteApproche => $"😢 Compte approché: {Tirage.Found}, écart: {Tirage.Ecart}",
                CebStatus.Invalide => "🤬 Tirage invalide",
                _ => "Jeu du Compte Est Bon"
            };

            Solution = Tirage.Solutions![0];

            UpdateForeground();
            IsBusy = false;
            RaisePropertyChanged(nameof(IsComputed), nameof(Duree), nameof(Solutions), nameof(Count));
            ShowPopup();
            if (MongoDb)
                Tirage.SerializeMongo(Settings.Default.MongoServer, "SfWpf");

            return Tirage.Status;
        }
        #endregion Action
    }
}