using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CebToolkit.ViewModel;

using CommunityToolkit.Maui.Markup;

using Syncfusion.Maui.Buttons;

using static CommunityToolkit.Maui.Markup.GridRowsColumns;


namespace CebToolkit;

public partial class ConfigPage : ContentPage {
    private readonly ViewTirage viewTirage  = App.Current.Services.GetService<ViewTirage>()!;

    public ConfigPage() {
        BindingContext = viewTirage;
        Content = new Grid() {
            RowDefinitions = Rows.Define(Star, Star, Star),
            Children = {
                VueOptionTheme.Row(0),
                VueOptionGrille.Row(1),
                VueOptionAuto.Row(2),
            }
        };
        //InitializeComponent();
    }
    private Grid VueOptionGrille => new() {
        ColumnDefinitions =
            GridRowsColumns.Columns.Define(Star, Star),
        HorizontalOptions = LayoutOptions.Fill,
        VerticalOptions = LayoutOptions.Center,
        Children = {
            new Label()
                .Text("Grille:")
                .Bold()
                .FillHorizontal()
                .End()
                .TextColor(Colors.White)
                .CenterVertical()
                .Column(0),
            new SfSwitch()
                .Bind(SfSwitch.IsOnProperty!, nameof(viewTirage.VueGrille))
                .Column(1)
        }
    };
    private Grid VueOptionTheme => new() {
        ColumnDefinitions = Columns.Define(Star, Star),
        HorizontalOptions = LayoutOptions.Fill,
        VerticalOptions = LayoutOptions.Center,

        Children = {
            new Label()
                .Text("Sombre:")
                .Bold()
                .FillHorizontal()
                .End()
                .TextColor(Colors.White)
                .CenterVertical()
                .Column(0),
            new SfSwitch()
                .Column(1)
                .Bind(SfSwitch.IsOnProperty!, nameof(viewTirage.ThemeDark)).Column(1)
        }
    };


    private Grid VueOptionAuto => new() {
        ColumnDefinitions = Columns.Define(Star, Star),
        HorizontalOptions = LayoutOptions.Fill,
        VerticalOptions = LayoutOptions.Center,

        Children = {
            new Label()
                .Text("Auto:")
                .Bold()
                .FillHorizontal()
                .End()
                .CenterVertical()
                .TextColor(Colors.White)
                .Column(0),
            new SfSwitch()
                .Bind(SfSwitch.IsOnProperty!, nameof(viewTirage.Auto)).Column(1)
        }
    };
    
}