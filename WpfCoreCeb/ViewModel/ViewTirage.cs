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
using CompteEstBon.Properties;
using Microsoft.Win32;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

#endregion

namespace CompteEstBon.ViewModel; 

public class ViewTirage : INotifyPropertyChanged, ICommand {
    private readonly Stopwatch NotifyWatch = new();
    private readonly TimeSpan SolutionTimer = TimeSpan.FromSeconds(10);

    private bool _auto;
    private Color _background;
    private TimeSpan _duree;
    private Color _foreground = Colors.White;
    private bool _isBusy;
    private bool _isUpdating;
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
    ///     Initialisation
    /// </summary>
    /// <returns>
    /// </returns>
    public ViewTirage() {
        Solutions = null;
        // stopwatch = new Stopwatch();

        dateDispatcher = new DispatcherTimer {
            Interval = TimeSpan.FromMilliseconds(10)
        };
        dateDispatcher.Tick += (_, _) => {
         //   if (stopwatch.IsRunning) Duree = stopwatch.Elapsed;
            if (Popup && NotifyWatch.Elapsed > SolutionTimer)
                Popup = false;
            Titre = $"{Result} - {DateTime.Now:dddd dd MMMM yyyy à HH:mm:ss}";
        };
        Plaques.CollectionChanged += (_, e) => {
            if (e.Action != NotifyCollectionChangedAction.Replace)
                return;

            var i = e.NewStartingIndex;
            Tirage.Plaques[i].Value = Plaques[i];
            Task.Run(ClearAsync);
        };
        Tirage.PropertyChanged += (_, args) => {
            if (args.PropertyName == "Clear" &&
                !IsBusy && Auto && Tirage.Status == CebStatus.Valide)
                Task.Run(ResolveAsync);
        };

        Background = ThemeColors["Sombre"];
        Auto = Settings.Default.AutoCalcul;
        _isUpdating = false;
        UpdateData();

        Titre = $"😊 Le compte est bon - {DateTime.Now:dddd dd MMMM yyyy à HH:mm:ss}";
        dateDispatcher.Start();
    }

    public bool Auto {
        get => _auto;
        set {
            if (_auto == value) return;
            _auto = value;

            NotifiedChanged();
            if (_auto && Tirage.Status == CebStatus.Valide) Task.Run(ClearAsync);
        }
    }

    public bool MongoDb {
        get => _mongodb;
        set {
            if (_mongodb == value)
                return;
            _mongodb = value;
            NotifiedChanged();
        }
    }

    //private readonly Storyboard WaitStory =
    //    Application.Current.MainWindow?.FindResource("WaitStoryboard") as Storyboard;
    public static Dictionary<string, Color> ThemeColors { get; } = new() {
        ["DarkSlateGray"] = Colors.DarkSlateGray,
        ["SlateGray"] = Colors.SlateGray,
        ["Blue"] = Color.FromArgb(0xFF, 0x15, 0x25, 0x49),
        ["Black"] = Colors.Black,
        ["DarkBlue"] = Colors.DarkBlue,
        ["Green"] = Colors.Green,
        ["Red"] = Colors.Red,
        ["Yellow"] = Colors.Yellow,
        ["Navy"] = Colors.Navy,
        ["Sombre"] = Color.FromRgb(40,40,40)
    };

    public string Theme {
        get => _theme;
        set {
            if (_theme == value) return;
            _theme = value;
            Background = ThemeColors[value];
            NotifiedChanged();
        }
    }

    public CebTirage Tirage { get; set; } = new();
    public static IEnumerable<int> ListePlaques { get; } = CebPlaque.DistinctPlaques;

