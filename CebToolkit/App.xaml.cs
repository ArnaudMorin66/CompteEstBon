using CebToolkit.ViewModel;

namespace CebToolkit;

public partial class App {
    
    public App() {
        Services = ConfigureServices();
        
        UserAppTheme = AppTheme.Dark;
InitializeComponent();

    }

    protected override Window CreateWindow(IActivationState? activationState) => new(new AppShell());
    /// <summary>
    /// Gets the current <see cref="App"/> instance in use
    /// </summary>
    public static new App Current => (App)Application.Current!;

    /// <summary>
    /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
    /// </summary>
    public IServiceProvider Services { get; }

    /// <summary>
    /// Configures the services for the application.
    /// </summary>
    private static IServiceProvider ConfigureServices() {
        var services = new ServiceCollection();

        services.AddSingleton<ViewTirage>();

        return services.BuildServiceProvider();
    }

    public static object? FindResource(string key) => Current.Resources.TryGetValue(key, out object value) ? value : null;
    public static T? FindResource<T>(string key) => Current.Resources.TryGetValue(key, out object value) ? (T)value : default;
}

