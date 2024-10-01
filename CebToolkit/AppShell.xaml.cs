using System.Windows.Input;

using CommunityToolkit.Maui.Markup;

namespace CebToolkit;

public partial class AppShell : Shell {
    public Dictionary<string, Type> Routes { get; private set; } = new Dictionary<string, Type>();
    public ICommand HelpCommand => new Command<string>(async (url) => await Launcher.OpenAsync(url));

    public AppShell() {
        RegisterRoutes();
        
         InitializeComponent();
#if WINDOWS
        SetNavBarIsVisible(this,false);
#endif
        BindingContext = this;
    }
    void RegisterRoutes() {
        Routes.Add("Ceb", typeof(CebPage));
        Routes.Add("Config", typeof(ConfigPage));

        foreach (var item in Routes) {
            Routing.RegisterRoute(item.Key, item.Value);
        }
    }

    private void MenuItem_OnClicked(object? sender, EventArgs e) {
        Application.Current?.Quit();
    }
}