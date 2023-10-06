
namespace CebBlazor.Maui;

public partial class App : Application {
    public App() {
        InitializeComponent();

        MainPage = new MainPage();
    }
    protected override Window CreateWindow(IActivationState activationState) {
        var window = base.CreateWindow(activationState);
        if (DeviceInfo.Platform == DevicePlatform.WinUI) {
            window.Width = 1024;
            window.Height = 800;
        }
        return window;
    } 
}
