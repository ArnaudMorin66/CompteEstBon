#region using

using CebToolkit.ViewModel;

using CommunityToolkit.Maui.Converters;
using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Maui.Markup.LeftToRight;

using CompteEstBon;

using Syncfusion.Maui.Core;
using Syncfusion.Maui.DataGrid;
using Syncfusion.Maui.Inputs;
using Syncfusion.Maui.Inputs.DropDownControls;
using Syncfusion.Maui.Popup;

using static CommunityToolkit.Maui.Markup.GridRowsColumns;

using SelectionMode = Microsoft.Maui.Controls.SelectionMode;

// ReSharper disable InvalidXmlDocComment

#endregion

namespace CebToolkit;

/// <summary>
///     Represents a custom page within the Compte est bon application.
/// </summary>
/// <remarks>
///     The <see cref="CebToolkit.CebPage" /> class is a specialized page that inherits from <see cref="ContentPage" />.
///     It initializes the binding context, context flyout, content, and theme color binding for the page.
/// </remarks>
public class CebPage : ContentPage {
    /// <summary>
    ///     An instance of <see cref="CommunityToolkit.Maui.Converters.InvertedBoolConverter" /> used to invert boolean values
    ///     in bindings.
    /// </summary>
    /// <remarks>
    ///     This converter is primarily used to bind properties that require the opposite boolean value of the source property.
    /// </remarks>
    private static readonly InvertedBoolConverter InvertedBoolConverter = new();

    private static readonly FuncConverter<bool, NumericEntryUpDownPlacementMode> UpDownConverter =
        new(b =>
            b ? NumericEntryUpDownPlacementMode.Hidden : NumericEntryUpDownPlacementMode.InlineVertical);

    /// <summary>
    ///     Represents the view model for the current page, providing commands and properties for the UI.
    /// </summary>
    /// <remarks>
    ///     This field is initialized using the dependency injection container and is used as the binding context for the page.
    /// </remarks>
    private static readonly ViewTirage BindTirage = App.Current.Services.GetService<ViewTirage>()!;


    /// <summary>
    ///     Initializes a new instance of the <see cref="CebToolkit.CebPage" /> class.
    /// </summary>
    /// <remarks>
    ///     This constructor sets up the binding context, context flyout, content, and theme color binding for the page.
    /// </remarks>
    public CebPage() {
        BindingContext = BindTirage;
        FlyoutBase.SetContextFlyout(this, MenuContext);
        Content = CebPage.MainStackLayout;
        this.AppThemeColorBinding(BackgroundColorProperty, AppShell.BackgroundLight, AppShell.BackgroundDark);
    }

    /// <summary>
    ///     Gets a view that displays the solutions in a grid format.
    /// </summary>
    /// <value>
    ///     A <see cref="View" /> that contains a grid with solutions data, a collection view, and an indicator.
    /// </value>
    private static View VueSolutions => new Grid {
        RowDefinitions = Rows.Define(Auto),
        Children = {
            VueDataGridSolutions,
            VueCollectionSolutions,
            CebPage.Vueindicator
        }
    };

    /// <summary>
    ///     Gets a view that represents the input section of the page.
    /// </summary>
    /// <remarks>
    ///     This view is constructed as a grid with different layouts for Windows and other platforms.
    ///     It includes a numeric entry for user input, which is bound to the "Tirage.Search" property.
    /// </remarks>
    private static View VueSaisie {
        get {
            var result = new Grid();
            if (DeviceInfo.Idiom != DeviceIdiom.Phone)
                result.ColumnDefinitions = Columns.Define(
                    Star, Star, Star, Star, Star, Star, Star);
            else
                result.RowDefinitions = Rows.Define(
                    Star, Star, Star, Star);

            result.Children.Add(DeviceInfo.Idiom != DeviceIdiom.Phone
                ? CebPage.VuePlaques.Column(0).ColumnSpan(6)
                : CebPage.VuePlaques.Row(0).RowSpan(3));
            var num = new SfNumericEntry {
                    CustomFormat = "000",
                    Maximum = 999,
                    Minimum = 100
                }
                .AppThemeBinding(SfNumericEntry.ShowBorderProperty, true, false)
                .Bind(SfNumericEntry.UpDownPlacementModeProperty, nameof(BindTirage.Auto), converter: UpDownConverter)
                .Bind(SfNumericEntry.ValueProperty, "Tirage.Search");
            if (DeviceInfo.Idiom != DeviceIdiom.Phone)
                num.Column(6);
            else
                num.Row(4);
            result.Children.Add(num);
            result.CenterVertical();
            return result;
        }
    }

