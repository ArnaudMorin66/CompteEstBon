using System.ComponentModel;

namespace CebMaui;

public partial class AppShell : Shell, INotifyPropertyChanged {
    
    public AppShell()
    {
        InitializeComponent();
        
        Title = $"Le compte est bon";
    }

}