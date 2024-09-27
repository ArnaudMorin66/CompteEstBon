using System.Globalization;

namespace CebToolkit;

public partial class App {
    public App() {
        UserAppTheme = AppTheme.Dark;
        InitializeComponent();
        CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");
    }

    protected override Window CreateWindow(IActivationState? activationState) => new(new AppShell()) { };
}