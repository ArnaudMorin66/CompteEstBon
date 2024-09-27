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


#if WINDOWS
using Microsoft.Win32;

using Windows.Storage;
using Launcher = Windows.System.Launcher;
// ReSharper disable InconsistentNaming
#endif

#endregion

#pragma warning disable CS0067
// ReSharper disable EnforceIfStatementBraces
namespace CebToolkit.ViewModel;

public partial class ViewTirage : ObservableObject {
    public static readonly string[] ListeFormats = ["Excel", "Word", "Json", "Xml", "HTML"];
    public static IEnumerable<int> ListePlaques => CebPlaque.DistinctPlaques;
    public CebTirage Tirage { get; } = new();

    // 
    private Timer? _timer;
    
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
        fmtExport = "Excel";
        
#if WINDOWS
        timerDay = new Timer((_) => Date = DateTime.Now, null,1000, 1000);
#endif
        ClearData();
    }
#if WINDOWS
    [ObservableProperty] public Timer? timerDay;
    private static bool IsLightTheme() {
        using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
        var value = key?.GetValue("AppsUseLightTheme");
        return value is int and > 0;
    }
#endif

    [ObservableProperty] public string fmtExport;
    [ObservableProperty] public string result = "Résoudre";
    [ObservableProperty] public Color background = Color.FromRgb(22, 22, 22);
    [ObservableProperty] public bool themeDark;
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
    
    [ObservableProperty] public bool vueGrille;
    [ObservableProperty] public CebBase? solution=null;
    [ObservableProperty] public Color foreground = Colors.White;
    [ObservableProperty] public bool isBusy;
    [ObservableProperty] public bool auto;
    [ObservableProperty] public bool isComputed;
    [ObservableProperty] public bool popup;
    partial void OnPopupChanged(bool value)
    {
        if (value)
            DelayPopup(5000);
        else
            _timer?.Dispose();
    }
#if WINDOWS
    [ObservableProperty] public DateTime date;
#endif

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

            case "export":
                if (Tirage.Count != 0)
                    await Export(FmtExport);

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
         OnPropertiesChanged( nameof(Tirage));
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

    [RelayCommand]
    public async Task Export(string format) {
        if (string.IsNullOrEmpty(format))
            format = FmtExport;
        if (format == "export")
            format = FmtExport;
#pragma warning disable CA1862
        var fmt = ListeFormats.FirstOrDefault(elt => elt.ToLower() == format.ToLower());
        if (fmt == null) return;
            FmtExport = fmt;
            
        if (Tirage.Status is not (CebStatus.CompteEstBon or CebStatus.CompteApproche)) return;
        string extension;
        Action<Stream> exportStream; 
        switch (fmt.ToLower()) {
            case "excel":
                extension = "xlsx";
                exportStream = Tirage.ExcelSaveStream;
                break;
            case "word":
                extension = "docx";
                exportStream = Tirage.WordSaveStream;
                break;
            case "json":
                extension = "json";
                exportStream= Tirage.JsonSaveStream;
                break;
            case "xml":
                extension = "xml";
                exportStream = Tirage.XmlSaveStream;
                break;
            case "html":
                extension = "html";
                exportStream = Tirage.HtmlSaveStream;
                break;
            default:
                throw new NotImplementedException();


        }
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
        IsComputed = true;
        OnPropertyChanged(nameof(Tirage));
        ShowPopup();

        return Tirage.Status;
    }

    public void OnPropertiesChanged(params string[] properties) {
        foreach (var property in properties) {
            OnPropertyChanged(property);
        }
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
    // public event PropertyChangedEventHandler? PropertyChanged;


    //protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
    //    if (EqualityComparer<T>.Default.Equals(field, value)) return false;
    //    field = value;
    //    OnPropertyChanged(propertyName);
    //    return true;
    //}
}