using CompteEstBon;
using Syncfusion.SfSkinManager;
using Syncfusion.UI.Xaml.Grid.Converter;
using Syncfusion.Windows.Shared;
using Syncfusion.XlsIO;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace WpfCeb {    
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ChromelessWindow {

        public MainWindow() {
            InitializeComponent();
            SfSkinManager.SetVisualStyle(this, VisualStyles.Blend);
            
         }

        private void ChromelessWindow_Initialized(object sender, EventArgs e) {
            bindTirage.HasardCommand.Execute(null);
            InvalidateVisual();
        }
    }
}
