//-----------------------------------------------------------------------
// <copyright file="ViewTirage.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#region Using

using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using CompteEstBon;

using Syncfusion.Maui.Themes;
// ReSharper disable InconsistentNaming
#if WINDOWS
using Microsoft.Win32;

using Windows.Storage;

using Launcher = Windows.System.Launcher;

#endif

#endregion

#pragma warning disable CS0067
// ReSharper disable EnforceIfStatementBraces
namespace CebToolkit.ViewModel;

public partial class ViewTirage : ObservableObject {
    public static readonly string[] ListeFormats = ["Excel", "Word", "Json", "Xml", "HTML"];
    private Timer? _timer;
    [ObservableProperty] private bool auto;
    [ObservableProperty] private Color background = Color.FromRgb(22, 22, 22);
#if WINDOWS
    [ObservableProperty] private DateTime date;
#endif
    [ObservableProperty] private string fmtExport;
    [ObservableProperty] private Color foreground = Colors.White;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private bool isComputed;
    [ObservableProperty] private bool popup;
    [ObservableProperty] private string result = "Résoudre";
    [ObservableProperty] private CebBase? solution;
    [ObservableProperty] private bool themeDark;
    [ObservableProperty] private bool vueGrille;

    /// <summary>
    ///     Initialisation
    /// </summary>
    /// <returns>
    /// </returns>
    public ViewTirage() {
        Auto = false;
        
        ThemeDark = Application.Current?.RequestedTheme == AppTheme.Dark;

        Tirage.PropertyChanged += (_, args) => {
            if (args.PropertyName != "Clear") return;
            ClearData();

            if (Auto) Task.Run(ResolveAsync);
        };
        fmtExport = "Excel";

#if WINDOWS
        timerDay = new Timer(_ => Date = DateTime.Now, null, 1000, 1000);
#endif
        vueGrille = false;
        ClearData();
    }

    public static IEnumerable<int> ListePlaques => CebPlaque.DistinctPlaques;
    public CebTirage Tirage { get; } = new();

    partial void OnThemeDarkChanged(bool value) {
        var mergedDictionaries = Application.Current!.Resources.MergedDictionaries;
        var theme = mergedDictionaries?.OfType<SyncfusionThemeResourceDictionary>().FirstOrDefault();
        if (theme is null) return;
        if (value) {
            theme.VisualTheme = SfVisuals.MaterialDark;
            Application.Current.UserAppTheme = AppTheme.Dark;
        } else {
            theme.VisualTheme = SfVisuals.MaterialLight;
            Application.Current.UserAppTheme = AppTheme.Light;
        }

        UpdateForeground();
    }

     partial void OnAutoChanged(bool value) {
        if (!value) return;
        if (Tirage.Status == CebStatus.Valide)
Task.Run(ResolveAsync);
     }

    partial void OnPopupChanged(bool value) {
        if (value)
            DelayPopup(5000);
        else
            _timer?.Dispose();
    }

    [RelayCommand]
    public async Task Ceb(object? parameter) {
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
        IsComputed = Tirage.Status == CebStatus.Invalide;
        Result = Tirage.Status != CebStatus.Invalide ? "Le Compte Est Bon" : "Tirage invalide";
        Popup = false;
        OnPropertyChanged(nameof(Tirage));
    }


    private void UpdateForeground() => Foreground =
        Tirage.Status switch {
            CebStatus.Indefini => Colors.Blue,
            CebStatus.Valide => Colors.White,
            CebStatus.EnCours => Colors.Aqua,
            CebStatus.CompteEstBon => ThemeDark ? Colors.GreenYellow: Colors.DarkGreen,
            CebStatus.CompteApproche => Colors.Orange,
            CebStatus.Invalide => Colors.Red,
            _ => throw new NotImplementedException()
        };

    public void ShowPopup(int index = 0) {
        if (index < 0) return;
        ShowPopup(Tirage.Solutions[index]);
    }

    public void ShowPopup(CebBase sol) {
        Solution = sol;
        Popup = true;
    }
#if WINDOWS
    [ObservableProperty] public Timer? timerDay;
    private static bool IsLightTheme() {
        using var key =
            Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
        var value = key?.GetValue("AppsUseLightTheme");
        return value is int and > 0;
    }
#endif

    [RelayCommand]
    public async Task Export(string format) {
        if (Tirage.Status is not (CebStatus.CompteEstBon or CebStatus.CompteApproche)) return;
        if (string.IsNullOrEmpty(format)) format = FmtExport;
#pragma warning disable CA1862
        var fmt = ListeFormats.FirstOrDefault(elt => elt.ToLower() == format.ToLower());
        if (fmt == null) return;
        FmtExport = fmt;

        var extension = fmt.ToLower() switch {
            "excel" => "xlsx",
            "word" => "docx",
            "json" => "json",
            "html" => "html",
            "xml" => "xml",
            _ => throw new NotImplementedException()
        };
                
        await using var mstream = new MemoryStream();
        if(! Tirage.Export(extension, stream: mstream)) return;
        
#pragma warning disable CA1416
        // ReSharper disable once UnusedVariable
        var fileresult = await FileSaver.Default.SaveAsync($"CompteEstBon.{extension}", mstream);
#if WINDOWS
        if (fileresult.IsSuccessful) await ShowFile(fileresult.FilePath);
#endif
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
        if (Auto) await ResolveAsync();
    }

    public async ValueTask<CebStatus> ResolveAsync() {
        if (IsBusy) return Tirage.Status;

        IsBusy = true;
        Result = "⏰ Calcul en cours...";
        Foreground = Colors.Aqua;
        await Tirage.ResolveAsync();
        Result = Tirage.Status switch {
            CebStatus.CompteEstBon => "😊 Compte est Bon",
            CebStatus.CompteApproche => "😒 Compte approché",
            CebStatus.Invalide => "🤬 Tirage invalide",
            _ => ""
        };

        Solution = Tirage.Solutions[0];

        UpdateForeground();
        IsBusy = false;
        IsComputed = true;
        OnPropertyChanged(nameof(Tirage));
        ShowPopup();

        return Tirage.Status;
    }

    public void OnPropertiesChanged(params string[] properties) {
        foreach (var property in properties) OnPropertyChanged(property);
    }

    #endregion Action

#if WINDOWS
    private async Task ShowFile(string filename) => await MainThread.InvokeOnMainThreadAsync(async () => {
        if (await Application.Current!.Windows[0].Page!
                .DisplayAlert("Le Compte est Bon", "Afficher le fichier", "Oui", "Non"))
            await Launcher.LaunchFileAsync(await StorageFile.GetFileFromPathAsync(filename));
    });
#endif
    
}