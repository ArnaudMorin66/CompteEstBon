//-----------------------------------------------------------------------
// <copyright file="ViewTirage.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#region Using

// ReSharper disable InconsistentNaming


using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using CompteEstBon;

using Syncfusion.Maui.Themes;
// ReSharper disable All
#if WINDOWS
using Windows.Storage;

using Launcher = Windows.System.Launcher;

#endif

#endregion

#pragma warning disable CS0067
// ReSharper disable EnforceIfStatementBraces
namespace CebToolkit.ViewModel;

#pragma warning disable IDE1006 // Styles d'affectation de noms
[SuppressMessage(
    "CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator",
    "MVVMTK0045:Using [ObservableProperty] on fields is not AOT compatible for WinRT")]
public partial class ViewTirage : ObservableObject {
    [ObservableProperty] private bool auto;
    [ObservableProperty] private TimeSpan elapsedTime;
    [ObservableProperty] private string fmtExport;

    [ObservableProperty] private Color foreground = Colors.LightGray;
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
    /// <returns></returns>
    public ViewTirage() {
        Auto = false;
        ThemeDark = Application.Current?.RequestedTheme == AppTheme.Dark;
        Tirage.PropertyChanged += (_, args) => {
            if (args.PropertyName != "Clear") return;
            ClearData();
            if (Auto) Task.Run(Solve);
        };
        fmtExport = "Excel";
        stopwatch = new Stopwatch();
        timerChrono = new Timer(_ => ElapsedTime = stopwatch.Elapsed, null, 0, 100);
        elapsedTime = TimeSpan.Zero;
        vueGrille = DeviceInfo.Current.Idiom == DeviceIdiom.Phone &&
                    DeviceDisplay.Current.MainDisplayInfo.Orientation == DisplayOrientation.Portrait;
        ClearData();
    }

    /// <summary>
    ///     Clears the data of the ViewTirage.
    /// </summary>
    private void ClearData() {
        Solution = null!;
        stopwatch.Reset();
        ElapsedTime = TimeSpan.Zero;
        UpdateForeground();
        IsComputed = Tirage.Status == CebStatus.Invalide;
        Result = Tirage.Status != CebStatus.Invalide ? "Le Compte Est Bon" : "Tirage invalide";
        Popup = false;
        OnPropertyChanged(nameof(Tirage));
    }

    /// <summary>
    ///     Delays the popup for a specified amount of time.
    /// </summary>
    /// <param name="duetime">The delay time in milliseconds.</param>
    private void DelayPopup(int duetime) {
        timerPopup = new Timer(
            state => {
                if (state is not Timer ti) return;
                ti.Dispose();
                Popup = false;
            });
        timerPopup.Change(duetime, 0);
    }

    partial void OnAutoChanged(bool value) {
        if (!value) return;
        if (Tirage.Status == CebStatus.Valide) Task.Run(Solve);
    }

    partial void OnPopupChanged(bool value) {
        if (value)
            DelayPopup(5000);
        else
            timerPopup?.Dispose();
    }

    /// <summary>
    ///     Handles changes to the ThemeDark property.
    /// </summary>
    /// <param name="value">A boolean value indicating whether the dark theme is enabled.</param>
    partial void OnThemeDarkChanged(bool value) {
        var mergedDictionaries = Application.Current?.Resources.MergedDictionaries;

        var theme = mergedDictionaries?.OfType<SyncfusionThemeResourceDictionary>().FirstOrDefault();
        if (theme == null) return;

        theme.VisualTheme = value ? SfVisuals.MaterialDark : SfVisuals.MaterialLight;
        Application.Current!.UserAppTheme = value ? AppTheme.Dark : AppTheme.Light;

        UpdateForeground();
    }

    /// <summary>
    ///     Updates the foreground color based on the status of the "Compte Est Bon" game.
    /// </summary>
    private void UpdateForeground() => Foreground =
        Tirage.Status switch {
            CebStatus.Indefini => ThemeDark ? Colors.Blue : Colors.DarkBlue,
            CebStatus.Valide => Colors.LightGray,
            CebStatus.EnCours => ThemeDark ? Colors.Aqua : Colors.Black,
            CebStatus.CompteEstBon => ThemeDark ? Colors.GreenYellow : Colors.DarkGreen,
            CebStatus.CompteApproche => Colors.Orange,
            CebStatus.Invalide => Colors.Red,
            var _ => throw new NotImplementedException()
        };

