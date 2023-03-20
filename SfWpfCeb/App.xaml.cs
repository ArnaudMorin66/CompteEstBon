//-----------------------------------------------------------------------
// <copyright file="App.xaml.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Markup;

using Syncfusion.Licensing;
using Syncfusion.SfSkinManager;


namespace CompteEstBon;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App {
    public App() {
        SyncfusionLicenseProvider.RegisterLicense(CompteEstBon.Properties.Resources.license);
        SfSkinManager.ApplyStylesOnApplication = true;
    }
    protected override void OnStartup(StartupEventArgs e) {
        CultureInfo vCulture = new("fr-FR");

        Thread.CurrentThread.CurrentCulture = vCulture;
        Thread.CurrentThread.CurrentUICulture = vCulture;
        CultureInfo.DefaultThreadCurrentCulture = vCulture;
        CultureInfo.DefaultThreadCurrentUICulture = vCulture;

        FrameworkElement.LanguageProperty
            .OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
        base.OnStartup(e);
    }
}


