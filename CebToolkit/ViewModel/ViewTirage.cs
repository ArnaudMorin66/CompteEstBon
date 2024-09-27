//-----------------------------------------------------------------------
// <copyright file="ViewTirage.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#region Using

using System.Reflection;
using System.Runtime.InteropServices;

using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

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

public partial class ViewTirage : ObservableObject {
    public static readonly string[] ListeFormats = ["Excel", "Word", "Json", "Xml", "HTML"];

// 
    private Timer? _timer;
#if WINDOWS
    private Timer? _timerDay;

    public Timer? TimerDay {
        get => _timerDay;
        set => SetProperty(ref _timerDay, value);
    }

    

    private static bool IsLightTheme() {
        using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
        var value = key?.GetValue("AppsUseLightTheme");
        return value is int i && i > 0;
    }
#endif

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
        
#if WINDOWS
        TimerDay = new Timer((_) => Date = DateTime.Now, null,1000, 1000);
#endif
        ClearData();
    }
#if WINDOWS
    ~ViewTirage() {
        TimerDay?.Dispose();
    }
#endif
    private string _fmtExport;
    public string FmtExport {
        get => _fmtExport;
        set => SetProperty(ref _fmtExport, value);
        
    }

    public static string DotnetVersion =>
        $"Version: {Assembly.GetExecutingAssembly().GetName().Version}, {RuntimeInformation.FrameworkDescription}";

    private Color _background = Color.FromRgb(22, 22, 22);
    public Color Background {
        get => _background;
        set => SetProperty(ref _background, value);
}

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

    // public ICommand ExportsCommmand { get; init; }
    public static IEnumerable<int> ListePlaques => CebPlaque.DistinctPlaques;

    public CebTirage Tirage { get; } = new();

    private bool _vueGrille;
    public bool VueGrille {
        get => _vueGrille;
        set => SetProperty(ref _vueGrille, value);
    }

    private CebBase _solution = null!;

    public CebBase Solution {
        get => _solution;
        set => SetProperty(ref _solution, value);
    }

    private Color _foreground = Colors.White;

    public Color Foreground {
        get => _foreground;
        set => SetProperty(ref _foreground, value);
        
    }

    private string _result = "Résoudre";

    public string Result {
        get => _result;
        set => SetProperty(ref _result, value);
    }

    private bool _isBusy;
    public bool IsBusy {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }
    
    private bool _auto;
    public bool Auto {
        get => _auto;
        set {
            if (SetProperty(ref _auto, value) && Tirage.Status == CebStatus.Valide && Auto) Task.Run(ResolveAsync);
        }
    }

    private bool _isComputed;
    public bool IsComputed {
        get => _isComputed; 
        // ReSharper disable once ValueParameterNotUsed
        set => SetProperty(ref _isComputed, value);
    }

private bool _popup;
public bool Popup {
    get => _popup;
    set {
        if (!SetProperty(ref _popup, value)) return;
        if (value)
            DelayPopup(5000);
        else
            _timer?.Dispose();
    }
}

#if WINDOWS
    private DateTime _date;

    public DateTime Date {
        get => _date;
        set => SetProperty(ref _date, value);
    }
    

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

    public event EventHandler? CanExecuteChanged;

    //public event PropertyChangedEventHandler? PropertyChanged;

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


    //protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
    //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    

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