    /// <summary>
    ///     Exports the solutions in the specified format.
    /// </summary>
    /// <param name="format">The format in which to export the solutions.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [RelayCommand]
    public async Task Export(string format) {
        if (Tirage.Solutions is []) return;
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
            var _ => throw new NotImplementedException()
        };

        await using var mstream = new MemoryStream();
        if (!Tirage.Export(extension, mstream)) return;

#pragma warning disable CA1416
        // ReSharper disable once UnusedVariable
        var fileresult = await FileSaver.Default.SaveAsync($"CompteEstBon.{extension}", mstream);
#if WINDOWS
        if (fileresult.IsSuccessful) await ShowFile(fileresult.FilePath);
#endif
    }

    /// <summary>
    ///     Quitte l' application.
    /// </summary>
    [RelayCommand]
    public void Quitter() => Application.Current?.Quit();

    /// <summary>
    ///     Executes a random selection of values for the "Compte est bon" game.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [RelayCommand]
    public async Task Random() => await Tirage.RandomAsync();

    /// <summary>
    ///     Resolves the current "Compte est bon" problem by finding the best possible solution.
    /// </summary>
    /// <returns>
    ///     The status of the resolution process, indicating whether the solution is exact, approximate, or invalid.
    /// </returns>
    /// <remarks>
    ///     This method checks the current status of the "Compte est bon" game and performs the necessary actions to resolve
    ///     it. If the game is already solved or in progress, it clears the current state. If the game is invalid, it
    ///     generates a new random game. If the game is valid, it starts the resolution process and updates the status and
    ///     result accordingly.
    /// </remarks>
    [RelayCommand]
    public async Task Solve() {
        if (IsBusy) return;

        switch (Tirage.Status) {
            case CebStatus.CompteEstBon or CebStatus.CompteApproche:
                await Tirage.ClearAsync();
                return;
            case CebStatus.Invalide:
                await Tirage.RandomAsync();
                return;
        }

        IsBusy = true;
        stopwatch.Restart();
        Result = "⏰ Calcul en cours...";
        Foreground = Colors.Aqua;
        await Tirage.SolveAsync();
        stopwatch.Stop();
        Result = Tirage.Status switch {
            CebStatus.CompteEstBon => "😊 Compte est Bon",
            CebStatus.CompteApproche => "😒 Compte approché",
            CebStatus.Invalide => "🤬 Tirage invalide",
            var _ => string.Empty
        };
        UpdateForeground();

        OnPropertyChanged(nameof(Tirage));

        IsBusy = false;
        IsComputed = true;
        ShowPopup(Tirage.Solutions[0]);
    }

    /// <summary>
    ///     Shows the popup for the specified solution of the solutions list.
    /// </summary>
    /// <param name="sol">The solution of the solution to show.</param>
    [RelayCommand]
    public void ShowPopup(CebBase sol) {
        Solution = sol;
        Popup = true;
    }

    /// <summary>
    ///     Updates the specified property based on the provided command.
    /// </summary>
    /// <param name="propriete">The property to update, specified as an object.</param>
    [RelayCommand]
    public void UpdateProperty(object? propriete) {
        var cmd = (propriete as string)?.ToLower();
        switch (cmd) {
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

    /// <summary>
    ///     Gets the current "Compte est bon" game instance.
    /// </summary>
    public CebTirage Tirage { get; } = new();

    /// <summary>
    ///     List of supported export formats.
    /// </summary>
    public static readonly string[] ListeFormats = ["Excel", "Word", "Json", "Xml", "HTML"];

    private Timer? timerPopup { get; set; }

    // ReSharper disable once NotAccessedField.Local
    private Timer? timerChrono;

    private readonly Stopwatch stopwatch;
#if WINDOWS
    /// <summary>
    ///     Shows the specified file using the default application on Windows.
    /// </summary>
    /// <param name="filename">The path of the file to show.</param>
    private static async Task ShowFile(string filename) => await MainThread.InvokeOnMainThreadAsync(async () => {
        if (await Application.Current!.Windows[0].Page!
                .DisplayAlert("Le Compte est Bon", "Afficher le fichier", "Oui", "Non"))
            await Launcher.LaunchFileAsync(await StorageFile.GetFileFromPathAsync(filename));
    });
#endif
}
