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
using Syncfusion.Maui.ListView;
using Syncfusion.Maui.Popup;

using static CommunityToolkit.Maui.Markup.GridRowsColumns;

using SelectionMode = Microsoft.Maui.Controls.SelectionMode;
// ReSharper disable InvalidXmlDocComment

#endregion

namespace CebToolkit;

public class CebPage : ContentPage {
    private readonly InvertedBoolConverter invertedBoolConverter = new();

    private readonly ViewTirage viewTirage = App.Current.Services.GetService<ViewTirage>()!;
    public Color BackgroundDark = Color.FromArgb("1E1F1C");
    public Color BackgroundLight = Color.FromArgb("8fbc8f");

    /// <summary>
    ///     Initializes a new instance of the <see cref="CebToolkit.CebPage" /> class.
    /// </summary>
    /// <remarks>
    ///     This constructor sets up the binding context, context flyout, content, and theme color binding for the page.
    /// </remarks>
    public CebPage() {
        if (App.Current.Resources.TryGetValue("BackgroundDark", out var value)) BackgroundDark = ((Color?)value)!;

        if (App.Current.Resources.TryGetValue("BackgroundLight", out value)) BackgroundLight = ((Color?)value)!;
        BindingContext = viewTirage;
        FlyoutBase.SetContextFlyout(this, MenuContext);
        //Content = MainScrollView;
        Content = MainStackLayout;
        this.AppThemeColorBinding(BackgroundColorProperty, BackgroundLight, BackgroundDark);
    }

    /// <summary>
    ///     Gets a view that displays the solutions in a grid format.
    /// </summary>
    /// <value>
    ///     A <see cref="View" /> that contains a grid with solutions data, a collection view, and an indicator.
    /// </value>
    private View VueSolutions => new Grid {
        RowDefinitions = Rows.Define(Auto),
        Children = {
            VueDataGridSolutions,
            VueGollectionSolutions,
            Vueindicator
        }
    };

    /// <summary>
    ///     Gets a view that represents the input section of the page.
    /// </summary>
    /// <remarks>
    ///     This view is constructed as a grid with different layouts for Windows and other platforms.
    ///     It includes a numeric entry for user input, which is bound to the "Tirage.Search" property.
    /// </remarks>
    private View VueSaisie {
        get {
            var result = new Grid();
            if (DeviceInfo.Idiom != DeviceIdiom.Phone)
                result.ColumnDefinitions = Columns.Define(
                    Star, Auto, Auto, Auto, Auto, Auto, Auto);
            else
                result.RowDefinitions = Rows.Define(
                    Star, Auto, Auto, Auto);

            result.Children.Add(DeviceInfo.Idiom != DeviceIdiom.Phone
                ? VuePlaques.Column(0).ColumnSpan(6)
                : VuePlaques.Row(0).RowSpan(3));
            var num = new SfNumericEntry {
                    CustomFormat = "000",
                    Maximum = 999,
                    Minimum = 100
                }
                .AppThemeBinding(SfNumericEntry.ShowBorderProperty, true, false)
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
    private View VueAction => new Grid {
        ColumnDefinitions = Columns.Define(Star, Star),
        Children = {
            new Button {
                    ImageSource = ImageSource.FromFile("resolve.png"),
                    ContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Left, 10)
                }
                .Text("Résoudre")
                .BindCommand(nameof(viewTirage.ResolveCommand))
                .Column(0),
            new Button {
                    ImageSource = ImageSource.FromFile("random.png"),
                    ContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Left, 10)
                }
                .BindCommand(nameof(viewTirage.RandomCommand))
                .Text("Hasard")
                .Column(1)
        }
    };


