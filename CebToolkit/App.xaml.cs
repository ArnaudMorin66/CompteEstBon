using CebToolkit.ViewModel;

namespace CebToolkit;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App {

    /// <summary>
    /// Initializes the singleton application object. This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App() {
        Services = ConfigureServices();

        UserAppTheme = AppTheme.Dark;
        InitializeComponent();
    }

    /// <summary>
    /// Creates the main application window.
    /// </summary>
    /// <param name="activationState">The activation state.</param>
    /// <returns>The main application window.</returns>
    protected override Window CreateWindow(IActivationState? activationState) => new(new AppShell());

    /// <summary>
    /// Gets the current <see cref="App"/> instance in use.
    /// </summary>
    public static new App Current => (App)Application.Current!;

    /// <summary>
    /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
    /// </summary>
    public IServiceProvider Services { get; }

    /// <summary>
    /// Configures the services for the application.
    /// </summary>
    /// <returns>The configured <see cref="IServiceProvider"/>.</returns>
    private static IServiceProvider ConfigureServices() {
        var services = new ServiceCollection();

        services.AddSingleton<ViewTirage>();

        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Finds a resource with the specified key.
    /// </summary>
    /// <param name="key">The key of the resource to find.</param>
    /// <returns>The resource if found; otherwise, null.</returns>
    public static object? FindResource(string key) => Current.Resources.TryGetValue(key, out object value) ? value : null;

    /// <summary>
    /// Finds a resource with the specified key and casts it to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to cast the resource to.</typeparam>
    /// <param name="key">The key of the resource to find.</param>
    /// <returns>The resource if found and cast successfully; otherwise, the default value of <typeparamref name="T"/>.</returns>
    public static T? FindResource<T>(string key) => Current.Resources.TryGetValue(key, out object value) ? (T)value : default;
}

