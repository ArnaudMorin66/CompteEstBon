namespace CebMaui {
    public partial class App : Application {
        public App() {
            UserAppTheme = AppTheme.Dark;
            InitializeComponent();

            //MainPage = new MainPage();
            MainPage = new AppShell();

        }
    }
}
