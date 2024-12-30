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

/// <summary>
/// Represents the configuration page of the application.
/// </summary>
public partial class ConfigPage : ContentPage {
    private readonly ViewTirage viewTirage = App.Current.Services.GetService<ViewTirage>()!;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigPage"/> class.
    /// </summary>
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

    /// <summary>
    /// Gets the view that represents the grille option in the configuration page.
    /// </summary>
    /// <remarks>
    /// This view includes a label for displaying the "Grille" text, a switch for toggling the grille option,
    /// and sets the column definitions.
    /// </remarks>
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
                    .CenterVertical()
                    .Column(0),
                new SfSwitch()
                    .Bind(SfSwitch.IsOnProperty!, nameof(viewTirage.VueGrille))
                    .Column(1)
            }
    };

    /// <summary>
    /// Gets the view that represents the theme option in the configuration page.
    /// </summary>
    /// <remarks>
    /// This view includes a label for displaying the "Sombre" text, a switch for toggling the theme,
    /// and sets the column definitions.
    /// </remarks>
    private Grid VueOptionTheme => new() {
        ColumnDefinitions = Columns.Define(Star, Star),
        Children = {
                new Label()
                    .Text("Sombre:")
                    .Bold()
                    .FillHorizontal()
                    .End()
                    .CenterVertical()
                    .Column(0),
                new SfSwitch()
                    .Bind(SfSwitch.IsOnProperty!, nameof(viewTirage.ThemeDark))
                    .Column(1)
            }
    };

    /// <summary>
    /// Gets the view that represents the auto option in the configuration page.
    /// </summary>
    /// <remarks>
    /// This view includes a label for displaying the "Auto" text, a switch for toggling the auto option,
    /// and sets the column definitions.
    /// </remarks>
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
                    .Column(0),
                new SfSwitch()
                    .Bind(SfSwitch.IsOnProperty!, nameof(viewTirage.Auto))
                    .Column(1)
            }
    };
}
