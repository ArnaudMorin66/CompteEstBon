namespace MauiCeb {
	public partial class App : Application {
		public App() {
			Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MzQ2MzY0NkAzMjM2MmUzMDJlMzBQNTJITjhiTlZoS2YrVDFaT3JZTk4xWS9xRUkvUEQ1L01TU2c1T3lnQ2UwPQ==");
			InitializeComponent();
			
		}

		protected override Window CreateWindow(IActivationState? activationState) {
			UserAppTheme = AppTheme.Dark;
			return new Window(new AppShell());
		}
	}
}
