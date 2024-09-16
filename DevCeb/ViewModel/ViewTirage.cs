//-----------------------------------------------------------------------
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

using arnaud.morin.outils;

// using DevCeb.Services;

using CompteEstBon;


#endregion

#pragma warning disable CS0067
// ReSharper disable EnforceIfStatementBraces
namespace DevCeb.ViewModel;

public class ViewTirage : INotifyPropertyChanged, ICommand {
    public static readonly string[] ListeFormats = ["Excel", "Word", "Json", "Xml", "HTML"];


    private bool _auto;
    private Color _background = Color.FromRgb(22, 22, 22);

    private Color _foreground = Colors.White;

    // private readonly Stopwatch _notifyWatch = new();
    private int _indexExport;

    private bool _isBusy;


    private bool _popup;

    private string _result = "Résoudre";

    private CebBase _solution = null!;

// 
    private bool _themeDark = true;
    private Timer? _timer;


    private bool _vueGrille;

    /// <summary>
    ///     Initialisation
    /// </summary>
    /// <returns>
    /// </returns>
    public ViewTirage() {
        Auto = false;
        ThemeDark = true;
        Tirage.PropertyChanged += (_, args) => {
            if (args.PropertyName != "Clear") return;
            ClearData();
            if (Auto)
                Task.Run(ResolveAsync);
        };
        ClearData();
    }

    public int IndexExport {
        get => _indexExport;
        set {
            if (_indexExport == value) return;
            _indexExport = value;
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
            //var mergedDictionaries = Application.Current!.Resources.MergedDictionaries;
            //var theme = mergedDictionaries?.OfType<SyncfusionThemeResourceDictionary>().FirstOrDefault();
            //if (theme == null) return;
            //if (_themeDark) {
            //    theme.VisualTheme = SfVisuals.MaterialDark;
            //    Application.Current.UserAppTheme = AppTheme.Dark;
            //} else {
            //    theme.VisualTheme = SfVisuals.MaterialLight;
            //    Application.Current.UserAppTheme = AppTheme.Light;
            //}

            UpdateForeground();
            OnPropertyChanged();
        }
    }


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
#pragma warning disable CA1862
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
                    await ExportFichierAsync();

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
            default:
                //if (ListeFormats.Any(p => p.ToLower() == cmd)) {
                    var (elt, i) = ListeFormats.Indexed().FirstOrDefault(elt => elt.Item1.ToLower() == cmd);
                        if (elt != null) {
                    IndexExport = i;
                        if (Tirage.Count != 0) await ExportFichierAsync();
                        break;
                }
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
        OnPropertyChanged(nameof(IsComputed), nameof(Tirage), "plq");
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


        var (extension, contentType) =
            IndexExport switch {
                0 => ("xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"),
                1 => ("docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"),
                2 => ("json", "application/json"),
                3 => ("xml", "application/xml"),
                4 => ("html", "text/html"),
                _ => throw new NotImplementedException()
            };
        //Action<MemoryStream> exportStream = extension switch {
        //    "xlsx" => Tirage.ExcelSaveStream,
        //    "docx" => Tirage.WordStream,
        //    "json" => Tirage.JsonSaveStream,
        //    "xml" => Tirage.XmlSaveStream,
        //    "html" => Tirage.HtmlStream,
        //    _ => throw new NotImplementedException()
        //};
        //await using var mstream = new MemoryStream();
        //exportStream(mstream);

//        SaveService.SaveAndView($"CompteEstBon.{extension}", contentType, mstream);
    }


    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    protected virtual void OnPropertyChanged(params string[] properties) {
        foreach (var property in properties) PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
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
}

public record ExportFile(string Extension, string ContentType) {
    public string ContentType = ContentType;
    public string Extension = Extension;
}