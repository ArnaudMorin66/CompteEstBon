﻿//-----------------------------------------------------------------------
// <copyright file="ViewTirage.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#region Using

using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Input;

using CommunityToolkit.Maui.Storage;

using CompteEstBon;


using Syncfusion.Maui.Themes;


#if WINDOWS
using Microsoft.Win32;

using Windows.Storage;
using Launcher = Windows.System.Launcher;
#endif

#endregion

#pragma warning disable CS0067
// ReSharper disable EnforceIfStatementBraces
namespace CebToolkit.ViewModel;

public class ViewTirage : INotifyPropertyChanged, ICommand {
    public static readonly string[] ListeFormats = ["Excel", "Word", "Json", "Xml", "HTML"];


    private bool _auto;
    private Color _background = Color.FromRgb(22, 22, 22);

    // private readonly Stopwatch _notifyWatch = new();
    private string _fmtExport;

    private Color _foreground = Colors.White;

    private bool _isBusy;


    private bool _popup;

    private string _result = "Résoudre";

    private CebBase _solution = null!;

// 
    private bool _themeDark = true;
    private Timer? _timer;

#if WINDOWS
    private static bool IsLightTheme() {
        using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
        var value = key?.GetValue("AppsUseLightTheme");
        return value is int i && i > 0;
    }
#endif
    private bool _vueGrille;

    /// <summary>
    ///     Initialisation
    /// </summary>
    /// <returns>
    /// </returns>
    public ViewTirage() {
        Auto = false;
#if WINDOWS
        ThemeDark = !IsLightTheme();
#else
        ThemeDark = true;
#endif
        Tirage.PropertyChanged += (_, args) => {
            if (args.PropertyName != "Clear") return;
            ClearData();
            if (Auto)
                Task.Run(ResolveAsync);
        };
        _fmtExport = "Excel";
        ExportsCommmand = new Command<string>(format => {
            if (string.IsNullOrEmpty( format))
                format = FmtExport;
            if (format == "export")
                format = FmtExport;
#pragma warning disable CA1862
            var elt = ListeFormats.FirstOrDefault(elt => elt.ToLower() == format.ToLower());
            if (elt != null) {
                FmtExport = elt;
                if (Tirage.Count != 0) Task.Run(() => ExportFichierAsync(elt));
            }
        });

        ClearData();
    }

    public string FmtExport {
        get => _fmtExport;
        set {
            if (_fmtExport == value) return;
            _fmtExport = value;
            OnPropertyChanged();
        }
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

    public ICommand ExportsCommmand { get; init; }
    public static IEnumerable<int> ListePlaques => CebPlaque.DistinctPlaques;

    public CebTirage Tirage { get; } = new();


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
                if (Tirage.Count != 0)
                    await ExportFichierAsync(FmtExport);

                break;
            case "theme":
                ThemeDark = !ThemeDark;
                break;
            case "vue":
                VueGrille = !VueGrille;
                break;
            case "auto":
                Auto = !Auto;
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
        OnPropertyChanged(nameof(IsComputed), nameof(Tirage));
    }


    private void UpdateForeground() => Foreground =
        Tirage.Status switch {
            CebStatus.Indefini => Colors.Blue,
            CebStatus.Valide => Colors.White,
            CebStatus.EnCours => Colors.Aqua,
            CebStatus.CompteEstBon => ThemeDark ? Colors.SpringGreen : Colors.DarkSlateGray,
            CebStatus.CompteApproche => ThemeDark ? Colors.Orange : Colors.OrangeRed,
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


    private async Task ExportFichierAsync(string fmt) {
        if (Tirage.Status is not (CebStatus.CompteEstBon or CebStatus.CompteApproche)) return;

        var extension =
            fmt.ToLower() switch {
                "excel" => "xlsx",
                "word" => "docx",
                "json" => "json",
                "xml" => "xml",
                "html" => "html",
                _ => throw new NotImplementedException()
            };
        Action<MemoryStream> exportStream = extension switch {
            "xlsx" => Tirage.ExcelSaveStream,
            "docx" => Tirage.WordStream,
            "json" => Tirage.JsonSaveStream,
            "xml" => Tirage.XmlSaveStream,
            "html" => Tirage.HtmlStream,
            _ => throw new NotImplementedException()
        };
        await using var mstream = new MemoryStream();
        exportStream(mstream);
#pragma warning disable CA1416
        // ReSharper disable once UnusedVariable
        var fileresult = await FileSaver.Default.SaveAsync($"CompteEstBon.{extension}", mstream);
#if WINDOWS
        if (fileresult.IsSuccessful)
            await ShowFile(fileresult.FilePath);
#endif
    }


    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    protected virtual void OnPropertyChanged(params string[] properties) {
        foreach (var property in properties) OnPropertyChanged(property); 
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
        ClearData();
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
            CebStatus.CompteApproche => "\ud83d\ude22 Compte approché",
            CebStatus.Invalide => "🤬 Tirage invalide",
            _ => ""
        };

        Solution = Tirage.Solutions[0];

        UpdateForeground();
        IsBusy = false;
        OnPropertyChanged(nameof(Tirage),
            nameof(IsComputed));
        ShowPopup();

        return Tirage.Status;
    }

    #endregion Action

#if WINDOWS
    private async Task ShowFile(string filename) {
        await MainThread.InvokeOnMainThreadAsync(async () => {
            if (await Application.Current!.Windows[0].Page!
                    .DisplayAlert("Le Compte est Bon", "Afficher le fichier", "Oui", "Non"))
                await Launcher.LaunchFileAsync(await StorageFile.GetFileFromPathAsync(filename));
        });
    }
#endif
}