    /// <summary>
    ///     Gets a view that displays a collection of solutions in a list format.
    /// </summary>
    /// <remarks>
    ///     The view is an instance of <see cref="Syncfusion.Maui.ListView.SfListView" /> with the following properties:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>SelectionMode: Single</description>
    ///         </item>
    ///         <item>
    ///             <description>AutoFitMode: DynamicHeight</description>
    ///         </item>
    ///         <item>
    ///             <description>CachingStrategy: RecycleTemplate</description>
    ///         </item>
    ///         <item>
    ///             <description>IsScrollingEnabled: true</description>
    ///         </item>
    ///         <item>
    ///             <description>HeightRequest: 500</description>
    ///         </item>
    ///         <item>
    ///             <description>ItemsLayout: GridLayout with SpanCount based on device idiom</description>
    ///         </item>
    ///         <item>
    ///             <description>ItemTemplate: DataTemplate for displaying solution details</description>
    ///         </item>
    ///     </list>
    ///     The view is bound to the "Tirage.Solutions" property and its visibility is controlled by the "viewTirage.VueGrille"
    ///     property.
    /// </remarks>
    private View VueGollectionSolutions => new CollectionView {
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
        .Bind(IsVisibleProperty, nameof(viewTirage.VueGrille))
        .Bind(ItemsView.ItemsSourceProperty, "Tirage.Solutions")
        .AppThemeColorBinding(BackgroundColorProperty, BackgroundLight,
            BackgroundDark) //"{AppThemeBinding Dark=DarkSlateGrey, Light=#8fbc8f}"
        .Invoke(l => {
            l.SelectionChanged += (sender, _) => {
                if (sender is SfListView { SelectedItem: CebBase sol }) viewTirage.ShowPopup(sol);
            };
        });

    /// <summary>
    ///     Gets a detailed view of the solutions.
    /// </summary>
    /// <value>
    ///     A <see cref="View" /> that represents the detailed view of the solutions.
    /// </value>
    private View VueSolutionsDetail => new CollectionView {
            Margin = 2,
            HeightRequest = 100,
            HorizontalOptions = LayoutOptions.Center,
            SelectionMode = SelectionMode.Single,
            ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical)
        }.Bind(ItemsView.ItemsSourceProperty, "Operations")
        .AppThemeColorBinding(BackgroundColorProperty, Colors.CadetBlue, BackgroundDark)
        .Center()
        .Invoke(collection => collection.SelectionChanged += (sender, _) => {
            if (sender is CollectionView { BindingContext: CebBase sol }) viewTirage.ShowPopup(sol);
        });

