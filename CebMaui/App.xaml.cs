namespace CebMaui {
    public partial class App : Application {
        public App() {
            UserAppTheme = AppTheme.Dark;
            InitializeComponent();

            //MainPage = new MainPage();
            

        }

        protected override Window CreateWindow(IActivationState? activationState) => new(new AppShell()){Title = "Le Compte Est Bon"};
    }
}
