#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using CompteEstBon.Properties;
using Microsoft.Win32;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Syncfusion.Windows.Shared;
//using Syncfusion.Drawing;

#endregion

// ReSharper disable once CheckNamespace
namespace CompteEstBon; 

public class ViewTirage : NotificationObject, ICommand {
    private readonly Stopwatch NotifyWatch = new();
    private readonly TimeSpan SolutionTimer = TimeSpan.FromSeconds(Settings.Default.SolutionTimer);

    private bool _auto;

    private int _count;

    private TimeSpan _duree;

    private Color _foreground = Colors.White;
    private bool _isBusy;
    private bool _isUpdating;

    // ⁞…
    private char _modeView = '…';
    private bool _popup;

    private string _result = "Résoudre";

    public CebBase _solution;

    public IEnumerable<CebBase> _solutions;
    private string _titre = "Jeu du Compte Est Bon";

    private bool _vertical;
    public DispatcherTimer dateDispatcher;
    public Stopwatch stopwatch;

    /// <summary>
    ///     Initialisation
    /// </summary>
    /// <returns>
    /// </returns>
    public ViewTirage() {
        stopwatch = new Stopwatch();

        dateDispatcher = new DispatcherTimer {
            Interval = TimeSpan.FromMilliseconds(Settings.Default.SolutionTimer)
        };
        dateDispatcher.Tick += (_, _) => {
            if (stopwatch.IsRunning) Duree = stopwatch.Elapsed;

            if (Popup && NotifyWatch.Elapsed > SolutionTimer) Popup = false;
            Titre = $"{Result} - {DateTime.Now:dddd dd MMMM yyyy à HH:mm:ss}";
        };

        Plaques.CollectionChanged += (_, e) => {
            if (e.Action != NotifyCollectionChangedAction.Replace) return;

            var i = e.NewStartingIndex;
            Tirage.Plaques[i].Value = Plaques[i];
            Tirage.Valid();
            Task.Run(ClearAsync);
        };
        _isUpdating = false;
        UpdateData();
        Titre = $"{Result} - {DateTime.Now:dddd dd MMMM yyyy à HH:mm:ss}";
        dateDispatcher.Start();
    }

    public static IEnumerable<int> ListePlaques => CebPlaque.AnyPlaques;

    public CebTirage Tirage { get; } = new();

    public ObservableCollection<int> Plaques { get; } = new() { 0, 0, 0, 0, 0, 0 };

    public IEnumerable<CebBase> Solutions {
        get => _solutions;
        set {
            _solutions = value;
            RaisePropertyChanged(nameof(Solutions));

            Count = _solutions?.Count() ?? 0;
        }
    }

    public TimeSpan Duree {
        get => _duree;
        set {
            _duree = value;
            RaisePropertyChanged(nameof(Duree));
        }
    }

    public bool Vertical {
        get => _vertical;
        set {
            _vertical = value;
            ModeView = value ? '⁞' : '…';
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
            Tirage.Search = value;
            RaisePropertyChanged(nameof(Search));
            Task.Run(ClearAsync);
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
            if (value == _result) return;
            _result = value;
            RaisePropertyChanged(nameof(Result));
        }
    }

    public bool IsBusy {
        get => _isBusy;
        set {
            _isBusy = value;
            RaisePropertyChanged(nameof(IsBusy));
        }
    }

    public bool Auto {
        get => _auto;
        set {
            if (_auto == value) return;
            _auto = value;
            RaisePropertyChanged(nameof(Auto));
            Task.Run(ClearAsync);
        }
    }

    public bool IsComputed {
        get => Tirage.Status is CebStatus.CompteEstBon or CebStatus.CompteApproche;
        // ReSharper disable once ValueParameterNotUsed
        set => RaisePropertyChanged(nameof(IsComputed));
    }

    public int Count {
        get => _count;
        set {
            if (_count == value) return;
            _count = value;
            RaisePropertyChanged(nameof(Count));
        }
    }

