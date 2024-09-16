//-----------------------------------------------------------------------
// <copyright file="ViewTirage.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#region
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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

using arnaud.morin.outils;

using CompteEstBon.Properties;

using Microsoft.Win32;

#endregion
namespace CompteEstBon.ViewModel {
    public class ViewTirage : INotifyPropertyChanged, ICommand {
        private readonly Stopwatch _notifyWatch = new();
        private readonly TimeSpan _solutionTimer = TimeSpan.FromSeconds(10);

        private bool _auto;
        private Color _background;
        private Color _foreground = Colors.White;
        private bool _isBusy;
        private char _modeView = '…';
        
        private bool _popup;

        private string _result = "Résoudre";


        private CebBase _solution;

        private string _theme = "Sombre";
        private string _titre = "Le compte est bon";
        private bool _vertical;

        public DispatcherTimer DateDispatcher;

        /// <summary>
        /// Initialisation
        /// </summary>
        /// <returns>
        ///
        /// </returns>
        public ViewTirage() {
            DateDispatcher = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(10) };
            DateDispatcher.Tick += (_, _) => {
                if (Popup && _notifyWatch.Elapsed > _solutionTimer)
                    Popup = false;
                Titre = $"{DateTime.Now:dddd dd MMMM yyyy à HH:mm:ss}\t{Result}";
            };
            IsBusy = false;
            

            Background = ThemeColors["Sombre"];
            Auto = Settings.Default.AutoCalcul;
            Tirage.PropertyChanged += (sender, args) => {
                if (args.PropertyName == "Clear") {
                    ClearData();
                    if (Auto)
                        Task.Run(ResolveAsync);
                }
            };
            ClearData();

            Titre = $"{DateTime.Now:dddd dd MMMM yyyy à HH:mm:ss}\t😊 Le compte est bon";
            DateDispatcher.Start();
        }

        public static Dictionary<string, Color> ThemeColors {
            get;
        } = new()
        {
            ["DarkSlateGray"] = Colors.DarkSlateGray,
            ["SlateGray"] = Colors.SlateGray,
            ["Blue"] = Color.FromArgb(0xFF, 0x15, 0x25, 0x49),
            ["Black"] = Colors.Black,
            ["DarkBlue"] = Colors.DarkBlue,
            ["Green"] = Colors.Green,
            ["Red"] = Colors.Red,
            ["Yellow"] = Colors.Yellow,
            ["Navy"] = Colors.Navy,
            ["Sombre"] = Color.FromRgb(40, 40, 40)
        };

        public bool Auto {
            get => _auto;
            set {
                if (_auto == value) {
                    return;
                }

                _auto = value;

                NotifiedChanged();
                if (_auto && Tirage.Status == CebStatus.Valide) {
                    Task.Run(ClearAsync);
                }
            }
        }




        public string Theme {
            get => _theme;
            set {
                if (_theme == value) {
                    return;
                }

                _theme = value;
                Background = ThemeColors[value];
                NotifiedChanged();
            }
        }

        public CebTirage Tirage { get; set; } = new();

        public static IEnumerable<int> ListePlaques { get; } = CebPlaque.DistinctPlaques;
        


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
        

        
        public string Result {
            get => _result;
            set {
                if (value == _result)
                    return;

                _result = value;
                UpdateForeground();
                NotifiedChanged();
            }
        }

        public bool IsComputed => Tirage.Status is CebStatus.CompteEstBon or CebStatus.CompteApproche or CebStatus.Invalide;

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
                if (_isBusy == value) {
                    return;
                }

                _isBusy = value;
                NotifiedChanged();
            }
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

        public bool CanExecute(object parameter) { return true; }

        public async void Execute(object parameter) {
            try {
                var cmd = (parameter as string)?.ToLower();
                switch (cmd) {
                    case "random":
                        await RandomAsync();
                        break;

                    case "resolve": {
                        switch (Tirage.Status) {
                            case CebStatus.Valide:
                                if (IsBusy)
                                    return;

                                await ResolveAsync();
                                break;

                            case CebStatus.CompteEstBon:
                            case CebStatus.CompteApproche:
                                await ClearAsync();
                                break;

                            case CebStatus.Invalide:
                                await RandomAsync();
                                break;
                        }

                        break;
                    }
                    case "export":
                        await ExportAsync();
                        break;
                }
            } catch (Exception e) {
                Console.WriteLine(e);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private async Task ExportAsync() {
            IsBusy = true;
            await Task.Run(ExportFichier);
            IsBusy = false;
        }

        private void UpdateForeground() {
            Foreground = Tirage.Status switch
            {
                CebStatus.Valide => Colors.White,
                CebStatus.Invalide => Colors.Red,
                CebStatus.CompteEstBon => Colors.LawnGreen,
                CebStatus.CompteApproche => Colors.Orange,
                CebStatus.EnCours => Colors.Yellow,
                _ => Colors.Gray
            };
        }

        private void NotifiedChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void NotifiedChanged(params string[] propertiesName) {
            foreach (var s in propertiesName)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(s));
        }

        private void ClearData() {
            Solution = null;
            

            Result = Tirage.Status != CebStatus.Invalide ? "Jeu du Compte Est Bon" : "Tirage invalide";
            Popup = false;
            NotifiedChanged(nameof(IsComputed));
            NotifiedChanged(nameof(Tirage));
        }

        
        

        public void ShowNotify(int index = 0) {
            if (index >= 0 && Tirage.Solutions!.Count != 0 && index < Tirage.Count) {
ShowNotify(Tirage.Solutions.ElementAt(index));

            }
        }

        public void ShowNotify(CebBase sol) {
            Solution = sol;
            Popup = true;
        }
        #region Action
        public async Task ClearAsync() {
            await Tirage.ClearAsync();
            ClearData();
            if (!IsBusy && Auto && Tirage.Status == CebStatus.Valide)
                await ResolveAsync();
        }

        public async Task RandomAsync() {
            await Tirage.RandomAsync();
            ClearData();
            if (Auto && Tirage.Status == CebStatus.Valide)
                await ResolveAsync();
        }


        public async Task ResolveAsync() {
            IsBusy = true;
            Result = "Calcul...";
            await Tirage.ResolveAsync();
            Result = Tirage.Status switch
            {
                CebStatus.CompteEstBon => "😊 Compte est bon",
                CebStatus.CompteApproche => $"😢 Compte approché: {Tirage.Found}, écart: {Tirage.Ecart}",
                CebStatus.Invalide => "Tirage invalide",
                _ => "Le Compte est Bon"
            };

            Solution = Tirage.Solutions![0];
            NotifiedChanged(nameof(Tirage));
            

            IsBusy = false;

            ShowNotify();
            // ReSharper disable once ExplicitCallerInfoArgument
            
            NotifiedChanged(nameof(IsComputed));
        }

        public static (bool select, string name) FileSaveName() {
            SaveFileDialog dialog = new() { DefaultExt = ".xlsx" };
            (dialog.Filter, dialog.Title) = (
                "Document Excel|*.xlsx|Document Word|*.docx| Document Json| *.json| Document XML|*.xml",
                "Export Excel-Word");
            dialog.InitialDirectory = Environment.SpecialFolder.UserProfile + "\\Downloads";
            // ReSharper disable once PossibleInvalidOperationException
            return (bool)dialog.ShowDialog() ? (true, dialog.FileName) : (false, null);
        }

        public void ExportFichier() {
            var (ok, path) = FileSaveName();
            if (!ok)
                return;

            FileInfo fi = new(path);
            Tirage.Export(fi);
            path.OpenDocument();
        }
        #endregion Action
    }
}