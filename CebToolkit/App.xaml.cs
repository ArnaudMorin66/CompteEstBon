namespace CebToolkit {
    public partial class App : Application {
        public App() {
             UserAppTheme = AppTheme.Dark;
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState) => new(new MainPage()){ Title = "Le Compte Est Bon"};
        //protected override Window CreateWindow(IActivationState? activationState) => new(new AppShell()) { Title = "Le Compte Est Bon" };
    }
}
