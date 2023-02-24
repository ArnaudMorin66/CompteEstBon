//-----------------------------------------------------------------------
// <copyright file="App.xaml.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.Windows;

using Syncfusion.Licensing;
using Syncfusion.SfSkinManager;


namespace CompteEstBon;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application {
    public App() {
        SyncfusionLicenseProvider.RegisterLicense(CompteEstBon.Properties.Settings.Default.SfLicence);
        ExportOffice.RegisterLicense(CompteEstBon.Properties.Settings.Default.SfLicence);
        SfSkinManager.ApplyStylesOnApplication = true;
    }
}