    // ReSharper disable once UnusedMember.Local
    /// <summary>
    ///     Gets a <see cref="View" /> that contains action buttons for resolving and randomizing commands.
    /// </summary>
    /// <value>
    ///     A <see cref="Grid" /> containing two buttons: one for resolving and one for randomizing.
    /// </value>
    private static View VueAction => new Grid {
        ColumnDefinitions = Columns.Define(Star, Stars(2)),
        Children = {
            new Button {
                ImageSource = ImageSource.FromFile("random.png"),
                ContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Left, 10),
                Command = BindTirage.RandomCommand,
                Text = "Hasard"
            }.Column(0),
            new Button {
                ImageSource = ImageSource.FromFile("resolve.png"),
                ContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Left, 10),
                Command = BindTirage.ResolveCommand,
                Text = "Résoudre"
            }.Column(1)
        }
    };


    /// <summary>
    ///     Gets a <see cref="CollectionView" /> that displays a collection of solutions.
    /// </summary>
    /// <remarks>
    ///     This view is bound to the "Tirage.Solutions" property and is visible based on the "VueGrille" property of the
    ///     <see cref="ViewModel.ViewTirage" /> view model.
    /// </remarks>
    private static View VueCollectionSolutions => new CollectionView {
            SelectionMode = SelectionMode.Single,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Default,
            VerticalScrollBarVisibility = ScrollBarVisibility.Default,
            HeightRequest = 500,
            HorizontalOptions = LayoutOptions.Fill,
            ItemsLayout = new GridItemsLayout(4, ItemsLayoutOrientation.Vertical) {
                HorizontalItemSpacing = 2,
                VerticalItemSpacing = 2,
                SnapPointsAlignment = SnapPointsAlignment.Center,
                SnapPointsType = SnapPointsType.MandatorySingle
            },
            ItemTemplate = new DataTemplate(() => VueSolutionsDetail)
        }
        .Bind(IsVisibleProperty, nameof(BindTirage.VueGrille))
        .Bind(ItemsView.ItemsSourceProperty, "Tirage.Solutions")
        .AppThemeColorBinding(BackgroundColorProperty, AppShell.BackgroundLight,
            AppShell.BackgroundDark) //"{AppThemeBinding Dark=DarkSlateGrey, Light=#8fbc8f}"
        .Invoke(l => {
            l.SelectionChanged += (sender, _) => {
                if (sender is CollectionView { SelectedItem: CebBase sol }) BindTirage.ShowPopup(sol);
            };
        });

    /// <summary>
    ///     Gets a detailed view of the solutions.
    /// </summary>
    /// <value>
    ///     A <see cref="View" /> that represents the detailed view of the solutions.
    /// </value>
    private static View VueSolutionsDetail => new CollectionView {
            Margin = 1,
            MinimumHeightRequest = 100,
            HorizontalOptions = LayoutOptions.Start,
            SelectionMode = SelectionMode.Single,
            VerticalScrollBarVisibility = ScrollBarVisibility.Never,
            VerticalOptions = LayoutOptions.Start,
            IsEnabled = false,
            ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical)
        }
        .Bind(ItemsView.ItemsSourceProperty, "Operations")
        .AppThemeColorBinding(BackgroundColorProperty, AppShell.BackgroundLight, AppShell.BackgroundDark)
        .Center()
        .Invoke(collection => {
            collection.SelectionChanged += (sender, _) => {
                if (sender is CollectionView { BindingContext: CebBase sol }) BindTirage.ShowPopup(sol);
            };
        });

    /// <summary>
    ///     Gets the data grid view for displaying solutions.
    /// </summary>
    /// <remarks>
    ///     This property initializes and configures an instance of <see cref="Syncfusion.Maui.DataGrid.SfDataGrid" />
    ///     to display solutions with specific column mappings and bindings.
    /// </remarks>
    private static SfDataGrid VueDataGridSolutions => new SfDataGrid {
            VerticalScrollBarVisibility = ScrollBarVisibility.Always,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Always,
            ShowRowHeader = false,
            HeaderRowHeight = 0,
            HeightRequest = 500,
            Columns = [
                new DataGridTextColumn {
                    MinimumWidth = 120,
                    MappingName = "Op1"
                },
                new DataGridTextColumn {
                    MinimumWidth = 120,
                    MappingName = "Op2"
                },
                new DataGridTextColumn {
                    MappingName = "Op3",
                    MinimumWidth = 120
                },
                new DataGridTextColumn {
                    MinimumWidth = 120,
                    MappingName = "Op4"
                },
                new DataGridTextColumn {
                    MappingName = "Op5",
                    MinimumWidth = 120
                }
            ]
        }
        .Bind(IsVisibleProperty, nameof(BindTirage.VueGrille), BindingMode.Default, InvertedBoolConverter)
        .Bind(SfDataGrid.ItemsSourceProperty, "Tirage.Solutions")
        .DynamicResource(StyleProperty, "DatagridSolutionsStyle")
        .Invoke(datagrid => {
            datagrid.SelectionChanged += (sender, _) => {
                if (sender is SfDataGrid { SelectedRow: CebBase sol }) BindTirage.ShowPopup(sol);
            };
        });

    /// <summary>
    ///     Gets a view that displays a busy indicator.
    /// </summary>
    /// <remarks>
    ///     The busy indicator is displayed when the <see cref="CebToolkit.ViewModel.ViewTirage.IsBusy" /> property is set to
    ///     <c>true</c>.
    ///     The indicator uses a circular material animation and is colored blue.
    /// </remarks>
    private static View Vueindicator => Borderize(new SfBusyIndicator {
                IndicatorColor = Colors.Blue,
                AnimationType = AnimationType.CircularMaterial,
                ZIndex = 99
            }
            .Bind(SfBusyIndicator.IsRunningProperty, nameof(BindTirage.IsBusy)))
        .Bind(IsVisibleProperty, nameof(BindTirage.IsBusy));

    /// <summary>
    ///     Gets the popup view used to display results and related information.
    /// </summary>
    /// <value>
    ///     A <see cref="View" /> representing the popup, which includes various UI elements such as labels, box views, and a
    ///     collection view.
    /// </value>
    /// <remarks>
    ///     The popup is configured with specific styles and bindings to display the results of a "tirage" operation, including
    ///     the found result,
    ///     the number of solutions, and the elapsed time.
    /// </remarks>
    private static View VuePopup => new SfPopup {
            Margin = 0,
            Padding = 0,
            AutoSizeMode = PopupAutoSizeMode.Height,
            WidthRequest = 400,
            ShowFooter = false,
            ShowHeader = false,
            AnimationDuration = 1,
            AnimationMode = PopupAnimationMode.SlideOnBottom,
            AnimationEasing = PopupAnimationEasing.SinOut,
            VerticalOptions = LayoutOptions.Fill,
            PopupStyle =
                new PopupStyle {
                    CornerRadius = 5,
                    BlurIntensity = PopupBlurIntensity.Dark
                }.Bind(PopupStyle.StrokeProperty,
                    nameof(BindTirage.Foreground)),
            ContentTemplate = new DataTemplate(() => Borderize(new VerticalStackLayout {
                Margin = 2,
                HorizontalOptions = LayoutOptions.Center,
                Spacing = 0.5,
                Children = {
                    VueHeaderPopup,
                    CebPage.VueSeparator,
                    VueDetailPopup,
                    CebPage.VueSeparator,
                    VueFooterPopup
                }
            }))
        }
        .Bind(SfPopup.IsOpenProperty, nameof(BindTirage.Popup));

    /// <summary>
    ///     Gets the view that represents the header of the popup in the CebPage.
    /// </summary>
    /// <value>
    ///     A <see cref="View" /> representing the header of the popup.
    /// </value>
    private static View VueHeaderPopup => new Label()
        .FontSize(24)
        .CenterHorizontal()
        .CenterVertical()
        .Bind(Label.TextProperty, "Result")
        .Bind(Label.TextColorProperty, nameof(BindTirage.Foreground));

    /// <summary>
    ///     Gets a detailed view of the solutions.
    /// </summary>
    /// <value>
    ///     A <see cref="View" /> that represents the detailed view of the solutions.
    /// </value>
    private static View VueDetailPopup => new CollectionView {
            Margin = 2,
            VerticalOptions = LayoutOptions.Center,
            ItemSizingStrategy = ItemSizingStrategy.MeasureAllItems,
            ItemTemplate = new DataTemplate(
                () => new Label {
                        DisableLayout = false,
                        HorizontalTextAlignment = TextAlignment.Center
                    }
                    .Bind(Label.TextProperty))
        }
        .Bind(ItemsView.ItemsSourceProperty, "Solution.Operations")
        .AppThemeColorBinding(BackgroundColorProperty, AppShell.BackgroundLight, AppShell.BackgroundDark)
        .Center();

    private static View VueSeparator => new BoxView {
        HeightRequest = 2,
        CornerRadius = 0
    }.Bind(BoxView.ColorProperty,
        nameof(BindTirage.Foreground));

    private static View VueFooterPopup => new Grid {
        ColumnDefinitions = Columns.Define(Stars(5), Stars(4)),
        RowDefinitions = Rows.Define(Star, Star, Star),
        Children = {
            new Label().Text("Trouvé:")
                .TextRight()
                .Margin(2)
                .FontSize(15)
                .Row(0).Column(0),
            new Label()
                .Margin(2)
                .TextRight()
                .Bind(Label.TextProperty, "Tirage.Found")
                .Bind(Label.TextColorProperty, nameof(BindTirage.Foreground))
                .Row(0).Column(1),

            new Label().Text("Nombre de solutions:")
                .TextRight()
                .Margin(2)
                .FontSize(15)
                .Row(1).Column(0),

            new Label()
                .Margin(2)
                .TextRight()
                .Bind(Label.TextProperty, "Tirage.Count")
                .Bind(Label.TextColorProperty, nameof(BindTirage.Foreground))
                .Row(1).Column(1),

            new Label().Text("Durée:")
                .TextRight()
                .Margin(2)
                .FontSize(15)
                .Row(2).Column(0),

            new Label()
                .Margin(2)
                .TextRight()
                .Bind(Label.TextProperty, nameof(BindTirage.ElapsedTime), stringFormat: "{0:hh\\:mm\\:ss\\.fff}")
                .Bind(Label.TextColorProperty, nameof(BindTirage.Foreground))
                .Row(2).Column(1)
        }
    };

    /// <summary>
    ///     Gets a view that displays the result of the computation, including the result value,
    ///     whether the result was found, the number of solutions, and the duration of the computation.
    /// </summary>
    /// <value>
    ///     A <see cref="View" /> that contains labels bound to the properties of the <see cref="ViewModel.ViewTirage" /> view
    ///     model.
    /// </value>
    private static View VueResultat => new Grid {
            HeightRequest = 40,
            ColumnDefinitions = DeviceInfo.Idiom == DeviceIdiom.Phone
                ? Columns.Define(Star, Star)
                : Columns.Define(Star, Star, Star, Star),
            RowDefinitions = DeviceInfo.Idiom == DeviceIdiom.Phone ? Rows.Define(Star, Star) : Rows.Define(Star),
            VerticalOptions = LayoutOptions.Center,
            Children = {
                new Label()
                    .CenterVertical()
                    .Bold()
                    .Bind(Label.TextProperty, nameof(BindTirage.Result))
                    .Bind(Label.TextColorProperty, nameof(BindTirage.Foreground))
                    .Bind(IsVisibleProperty, nameof(BindTirage.IsComputed))
                    .Row(0)
                    .Column(0),
                new Label()
                    .Bold()
                    .CenterVertical()
                    .Bind(Label.TextProperty, "Tirage.Found", stringFormat: "Trouvé: {0}")
                    .Bind(Label.TextColorProperty, nameof(BindTirage.Foreground))
                    .Bind(IsVisibleProperty, nameof(BindTirage.IsComputed))
                    .Row(0)
                    .Column(1),
                new Label()
                    .Bold()
                    .CenterVertical()
                    .Bind(Label.TextProperty, "Tirage.Count", stringFormat: "Nombre de solutions: {0}")
                    .Bind(Label.TextColorProperty, nameof(BindTirage.Foreground))
                    .Column(DeviceInfo.Idiom == DeviceIdiom.Phone ? 0 : 2)
                    .Row(DeviceInfo.Idiom == DeviceIdiom.Phone ? 1 : 0)
                    .Bind(IsVisibleProperty, nameof(BindTirage.IsComputed)),
                new Label()
                    .CenterVertical()
                    .Bold()
                    .Bind(Label.TextProperty, nameof(BindTirage.ElapsedTime),
                        stringFormat: "Durée: {0:hh\\:mm\\:ss\\.fff}")
                    .Bind(Label.TextColorProperty, nameof(BindTirage.Foreground))
                    .Column(DeviceInfo.Idiom == DeviceIdiom.Phone ? 1 : 3)
                    .Row(DeviceInfo.Idiom == DeviceIdiom.Phone ? 1 : 0)
            }
        }
        .AppThemeColorBinding(BackgroundColorProperty, AppShell.BackgroundLight, AppShell.BackgroundDark);


    /// <summary>
    ///     Gets the main stack layout of the page.
    /// </summary>
    /// <value>
    ///     A <see cref="VerticalStackLayout" /> representing the main stack layout of the page.
    /// </value>
    private static VerticalStackLayout MainStackLayout {
        get {
            VerticalStackLayout verticalStackLayout = [
                Borderize(CebPage.VueSaisie, true), Borderize(VueResultat, true), CebPage.VueSolutions, CebPage.VuePopup
            ];
            if (DeviceInfo.Idiom != DeviceIdiom.Phone) verticalStackLayout.Insert(1, Borderize(CebPage.VueAction, true));
            return verticalStackLayout;
        }
    }

    /// <summary>
    ///     Gets a <see cref="Grid" /> representing the view for displaying plaques.
    /// </summary>
    /// <remarks>
    ///     The layout of the grid varies depending on the platform.
    ///     On Windows and MacCatalyst, it defines six columns.
    ///     On other platforms, it defines three rows and two columns.
    /// </remarks>
    /// <returns>A <see cref="Grid" /> representing the view for displaying plaques.</returns>
    private static Grid VuePlaques {
        get {
            var grid = new Grid();
            if (DeviceInfo.Idiom != DeviceIdiom.Phone) {
                grid.ColumnDefinitions = Columns.Define(Star, Star, Star, Star, Star, Star);
            } else {
                grid.RowDefinitions = Rows.Define(Star, Star, Star);
                grid.ColumnDefinitions = Columns.Define(Star, Star);
            }

            AddPlaquesToGrid(ref grid);
            return grid;
        }
    }


    /// <summary>
    ///     Gets the context menu for the page, providing various commands and options for user interaction.
    /// </summary>
    /// <value>
    ///     A <see cref="MenuFlyout" /> containing menu items and sub-items with associated commands and keyboard shortcuts.
    /// </value>
    private static MenuFlyout MenuContext => [
        new MenuFlyoutItem {
                KeyboardAccelerators = {
                    new KeyboardAccelerator {
                        Key = "H",
                        Modifiers = KeyboardAcceleratorModifiers.Ctrl
                    }
                }
            }
            .Text("Hasard")
            .BindCommand(nameof(BindTirage.RandomCommand)),
        new MenuFlyoutItem {
                KeyboardAccelerators = {
                    new KeyboardAccelerator {
                        Key = "R",
                        Modifiers = KeyboardAcceleratorModifiers.Ctrl
                    }
                }
            }
            .BindCommand(nameof(BindTirage.ResolveCommand))
            .Text("Résoudre"),
        new MenuFlyoutSeparator(), new MenuFlyoutSubItem {
            new MenuFlyoutItem {
                    KeyboardAccelerators = {
                        new KeyboardAccelerator {
                            Key = "X",
                            Modifiers = KeyboardAcceleratorModifiers.Ctrl | KeyboardAcceleratorModifiers.Shift
                        }
                    }
                }
                .Text("Excel")
                .BindCommand(nameof(BindTirage.ExportCommand), parameterSource: "excel"),
            new MenuFlyoutItem {
                    KeyboardAccelerators = {
                        new KeyboardAccelerator {
                            Key = "W",
                            Modifiers = KeyboardAcceleratorModifiers.Ctrl | KeyboardAcceleratorModifiers.Shift
                        }
                    }
                }
                .Text("Word")
                .BindCommand(nameof(BindTirage.ExportCommand), parameterSource: "word"),
            new MenuFlyoutItem {
                    KeyboardAccelerators = {
                        new KeyboardAccelerator {
                            Key = "J",
                            Modifiers = KeyboardAcceleratorModifiers.Ctrl | KeyboardAcceleratorModifiers.Shift
                        }
                    }
                }
                .Text("Json")
                .BindCommand(nameof(BindTirage.ExportCommand), parameterSource: "json"),
            new MenuFlyoutItem {
                    KeyboardAccelerators = {
                        new KeyboardAccelerator {
                            Key = "M",
                            Modifiers = KeyboardAcceleratorModifiers.Ctrl | KeyboardAcceleratorModifiers.Shift
                        }
                    }
                }
                .Text("Xml")
                .BindCommand(nameof(BindTirage.ExportCommand), parameterSource: "xml"),
            new MenuFlyoutItem {
                    KeyboardAccelerators = {
                        new KeyboardAccelerator {
                            Key = "H",
                            Modifiers = KeyboardAcceleratorModifiers.Ctrl | KeyboardAcceleratorModifiers.Shift
                        }
                    }
                }
                .Text("HTML")
                .BindCommand(nameof(BindTirage.ExportCommand), parameterSource: "html")
        }.Text("Export"),
        new MenuFlyoutItem {
                KeyboardAccelerators = {
                    new KeyboardAccelerator {
                        Key = "V",
                        Modifiers = KeyboardAcceleratorModifiers.Ctrl
                    }
                }
            }
            .Text("Vue").BindCommand(nameof(BindTirage.UpdateProprieteCommand), parameterSource: "vue"),
        new MenuFlyoutItem {
            KeyboardAccelerators = {
                new KeyboardAccelerator {
                    Key = "T",
                    Modifiers = KeyboardAcceleratorModifiers.Ctrl
                }
            }
        }.Text("Thème").BindCommand(nameof(BindTirage.UpdateProprieteCommand), parameterSource: "theme"),
        new MenuFlyoutItem {
            KeyboardAccelerators = {
                new KeyboardAccelerator {
                    Key = "A",
                    Modifiers = KeyboardAcceleratorModifiers.Ctrl
                }
            }
        }.Text("Auto").BindCommand(nameof(BindTirage.UpdateProprieteCommand), parameterSource: "auto"),
        new MenuFlyoutSeparator(), new MenuFlyoutItem {
                KeyboardAccelerators = {
                    new KeyboardAccelerator {
                        Key = "Q",
                        Modifiers = KeyboardAcceleratorModifiers.Alt
                    }
                }
            }
            .Text("Quitter").BindCommand(nameof(BindTirage.QuitterCommand))
    ];

    /// <summary>
    ///     Adds the plaques to the grid.
    /// </summary>
    /// <param name="grid">The grid to add the plaques to.</param>
    private static void AddPlaquesToGrid(ref Grid grid) {
        for (var i = 0; i < 6; i++) {
            var comboBox = PlaqueComboBox(i);
            PositionComboBoxInGrid(ref comboBox, i);
            grid.Children.Add(comboBox);
        }
    }

    /// <summary>
    ///     Positions the <see cref="SfComboBox" /> in the grid based on the index.
    /// </summary>
    /// <param name="comboBox">The <see cref="SfComboBox" /> to position.</param>
    /// <param name="index">The index of the plaque.</param>
    private static void PositionComboBoxInGrid(ref SfComboBox comboBox, int index) {
        if (DeviceInfo.Idiom != DeviceIdiom.Phone)
            comboBox.Column(index);
        else
            comboBox.Column(index % 2).Row(index / 2);
    }

    /// <summary>
    ///     Creates a <see cref="SfComboBox" /> for a plaque at the specified index.
    /// </summary>
    /// <param name="index">The index of the plaque.</param>
    /// <returns>A <see cref="SfComboBox" /> for the plaque.</returns>
    private static SfComboBox PlaqueComboBox(int index) => new SfComboBox {
            ItemsSource = CebPlaque.DistinctPlaques
        }
        .Bind(DropDownListBase.TextProperty, $"Tirage.Plaques[{index}].Value", BindingMode.TwoWay);

    /// <summary>
    ///     Creates a bordered view with optional shadow effect.
    /// </summary>
    /// <param name="content">The content to be wrapped within the border.</param>
    /// <param name="isShadow">Indicates whether the border should have a shadow effect. Default is false.</param>
    /// <returns>A <see cref="Border" /> element containing the specified content.</returns>
    private static Border Borderize(View content, bool isShadow = false) => new Border {
            StrokeThickness = 0.5,
            Content = content
        }
        .Margin(1)
        .Bind(IsEnabledProperty,
            nameof(BindTirage.IsBusy), BindingMode.OneWay, InvertedBoolConverter)
        .Bind(Border.StrokeProperty, nameof(BindTirage.Foreground))
        .Invoke(b => {
            if (isShadow) b.DynamicResource(StyleProperty, "BorderWithShadowStyle");
        });
}