    public ObservableCollection<int> Plaques { get; } =
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
            Tirage.Search = value;
            NotifiedChanged();
            Task.Run(ClearAsync);
        }
    }

    public CebStatus Status => Tirage.Status;

    public string Result {
        get => _result;
        set {
            if (value == _result) return;
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
            _isBusy = value;
            NotifiedChanged();
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
            switch (cmd) {
                case "random":
                    await RandomAsync();
                    break;

                case "resolve": {
                    switch (Tirage.Status) {
                        case CebStatus.Valide:
                            if (IsBusy) return;
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
        }
        catch (Exception e) {
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
        Foreground = Tirage.Status switch {
            CebStatus.Valide => Colors.White,
            CebStatus.Invalide => Colors.Red,
            CebStatus.CompteEstBon => Colors.LightYellow,
            CebStatus.CompteApproche => Colors.Orange,
            CebStatus.EnCours => Colors.Yellow,
            _ => Colors.White
        };
    }

    private void NotifiedChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private void ClearData() {
        if (_isUpdating)
            return;
        _isUpdating = true;
        Solution = null;
        Solutions = null;
        Duree = TimeSpan.Zero;

        UpdateForeground();
        Result = Tirage.Status != CebStatus.Invalide ? "Jeu du Compte Est Bon" : "Tirage invalide";
        Popup = false;
        _isUpdating = false;
        NotifiedChanged(nameof(IsComputed));
    }

    private void UpdateData() {
        if (_isUpdating)
            return;
        _isUpdating = true;
        foreach (var (p, i) in Tirage.Plaques.WithIndex())
            Plaques[i] = p.Value;
        NotifiedChanged(nameof(Search));
        //Task.Run(ClearAsync);
        _isUpdating = false;
        ClearData();
    }

    public void ShowNotify(int index = 0) {
        if (index >= 0 && Tirage.Solutions!.Any() && index < Tirage.Count) {
            Solution = Tirage.Solutions.ElementAt(index);
            Popup = true;
        }
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
        if (Auto && Tirage.Status == CebStatus.Valide)
            await ResolveAsync();
    }

    private int _count;

    public int Count {
        get => _count;
        private set {
            if (_count == value) return;
            _count = value;
            NotifiedChanged();
        }
    }

    public async Task ResolveAsync() {
        IsBusy = true;
        Result = "Calcul...";
        await Tirage.ResolveAsync();
        Result = Tirage.Status switch {
            CebStatus.CompteEstBon => "😊 Compte est bon",
            CebStatus.CompteApproche => $"😢 Compte approché: {Tirage.Found}, écart: {Tirage.Diff}",
            CebStatus.Invalide => "Tirage invalide",
            _ => "Le Compte est Bon"
        };
        Duree = Tirage.Duree;
        UpdateForeground();

        Solution = Tirage.Solutions![0];
        
        Solutions = Tirage.Solutions; // .Select(CebDetail.FromCebBase);
        if (MongoDb)
            await SaveToMongoDB();
        IsBusy = false;

        ShowNotify();
        // ReSharper disable once ExplicitCallerInfoArgument
        NotifiedChanged(nameof(Status));
        NotifiedChanged(nameof(IsComputed));
    }

    public async Task SaveToMongoDB() {
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
                    .AddRange(Tirage.Result.ToBsonDocument()));
        }
        catch (Exception) {
            // ignored
        }
    }

    public static (bool select, string name) FileSaveName() {
        SaveFileDialog dialog = new() {
            DefaultExt = ".xlsx"
        };
        (dialog.Filter, dialog.Title) = ("Document Excel|*.xlsx|Document Word|*.docx", "Export Excel-Word");
        dialog.InitialDirectory = Environment.SpecialFolder.UserProfile + "\\Downloads";
        // ReSharper disable once PossibleInvalidOperationException
        return (bool)dialog.ShowDialog() ? (true, dialog.FileName) : (false, null);
    }

    public void ExportFichier() {
        var (Ok, Path) = FileSaveName();
        if (!Ok) return;
        FileInfo fi = new(Path);
        if (fi.Exists)
            fi.Delete();

        Action<Stream> ExportFunction = fi.Extension switch {
            ".xlsx" => Tirage.ToExcel,
            ".docx" => Tirage.ToWord,
            _ => throw new NotImplementedException()
        };
        var stream = new FileStream(Path!, FileMode.CreateNew);
        ExportFunction(stream);
        stream.Flush();
        stream.Close();
        OpenDocument(Path);
    }

    public static void OpenDocument(string nom) =>
        Process.Start(new ProcessStartInfo {
            UseShellExecute = true,
            FileName = nom
        });

    #endregion Action
}