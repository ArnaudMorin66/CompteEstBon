//-----------------------------------------------------------------------
// <copyright file="ViewTirage.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#region Using

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Input;

using arnaud.morin.outils;

using CebMaui.Services;

using CompteEstBon;

using Syncfusion.Maui.Themes;

#endregion

#pragma warning disable CS0067
// ReSharper disable EnforceIfStatementBraces
namespace CebMaui.ViewModel;

public class ViewTirage : INotifyPropertyChanged, ICommand {
    public static readonly List<string> ListeFormats = ["Excel", "Word", "Json", "Xml", "HTML"];

    private static readonly Dictionary<string, ExportFile> ListExportFiles = new() {
        ["Excel"] = new ExportFile("xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"),
        ["Word"] = new ExportFile("docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"),
        ["Json"] = new ExportFile("json", "application/json"), ["Xml"] = new ExportFile("xml", "application/xml"),
        ["HTML"] = new ExportFile("html", "text/html")
    };

// private readonly Stopwatch _notifyWatch = new();
    private string _formatExport = ListeFormats[0];

    public string FormatExport {
        get => _formatExport;
        set {
            if (_formatExport == value) return;
            _formatExport = value;
            OnPropertyChanged();
        }
    }

    private bool _auto;
    private Color _background = Color.FromRgb(22, 22, 22);

    private Color _foreground = Colors.White;

    private bool _isBusy;


    private bool _popup;

    private string _result = "Résoudre";

    private CebBase _solution = null!;
    private Timer? _timer;

    private string _titre = "Jeu du Compte Est Bon";

    private bool _vueGrille;


    /// <summary>
    ///     Initialisation
    /// </summary>
    /// <returns>
    /// </returns>
    public ViewTirage() {
        Plaques.CollectionChanged += async (_, e) => {
            if (e is not { Action: NotifyCollectionChangedAction.Replace } || IsBusy)
                return;

            var i = e.NewStartingIndex;
            Tirage.Plaques[i].Value = Plaques[i];
            await ClearAsync();

            if (Auto)
                await ResolveAsync();
        };
        Auto = false;
        ThemeDark = true;
        UpdateData();
        Titre = $"{Result} - {DateTime.Now:dddd dd MMMM yyyy à HH:mm:ss}";
    }

    public static string DotnetVersion =>
        $"Version: {Assembly.GetExecutingAssembly().GetName().Version}, {RuntimeInformation.FrameworkDescription}";

    public Color Background {
        get => _background;
        set {
            _background = value;
            OnPropertyChanged();
        }
    }

// 
    private bool _themeDark = true;

    public bool ThemeDark {
        get => _themeDark;
        set {
            if (_themeDark == value) return;
            _themeDark = value;
            var mergedDictionaries = Application.Current!.Resources.MergedDictionaries;
            var theme = mergedDictionaries?.OfType<SyncfusionThemeResourceDictionary>().FirstOrDefault();
            if (theme == null) return;
            if (_themeDark) {
                theme.VisualTheme = SfVisuals.MaterialDark;
                Application.Current.UserAppTheme = AppTheme.Dark;
            } else {
                theme.VisualTheme = SfVisuals.MaterialLight;
                Application.Current.UserAppTheme = AppTheme.Light;
            }

            UpdateForeground();
            OnPropertyChanged();
        }
    }



    public static IEnumerable<int> ListePlaques => CebPlaque.DistinctPlaques;

    public CebTirage Tirage { get; } = new();

    public ObservableCollection<int> Plaques { get; } = [0, 0, 0, 0, 0, 0];

    public IEnumerable<CebBase> Solutions {
        get => Tirage.Solutions;
        // ReSharper disable once ValueParameterNotUsed
        set => OnPropertyChanged(nameof(Solutions), nameof(Count));
    }


    public TimeSpan Duree {
        get => Tirage.Duree;
        // ReSharper disable once ValueParameterNotUsed
        set => OnPropertyChanged();
    }

    public string Found {
        get => Tirage.Found;
        // ReSharper disable once ValueParameterNotUsed
        set => OnPropertyChanged();
    }

    public bool VueGrille {
        get => _vueGrille;
        set {
            if (_vueGrille == value) return;
            _vueGrille = value;
            OnPropertyChanged();
        }
    }


    public CebBase Solution {
        get => _solution;
        set {
            _solution = value;
            OnPropertyChanged();
        }
    }

    public int Search {
        get => Tirage.Search;
        set {
            if (Tirage.Search == value)
                return;

            Tirage.Search = value;
            OnPropertyChanged();
            ClearData();
            if (Auto && Tirage.Status == CebStatus.Valide)
                Task.Run(ResolveAsync);
        }
    }