    public bool Popup {
        get => _popup;
        set {
            if (_popup == value) return;
            _popup = value;
            NotifyWatch.Stop();
            NotifyWatch.Reset();
            if (_popup) NotifyWatch.Start();
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

    public bool CanExecute(object parameter) {
        return true;
    }

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

                    case CebStatus.CompteEstBon:
                    case CebStatus.CompteApproche:
                        await ClearAsync();
                        break;

                    case CebStatus.Invalide:
                        await RandomAsync();
                        break;
                }

                break;

            case "export":
                if (Count != 0) await ExportAsync();
                break;
        }
    }

    private async Task ExportAsync() {
        IsBusy = true;
        await Task.Run(ExportFichier);
        IsBusy = false;
    }

    private void ClearData() {
        if (_isUpdating) return;
        stopwatch.Reset();
        Duree = stopwatch.Elapsed;
        RaisePropertyChanged(nameof(IsComputed));
        Solution = null;
        Solutions = null;
        UpdateForeground();
        Result = Tirage.Status != CebStatus.Invalide ? "Jeu du Compte Est Bon" : "Tirage invalide";
        Popup = false;
    }

    private void UpdateData() {
        if (_isUpdating) return;
        _isUpdating = true;
        for (var i = 0; i < Tirage.Plaques.Count; i++)
            Plaques[i] = Tirage.Plaques[i].Value;
        _isUpdating = false;
        RaisePropertyChanged(nameof(Search));
        Task.Run(ClearAsync);
    }

    private void UpdateForeground() {
        Foreground = Tirage.Status switch {
            CebStatus.Indefini => Colors.Blue,
            CebStatus.Valide => Colors.White,
            CebStatus.EnCours => Colors.Aqua,
            CebStatus.CompteEstBon => Colors.ForestGreen,
            CebStatus.CompteApproche => Colors.Orange,
            CebStatus.Invalide => Colors.Red,
            _ => throw new NotImplementedException()
        };
    }

    public void ShowPopup(int index = 0) {
        if (index >= 0 && index < Tirage.Solutions.Count) {
            Solution = Tirage.Solutions[index];
            Popup = true;
        }
    }

    public static (bool Ok, string Path) SaveFileName() {
        var dialog = new SaveFileDialog();
        (dialog.Filter, dialog.Title) = ("Excel (*.xlsx)| *.xlsx | Word (*.docx) | *.docx", "Export Excel-Word");
        // ReSharper disable once PossibleInvalidOperationException
        return ((bool)dialog.ShowDialog(), dialog.FileName);
    }

    private async Task SaveMongoDB() {
        try {
            ConventionRegistry.Register("EnumStringConvention",
                new ConventionPack {
                    new EnumRepresentationConvention(BsonType.String)
                },
                _ => true);
            var clientSettings = MongoClientSettings.FromConnectionString(Settings.Default.MongoServer);
            clientSettings.LinqProvider = LinqProvider.V3;

            var cl = new MongoClient(clientSettings)
                .GetDatabase("ceb")
                .GetCollection<BsonDocument>("comptes");

            await cl.InsertOneAsync(
                new BsonDocument(new Dictionary<string, object> {
                        {
                            "_id",
                            new {
                                lang = "wpf", domain = Environment.GetEnvironmentVariable("USERDOMAIN"),
                                date = DateTime.UtcNow
                            }.ToBsonDocument()
                        }
                    })
                    .AddRange(Tirage.Data.ToBsonDocument()));
        }
        catch (Exception) {
            // ignored
        }
    }

    private void ExportFichier() {
        var (Ok, Path) = SaveFileName();
        if (Ok) {
            FileInfo fi = new(Path);
            if (fi.Exists)
                fi.Delete();

            Action<Stream> ExportFunction = fi.Extension switch {
                ".xlsx" => Tirage.ExportExcel,
                ".docx" => Tirage.ExportWord,
                _ => throw new NotImplementedException()
            };
            FileStream stream = new(Path!, FileMode.CreateNew);
            ExportFunction(stream);
            stream.Flush();
            stream.Close();
            OpenDocument(Path);
        }
    }

    public static void OpenDocument(string nom) {
        Process.Start(new ProcessStartInfo {
            UseShellExecute = true,
            FileName = nom
        });
    }

    #region Action

    public async Task ClearAsync() {
        await Tirage.ClearAsync();
        ClearData();
        if (!IsBusy && Auto && Tirage.Status == CebStatus.Valide)
            await ResolveAsync();
    }

    public async Task RandomAsync() {
        IsBusy = true;
        await Tirage.RandomAsync();
        UpdateData();
        IsBusy = false;
        //if (Auto && Tirage.Status == CebStatus.Valide) {
        //    stopwatch.Stop();
        //    await ResolveAsync();
        //}
    }

    public async Task<CebStatus> ResolveAsync() {
        IsBusy = true;
        Result = "⏰ Calcul en cours...";
        stopwatch.Start();
        Foreground = Colors.Aqua;

        await Tirage.ResolveAsync();
        Result = Tirage.Status switch {
            CebStatus.CompteEstBon => "😊 Compte est Bon",
            CebStatus.CompteApproche => $"😢 Compte approché: {Tirage.Found}, écart: {Tirage.Diff}",
            CebStatus.Invalide => "🤬 Tirage invalide",
            _ => "Jeu du Compte Est Bon"
        };
        stopwatch.Stop();
        Duree = stopwatch.Elapsed; // .FromSeconds( Tirage.Duree);
        Solution = Tirage.Solutions[0];
        Solutions = Tirage.Solutions;
        UpdateForeground();
        IsBusy = false;
        RaisePropertyChanged(nameof(IsComputed));

        ShowPopup();
        if (Settings.Default.MongoDB)
            await SaveMongoDB();

        return Tirage.Status;
    }

    #endregion Action
}