    /// <summary>
    ///     Gets the data grid view for displaying solutions.
    /// </summary>
    /// <remarks>
    ///     This property initializes and configures an instance of <see cref="Syncfusion.Maui.DataGrid.SfDataGrid" />
    ///     to display solutions with specific column mappings and bindings.
    /// </remarks>
    private SfDataGrid VueDataGridSolutions => new SfDataGrid {
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
        .Bind(IsVisibleProperty, nameof(viewTirage.VueGrille), BindingMode.Default, invertedBoolConverter)
        .Bind(SfDataGrid.ItemsSourceProperty, "Tirage.Solutions")
        .DynamicResource(StyleProperty, "DatagridSolutionsStyle")
        .Invoke(datagrid => {
            datagrid.SelectionChanged += (sender, _) => {
                if (sender is SfDataGrid { SelectedRow: CebBase sol }) viewTirage.ShowPopup(sol);
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
    private View Vueindicator => Borderize(new SfBusyIndicator {
                IndicatorColor = Colors.Blue,
                AnimationType = AnimationType.CircularMaterial,
                ZIndex = 99
            }
            .Bind(SfBusyIndicator.IsRunningProperty, nameof(viewTirage.IsBusy)))
        .Bind(IsVisibleProperty, nameof(viewTirage.IsBusy));

    private SfPopup VuePopup => new SfPopup {
            Margin = 4,
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
                    StrokeThickness = 2
                }.Bind(PopupStyle.StrokeProperty,
                    nameof(viewTirage.Foreground)),
            ContentTemplate = new DataTemplate(() => new VerticalStackLayout {
                Margin = 4,
                HorizontalOptions = LayoutOptions.Center,
                Spacing = 0.5,
                Children = {
                    new VerticalStackLayout {
                        Padding = 2,
                        VerticalOptions = LayoutOptions.Start,
                        Children = {
                            new Label()
                                .FontSize(24)
                                .CenterHorizontal()
                                .CenterVertical()
                                .Bind(Label.TextProperty, "Result")
                                .Bind(Label.TextColorProperty, nameof(viewTirage.Foreground))
                        }
                    },
                    new BoxView {
                        HeightRequest = 2,
                        CornerRadius = 0
                    }.Bind(BoxView.ColorProperty,
                        nameof(viewTirage.Foreground)),
                    new VerticalStackLayout {
                        VerticalOptions = LayoutOptions.Center,
                        Children = {
                            new CollectionView {
                                Margin = 4,
                                VerticalOptions = LayoutOptions.Center,
                                ItemTemplate = new DataTemplate(
                                    () => new Label {
                                            HorizontalTextAlignment = TextAlignment.Center
                                        }
                                        .Bind(Label.TextProperty))
                            }.Bind(ItemsView.ItemsSourceProperty, "Solution.Operations")
                        }
                    },
                    new BoxView {
                        HeightRequest = 2,
                        CornerRadius = 0
                    }.Bind(BoxView.ColorProperty,
                        nameof(viewTirage.Foreground)),
                    new Grid {
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
                                .Bind(Label.TextColorProperty, nameof(viewTirage.Foreground))
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
                                .Bind(Label.TextColorProperty, nameof(viewTirage.Foreground))
                                .Row(1).Column(1),

                            new Label().Text("Durée:")
                                .TextRight()
                                .Margin(2)
                                .FontSize(15)
                                .Row(2).Column(0),

                            new Label()
                                .Margin(2)
                                .TextRight()
                                .Bind(Label.TextProperty, "Tirage.Duree.TotalSeconds", stringFormat: "{0:N3} s")
                                .Bind(Label.TextColorProperty, nameof(viewTirage.Foreground))
                                .Row(2).Column(1)
                        }
                    }
                }
            })
        }
        .Bind(SfPopup.IsOpenProperty, nameof(viewTirage.Popup));


    /// <summary>
    ///     Gets a view that displays the result of the computation, including the result value,
    ///     whether the result was found, the number of solutions, and the duration of the computation.
    /// </summary>
    /// <value>
    ///     A <see cref="View" /> that contains labels bound to the properties of the <see cref="ViewTirage" /> view model.
    /// </value>
    private View VueResultat => new Grid {
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
                    .Bind(Label.TextProperty, nameof(viewTirage.Result))
                    .Bind(Label.TextColorProperty, nameof(viewTirage.Foreground))
                    .Row(0)
                    .Column(0),
                new Label()
                    .Bold()
                    .CenterVertical()
                    .Bind(Label.TextProperty, "Tirage.Found", stringFormat: "Trouvé: {0}")
                    .Bind(Label.TextColorProperty, nameof(viewTirage.Foreground))
                    .Row(0)
                    .Column(1),
                new Label()
                    .Bold()
                    .CenterVertical()
                    .Bind(Label.TextProperty, "Tirage.Count", stringFormat: "Nombre de solutions: {0}")
                    .Bind(Label.TextColorProperty, nameof(viewTirage.Foreground))
                    .Column(DeviceInfo.Idiom == DeviceIdiom.Phone ? 0 : 2)
                    .Row(DeviceInfo.Idiom == DeviceIdiom.Phone ? 1 : 0),
                new Label()
                    .CenterVertical()
                    .Bold()
                    .Bind(Label.TextProperty, "Tirage.Duree.TotalSeconds", stringFormat: "Durée: {0:N3} s")
                    .Bind(Label.TextColorProperty, nameof(viewTirage.Foreground))
                    .Column(DeviceInfo.Idiom == DeviceIdiom.Phone ? 1 : 3)
                    .Row(DeviceInfo.Idiom == DeviceIdiom.Phone ? 1 : 0)
            }
        }.Bind(IsVisibleProperty, nameof(viewTirage.IsComputed))
        .AppThemeColorBinding(BackgroundColorProperty, BackgroundLight, BackgroundDark);


    

    private VerticalStackLayout MainStackLayout {
        get {
            var verticalStackLayout = new VerticalStackLayout();
            if (DeviceInfo.Idiom != DeviceIdiom.Phone) verticalStackLayout.Add(Borderize(VueAction, true));

            verticalStackLayout.Add(Borderize(VueSaisie, true));
            verticalStackLayout.Add(Borderize(VueResultat, true));
            verticalStackLayout.Add(VueSolutions);
            verticalStackLayout.Add(VuePopup);
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
    private Grid VuePlaques {
        get {
            var result = new Grid();
            if (DeviceInfo.Idiom != DeviceIdiom.Phone) {
                result.ColumnDefinitions = Columns.Define(Star, Star, Star, Star, Star, Star);
            }
            else {
                result.RowDefinitions = Rows.Define(Star, Star, Star);
                result.ColumnDefinitions = Columns.Define(Star, Star);
            }
            AddPlaquesToGrid();
            return result;

            void AddPlaquesToGrid() {
                for (var i = 0; i < 6; i++) {
                    var comboBox = PlaqueComboBox(i);
                    PositionComboBoxInGrid(ref comboBox, i);
                    result.Children.Add(comboBox);
                }
                return;

                /// <summary>
                ///     Creates a <see cref="SfComboBox" /> for a plaque at the specified index.
                /// </summary>
                /// <param name="index">The index of the plaque.</param>
                /// <returns>A <see cref="SfComboBox" /> for the plaque.</returns>
                SfComboBox PlaqueComboBox(int index) => new SfComboBox {
                    ItemsSource = CebPlaque.DistinctPlaques
                }
                .Bind(DropDownListBase.TextProperty, $"Tirage.Plaques[{index}].Value", BindingMode.TwoWay);

                /// <summary>
                ///     Positions the <see cref="SfComboBox" /> in the grid based on the index.
                /// </summary>
                /// <param name="comboBox">The <see cref="SfComboBox" /> to position.</param>
                /// <param name="index">The index of the plaque.</param>
                void PositionComboBoxInGrid(ref SfComboBox comboBox, int index) {
                    if (DeviceInfo.Idiom != DeviceIdiom.Phone) {
                        comboBox.Column(index);
                    }
                    else {
                        comboBox.Column(index % 2).Row(index / 2);
                    }
                }
            }
        }
    }
        
    

    /// <summary>
    ///     Gets the context menu for the page, providing various commands and options for user interaction.
    /// </summary>
    /// <value>
    ///     A <see cref="MenuFlyout" /> containing menu items and sub-items with associated commands and keyboard shortcuts.
    /// </value>
    private MenuFlyout MenuContext => [
        new MenuFlyoutItem {
                KeyboardAccelerators = {
                    new KeyboardAccelerator {
                        Key = "H",
                        Modifiers = KeyboardAcceleratorModifiers.Ctrl
                    }
                }
            }
            .Text("Hasard")
            .BindCommand(nameof(viewTirage.RandomCommand)),
        new MenuFlyoutItem {
                KeyboardAccelerators = {
                    new KeyboardAccelerator {
                        Key = "R",
                        Modifiers = KeyboardAcceleratorModifiers.Ctrl
                    }
                }
            }
            .BindCommand(nameof(viewTirage.ResolveCommand))
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
                .BindCommand(nameof(viewTirage.ExportCommand), parameterSource: "excel"),
            new MenuFlyoutItem {
                    KeyboardAccelerators = {
                        new KeyboardAccelerator {
                            Key = "W",
                            Modifiers = KeyboardAcceleratorModifiers.Ctrl | KeyboardAcceleratorModifiers.Shift
                        }
                    }
                }
                .Text("Word")
                .BindCommand(nameof(viewTirage.ExportCommand), parameterSource: "word"),
            new MenuFlyoutItem {
                    KeyboardAccelerators = {
                        new KeyboardAccelerator {
                            Key = "J",
                            Modifiers = KeyboardAcceleratorModifiers.Ctrl | KeyboardAcceleratorModifiers.Shift
                        }
                    }
                }
                .Text("Json")
                .BindCommand(nameof(viewTirage.ExportCommand), parameterSource: "json"),
            new MenuFlyoutItem {
                    KeyboardAccelerators = {
                        new KeyboardAccelerator {
                            Key = "M",
                            Modifiers = KeyboardAcceleratorModifiers.Ctrl | KeyboardAcceleratorModifiers.Shift
                        }
                    }
                }
                .Text("Xml")
                .BindCommand(nameof(viewTirage.ExportCommand), parameterSource: "xml"),
            new MenuFlyoutItem {
                    KeyboardAccelerators = {
                        new KeyboardAccelerator {
                            Key = "H",
                            Modifiers = KeyboardAcceleratorModifiers.Ctrl | KeyboardAcceleratorModifiers.Shift
                        }
                    }
                }
                .Text("HTML")
                .BindCommand(nameof(viewTirage.ExportCommand), parameterSource: "html")
        }.Text("Export"),
        new MenuFlyoutItem {
                KeyboardAccelerators = {
                    new KeyboardAccelerator {
                        Key = "V",
                        Modifiers = KeyboardAcceleratorModifiers.Ctrl
                    }
                }
            }
            .Text("Inverser vue").BindCommand(nameof(viewTirage.InverseCommand), parameterSource: "vue"),
        new MenuFlyoutItem {
            KeyboardAccelerators = {
                new KeyboardAccelerator {
                    Key = "T",
                    Modifiers = KeyboardAcceleratorModifiers.Ctrl
                }
            }
        }.Text("Thème").BindCommand(nameof(viewTirage.InverseCommand), parameterSource: "theme"),
        new MenuFlyoutItem {
            KeyboardAccelerators = {
                new KeyboardAccelerator {
                    Key = "A",
                    Modifiers = KeyboardAcceleratorModifiers.Ctrl
                }
            }
        }.Text("Auto").BindCommand(nameof(viewTirage.InverseCommand), parameterSource: "auto")
    ];

    /// <summary>
    ///     Creates a bordered view with optional shadow effect.
    /// </summary>
    /// <param name="content">The content to be wrapped within the border.</param>
    /// <param name="isShadow">Indicates whether the border should have a shadow effect. Default is false.</param>
    /// <returns>A <see cref="Border" /> element containing the specified content.</returns>
    private Border Borderize(View content, bool isShadow = false) => new Border {
            StrokeThickness = 0.5,
            Content = content
        }
        .Margin(new Thickness(1, 1))
        .Bind(IsEnabledProperty,
            nameof(viewTirage.IsBusy), BindingMode.OneWay, invertedBoolConverter)
        .Bind(Border.StrokeProperty, nameof(viewTirage.Foreground))
        .Invoke(b => {
            if (isShadow) b.DynamicResource(StyleProperty, "BorderWithShadowStyle");
        });
}