    public Color Foreground {
        get => _foreground;
        set {
            _foreground = value;
            OnPropertyChanged();
        }
    }

    public string Result {
        get => _result;
        set {
            if (value == _result)
                return;

            _result = value;
            OnPropertyChanged();
        }
    }

    public bool IsBusy {
        get => _isBusy;
        set {
            if (_isBusy == value)
                return;

            _isBusy = value;
            OnPropertyChanged();
        }
    }

    public bool Auto {
        get => _auto;
        set {
            if (_auto == value)
                return;

            _auto = value;
            OnPropertyChanged();
            if (Tirage.Status == CebStatus.Valide && Auto)
                Task.Run(ResolveAsync);
        }
    }


    public bool IsComputed {
        get => Tirage.Status is CebStatus.CompteEstBon or CebStatus.CompteApproche or CebStatus.Invalide;
        // ReSharper disable once ValueParameterNotUsed
        set => OnPropertyChanged();
    }

    public int Count {
        get => Tirage.Count;
        // ReSharper disable once ValueParameterNotUsed
        set => OnPropertyChanged();
    }

    public bool Popup {
        get => _popup;
        set {
            if (_popup == value)
                return;

            _popup = value;
            if (_popup)
                DelayPopup(5000);
            else
                _timer?.Dispose();

            OnPropertyChanged();
        }
    }

    public string Titre {
        get => _titre;
        set {
            _titre = value;
            OnPropertyChanged();
        }
    }

    public bool CanExecute(object? parameter) => true;

    public async void Execute(object? parameter) {
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
                    await ExportFichierAsync();

                break;
            case "theme":
                ThemeDark = !ThemeDark;
                break;
            case "vue":
                VueGrille = !VueGrille;
                break;
        }
    }

    public event EventHandler? CanExecuteChanged;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void DelayPopup(int duetime) {
        _timer = new Timer(state => {
            if (state is not Timer ti) return;
            ti.Dispose();
            Popup = false;
        });
        _timer.Change(duetime, 0);
    }

    private void ClearData() {
        Solution = null!;
        UpdateForeground();
        Result = Tirage.Status != CebStatus.Invalide ? "" : "Tirage invalide";
        Popup = false;
        OnPropertyChanged(nameof(IsComputed), nameof(Duree), nameof(Solutions), nameof(Found), nameof(Count));
    }

    private void UpdateData() {
        IsBusy = true;
        lock (Plaques) {
            foreach (var (p, i) in Tirage.Plaques.Indexed()) Plaques[i] = p.Value;
        }

        OnPropertyChanged(nameof(Plaques), nameof(Search));
        IsBusy = false;
        ClearData();
    }

    private void UpdateForeground() => Foreground =
        Tirage.Status switch {
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
        ShowPopup(Tirage.Solutions[index]);
    }

    public void ShowPopup(CebBase sol) {
        Solution = sol;
        Popup = true;
    }


    private async Task ExportFichierAsync() {
        if (Tirage.Status is not (CebStatus.CompteEstBon or CebStatus.CompteApproche)) return;

        var typ = ListExportFiles[FormatExport];
        var filename = $"CompteEstBon.{typ.Extension}";
        await using var mstream = new MemoryStream();
        Action<MemoryStream> exportStream = typ.Extension switch {
            "xlsx" => Tirage.ExcelSaveStream,
            "docx" => Tirage.WordStream,
            "json" => Tirage.JsonSaveStream,
            "xml" => Tirage.XmlSaveStream,
            "html" => Tirage.HtmlStream,
            _ => throw new NotImplementedException()
        };
        exportStream(mstream);
        SaveService.SaveAndView(filename, typ.ContentType, mstream);
    }


    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    protected virtual void OnPropertyChanged(params string[] properties) {
        foreach (var property in properties) PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
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
        Result = Tirage.Status switch {
            CebStatus.CompteEstBon => "😊 Compte est Bon",
            CebStatus.CompteApproche => $"😢 Compte approché",
            CebStatus.Invalide => "🤬 Tirage invalide",
            _ => ""
        };

        Solution = Tirage.Solutions[0];

        UpdateForeground();
        IsBusy = false;
        OnPropertyChanged(nameof(Duree),
            nameof(Solutions), nameof(Count), nameof(IsComputed), nameof(Found));
        ShowPopup();

        return Tirage.Status;
    }

    #endregion Action
}

public record ExportFile(string Extension, string ContentType) {
    public string Extension = Extension;
    public string ContentType = ContentType;
}