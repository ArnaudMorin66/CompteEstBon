﻿using Syncfusion.Licensing;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace CompteEstBon {
    /// <summary>
    /// Fournit un comportement spécifique à l'application afin de compléter la classe Application par défaut.
    /// </summary>
    sealed partial class App : Application {
        /// <summary>
        /// Initialise l'objet d'application de singleton.  Il s'agit de la première ligne du code créé
        /// à être exécutée. Elle correspond donc à l'équivalent logique de main() ou WinMain().
        /// </summary>
        public App() {
            // ApplicationData.Current.LocalSettings.Values["sflicence"] = "MTA2OTY3QDMxMzcyZTMxMmUzMEN4MmRZRjAzTDQ5cnNvZGlCVWFmRzBmQlVUVGZRRHJuVkxwdG9penFNUE09";
            InitializeComponent();
            Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoqué lorsque l'application est lancée normalement par l'utilisateur final.  D'autres points d'entrée
        /// seront utilisés par exemple au moment du lancement de l'application pour l'ouverture d'un fichier spécifique.
        /// </summary>
        /// <param name="e">Détails concernant la requête et le processus de lancement.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e) {
            await RegisterLicenseAsync();
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;

            // Set active window colors
            // titleBar.ForegroundColor = Windows.UI.Colors.White;
            // titleBar.BackgroundColor = Windows.UI.Colors.Blue;
            titleBar.ButtonForegroundColor = Windows.UI.Colors.White;
            titleBar.ButtonBackgroundColor = Windows.UI.Colors.Transparent;
            titleBar.ButtonHoverForegroundColor = Windows.UI.Colors.Black;
            titleBar.ButtonHoverBackgroundColor = Windows.UI.Colors.Transparent;
            titleBar.ButtonPressedForegroundColor = Windows.UI.Colors.Yellow;
            titleBar.ButtonPressedBackgroundColor = Windows.UI.Colors.Navy;

            // Set inactive window colors
            titleBar.InactiveForegroundColor = Windows.UI.Colors.Gray;
            titleBar.InactiveBackgroundColor = Windows.UI.Colors.SeaGreen;
            titleBar.ButtonInactiveForegroundColor = Windows.UI.Colors.Gray;
            titleBar.ButtonInactiveBackgroundColor = Windows.UI.Colors.SeaGreen;
            Frame rootFrame = Window.Current.Content as Frame;

            // Ne répétez pas l'initialisation de l'application lorsque la fenêtre comporte déjà du contenu,
            // assurez-vous juste que la fenêtre est active
            if (rootFrame == null) {
                // Créez un Frame utilisable comme contexte de navigation et naviguez jusqu'à la première page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated) {
                    //TODO: chargez l'état de l'application précédemment suspendue
                }

                // Placez le frame dans la fenêtre active
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false) {
                if (rootFrame.Content == null) {
                    // Quand la pile de navigation n'est pas restaurée, accédez à la première page,
                    // puis configurez la nouvelle page en transmettant les informations requises en tant que
                    // paramètre
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Vérifiez que la fenêtre actuelle est active
                Window.Current.Activate();
            }
        }

        async Task RegisterLicenseAsync() {
            try {
                var storageFolder =
                    ApplicationData.Current.LocalFolder;
                var licenseFile =
                    await storageFolder.GetFileAsync("SyncfusionLicense.txt");
                SyncfusionLicenseProvider.RegisterLicense(await FileIO.ReadTextAsync(licenseFile));
            } catch (Exception) {
                // ignored
            }
        }
        /// <summary>
        /// Appelé lorsque la navigation vers une page donnée échoue
        /// </summary>
        /// <param name="sender">Frame à l'origine de l'échec de navigation.</param>
        /// <param name="e">Détails relatifs à l'échec de navigation</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e) {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Appelé lorsque l'exécution de l'application est suspendue.  L'état de l'application est enregistré
        /// sans savoir si l'application pourra se fermer ou reprendre sans endommager
        /// le contenu de la mémoire.
        /// </summary>
        /// <param name="sender">Source de la requête de suspension.</param>
        /// <param name="e">Détails de la requête de suspension.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e) {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: enregistrez l'état de l'application et arrêtez toute activité en arrière-plan
            deferral.Complete();
        }
    }
}
