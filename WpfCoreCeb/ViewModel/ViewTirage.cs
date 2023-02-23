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
namespace CompteEstBon.ViewModel;

public class ViewTirage : INotifyPropertyChanged, ICommand {
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

    private readonly Stopwatch NotifyWatch = new();
    private readonly TimeSpan SolutionTimer = TimeSpan.FromSeconds(10);

    private bool _auto;
    private Color _background;
    private TimeSpan _duree;
    private Color _foreground = Colors.White;
    private bool _isBusy;
    private char _modeView = '…';

    private bool _mongodb;
    private bool _popup;

    private string _result = "Résoudre";


    public CebBase _solution;

    //private IEnumerable<CebBase> _solutions;
    private IEnumerable<CebBase> _solutions;

    private string _theme = "Sombre";
    private string _titre = "Le compte est bon";
    private bool _vertical;

    public DispatcherTimer dateDispatcher;

    // public Stopwatch stopwatch;

    /// <summary>
    /// Initialisation
    /// </summary>
    /// <returns>
    ///
    /// </returns>
    public ViewTirage() {
        Solutions = null;
        dateDispatcher = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(10) };
        dateDispatcher.Tick += (_, _) => {
            if(Popup && NotifyWatch.Elapsed > SolutionTimer)
                Popup = false;
            Titre = $"{Result} - {DateTime.Now:dddd dd MMMM yyyy à HH:mm:ss}";
        };
        IsBusy = false;
        Plaques.CollectionChanged += (_, e) => {
            if(e.Action != NotifyCollectionChangedAction.Replace || IsBusy)
                return;

            var i = e.NewStartingIndex;
            Tirage.Plaques[i].Value = Plaques[i];
            Task.Run(ClearAsync);
        };

        Background = ThemeColors["Sombre"];
        Auto = Settings.Default.AutoCalcul;
        UpdateData();

        Titre = $"😊 Le compte est bon - {DateTime.Now:dddd dd MMMM yyyy à HH:mm:ss}";
        dateDispatcher.Start();
    }

    public bool Auto {
        get => _auto;
        set {
            if(_auto == value)
                return;
            _auto = value;

            NotifiedChanged();
            if(_auto && Tirage.Status == CebStatus.Valide)
                Task.Run(ClearAsync);
        }
    }

    public bool MongoDb {
        get => _mongodb;
        set {
            if(_mongodb == value)
                return;
            _mongodb = value;
            NotifiedChanged();
        }
    }


    public string Theme {
        get => _theme;
        set {
            if(_theme == value)
                return;
            _theme = value;
            Background = ThemeColors[value];
            NotifiedChanged();
        }
    }

    public CebTirage Tirage { get; set; } = new();

    public static IEnumerable<int> ListePlaques { get; } = CebPlaque.DistinctPlaques;

    public ObservableCollection<int> Plaques {
        get;
    } =
        new() { 0, 0, 0, 0, 0, 0 };

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
        get => Tirage.Search;
        set {
            if(Tirage.Search == value)
                return;
            Tirage.Search = value;
            NotifiedChanged();
            ClearData();
            if(!IsBusy && Auto && Tirage.Status == CebStatus.Valide)
                Task.Run(ResolveAsync);
        }
    }

    public CebStatus Status => Tirage.Status;

    public string Result {
        get => _result;
        set {
            if(value == _result)
                return;
            _result = value;
            NotifiedChanged();
        }
    }

    public bool IsComputed {
        get => Tirage.Status is CebStatus.CompteEstBon or CebStatus.CompteApproche;
        // ReSharper disable once ValueParameterNotUsed
        set => NotifiedChanged();
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
            if(_isBusy == value)
                return;
            _isBusy = value;
            NotifiedChanged();
        }
    }

    public bool Popup {
        get => _popup;
        set {
            if(_popup == value)
                return;
            _popup = value;
            NotifyWatch.Stop();
            NotifyWatch.Reset();
            if(_popup)
                NotifyWatch.Start();
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
        try {
            var cmd = (parameter as string)?.ToLower();
            switch(cmd) {
                case "random":
                    await RandomAsync();
                    break;

                case "resolve": {
                    switch(Tirage.Status) {
                        case CebStatus.Valide:
                            if(IsBusy)
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
        } catch(Exception e) {
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
            CebStatus.CompteEstBon => Colors.LightYellow,
            CebStatus.CompteApproche => Colors.Orange,
            CebStatus.EnCours => Colors.Yellow,
            _ => Colors.White
        };
    }

    private void NotifiedChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(
        this,
        new PropertyChangedEventArgs(propertyName));

    private void ClearData() {
        //var old = IsBusy;

        //IsBusy = true;
        Solution = null;
        Solutions = null;
        Duree = TimeSpan.Zero;

        UpdateForeground();
        Result = Tirage.Status != CebStatus.Invalide ? "Jeu du Compte Est Bon" : "Tirage invalide";
        Popup = false;
        //IsBusy = old;
        NotifiedChanged(nameof(IsComputed));
    }

    private void UpdateData() {
        IsBusy = true;
        foreach (var (p, i) in Tirage.Plaques.Indexed())
            Plaques[i] = p.Value;
        NotifiedChanged(nameof(Search));
        IsBusy = false;
        ClearData();
    }

    public void ShowNotify(int index = 0) {
        if(index >= 0 && Tirage.Solutions!.Any() && index < Tirage.Count) {
            Solution = Tirage.Solutions.ElementAt(index);
            Popup = true;
        }
    }

    #region Action
    public async Task ClearAsync() {
        await Tirage.ClearAsync();
        ClearData();
        if(!IsBusy && Auto && Tirage.Status == CebStatus.Valide)
            await ResolveAsync();
    }

    public async Task RandomAsync() {
        await Tirage.RandomAsync();
        UpdateData();
        if(Auto && Tirage.Status == CebStatus.Valide)
            await ResolveAsync();
    }

    private int _count;

    public int Count {
        get => _count;
        private set {
            if(_count == value)
                return;
            _count = value;
            NotifiedChanged();
        }
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
        Duree = Tirage.Duree;
        UpdateForeground();

        Solution = Tirage.Solutions![0];

        Solutions = Tirage.Solutions; // .Select(CebDetail.FromCebBase);
        if(MongoDb)
            Tirage.SerializeMongo(Settings.Default.MongoServer, "Wpf");
        IsBusy = false;

        ShowNotify();
        // ReSharper disable once ExplicitCallerInfoArgument
        NotifiedChanged(nameof(Status));
        NotifiedChanged(nameof(IsComputed));
    }

    public static (bool select, string name) FileSaveName() {
        SaveFileDialog dialog = new() { DefaultExt = ".xlsx" };
        (dialog.Filter, dialog.Title) = ("Document Excel|*.xlsx|Document Word|*.docx| Document Json| *.json| Document XML|*.xml", "Export Excel-Word");
        dialog.InitialDirectory = Environment.SpecialFolder.UserProfile + "\\Downloads";
        // ReSharper disable once PossibleInvalidOperationException
        return (bool)dialog.ShowDialog() ? (true, dialog.FileName) : (false, null);
    }

    public void ExportFichier() {
        var (Ok, Path) = FileSaveName();
        if(!Ok)
            return;
        FileInfo fi = new(Path);
        if(fi.Exists)
            fi.Delete();
        switch(fi.Extension) {
            case ".xlsx":
                Tirage.SaveXlsx(fi);
                break;
            case ".docx":
                Tirage.SaveDocx(fi);
                break;
            case ".json":
                Tirage.SaveJson(fi);
                break;
            case ".xml":
                Tirage.SaveXml(fi);
                break;
        }
        Outils.OpenDocument(Path);
    }
    #endregion Action
}