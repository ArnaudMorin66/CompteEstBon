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
///     The <see cref="CebToolkit.CebPage" /> class is a specialized page that inherits from <see cref="ContentPage" />. It
///     initializes the binding context, context flyout, content, and theme color binding for the page.
/// </remarks>
// ReSharper disable once PartialTypeWithSinglePart
public partial class CebPage : ContentPage {
    /// <summary>
    ///     An instance of <see cref="CommunityToolkit.Maui.Converters.InvertedBoolConverter" /> used to invert boolean
    ///     values in bindings.
    /// </summary>
    /// <remarks>
    ///     This converter is primarily used to bind properties that require the opposite boolean value of the source
    ///     property.
    /// </remarks>
    private static readonly InvertedBoolConverter InvertedBoolConverter = new();

    private static readonly FuncConverter<bool, NumericEntryUpDownPlacementMode> UpDownConverter =
        new(b => b ? NumericEntryUpDownPlacementMode.Hidden : NumericEntryUpDownPlacementMode.InlineVertical);

    /// <summary>
    ///     Represents the view model for the current page, providing commands and properties for the UI.
    /// </summary>
    /// <remarks>
    ///     This field is initialized using the dependency injection container and is used as the binding context for the
    ///     page.
    /// </remarks>
    private static readonly ViewTirage TirageContext = App.Current.Services.GetService<ViewTirage>()!;


    /// <summary>
    ///     Initializes a new instance of the <see cref="CebToolkit.CebPage" /> class.
    /// </summary>
    /// <remarks>
    ///     This constructor sets up the binding context, context flyout, content, and theme color binding for the page.
    /// </remarks>
    public CebPage() {
        BindingContext = TirageContext;
        FlyoutBase.SetContextFlyout(this, MenuContext);
        Content = MainStackLayout;
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
            Vueindicator
        }
    };

    /// <summary>
    ///     Gets a view that represents the input section of the page.
    /// </summary>
    /// <remarks>
    ///     This view is constructed as a grid with different layouts for Windows and other platforms. It includes a numeric
    ///     entry for user input, which is bound to the "Tirage.Search" property.
    /// </remarks>
    private static View VueSaisie {
        get {
            var result = new Grid();
            if (DeviceInfo.Idiom != DeviceIdiom.Phone)
                result.ColumnDefinitions = Columns.Define(Star, Star, Star, Star, Star, Star, Star);
            else
                result.RowDefinitions = Rows.Define(Star, Star, Star, Star);

            result.Children
                .Add(
                    DeviceInfo.Idiom != DeviceIdiom.Phone
                        ? VuePlaques.Column(0).ColumnSpan(6)
                        : VuePlaques.Row(0).RowSpan(3));
            var num = new SfNumericEntry {
                    CustomFormat = "000",
                    Maximum = 999,
                    Minimum = 100
                }
                .Bind(SfNumericEntry.UpDownPlacementModeProperty, nameof(TirageContext.Auto),
                    converter: UpDownConverter)
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
                ImageSource = ImageSource.FromFile("alea.png"),
                Margin = 5,
                ContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Left, 30),
                Command = TirageContext.RandomCommand,
                Text = "Hasard"
            }.Column(0),
            new Button {
                ImageSource = ImageSource.FromFile("calculer.png"),
                Margin = 5,
                ContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Left, 30),
                Command = TirageContext.SolveCommand,
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
            ItemsLayout = new GridItemsLayout(DeviceInfo.Idiom == DeviceIdiom.Phone ? 2 : 4, ItemsLayoutOrientation.Vertical) {
                    HorizontalItemSpacing = 5,
                    VerticalItemSpacing = 5,
                    SnapPointsAlignment = SnapPointsAlignment.Center,
                    SnapPointsType = SnapPointsType.MandatorySingle
                },
            ItemTemplate = new DataTemplate(() => VueSolutionsDetail)
        }.Bind(IsVisibleProperty, nameof(TirageContext.VueGrille))
        .Bind(ItemsView.ItemsSourceProperty, "Tirage.Solutions")
        .AppThemeColorBinding(BackgroundColorProperty, AppShell.BackgroundLight, AppShell.BackgroundDark)
        .Invoke(
            l => {
                l.SelectionChanged += (sender, _) => {
                    if (sender is CollectionView { SelectedItem: CebBase selectedItem } &&
                        TirageContext.ShowPopupCommand.CanExecute(selectedItem))
                        TirageContext.ShowPopupCommand.Execute(selectedItem);
                };
            });

    /// <summary>
    ///     Gets a detailed view of the solutions.
    /// </summary>
    /// <value>
    ///     A <see cref="View" /> that represents the detailed view of the solutions.
    /// </value>
    private static View VueSolutionsDetail => Borderize(
        new CollectionView {
                Margin = -2,
                MinimumHeightRequest = 100,
                HorizontalOptions = LayoutOptions.Start,
                SelectionMode = SelectionMode.Single,
                VerticalScrollBarVisibility = ScrollBarVisibility.Never,
                VerticalOptions = LayoutOptions.Start,
                IsEnabled = false,
                ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical),
                ItemTemplate =
                    new DataTemplate(
                        () => Borderize(new Label {
                                HorizontalTextAlignment = TextAlignment.Center
                            }.Bind(Label.TextProperty)
                            .AppThemeColorBinding(Label.TextColorProperty, Colors.Black, Colors.White)
                            .Bold()))
            }.Bind(ItemsView.ItemsSourceProperty, "Operations")
            .AppThemeColorBinding(BackgroundColorProperty, AppShell.BackgroundLight, AppShell.BackgroundDark)
            .Center());


    /// <summary>
    ///     Gets the data grid view for displaying solutions.
    /// </summary>
    /// <remarks>
    ///     This property initializes and configures an instance of <see cref="Syncfusion.Maui.DataGrid.SfDataGrid" /> to
    ///     display solutions with specific column mappings and bindings.
    /// </remarks>
    private static SfDataGrid VueDataGridSolutions => new SfDataGrid {
            VerticalScrollBarVisibility = ScrollBarVisibility.Always,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Always,
            ShowRowHeader = false,
            HeaderRowHeight = 0,
            MinimumHeightRequest = 500,
            Columns = Colonnes
        }.Bind(IsVisibleProperty, nameof(TirageContext.VueGrille), BindingMode.Default, InvertedBoolConverter)
        .Bind(SfDataGrid.ItemsSourceProperty, "Tirage.Solutions")
        .DynamicResource(StyleProperty, "DatagridSolutionsStyle")
        .Invoke(
            datagrid => {
                datagrid.SelectionChanged += (sender, _) => {
                    if (sender is SfDataGrid { SelectedRow: CebBase selectedRow } &&
                        TirageContext.ShowPopupCommand.CanExecute(selectedRow))
                        TirageContext.ShowPopupCommand.Execute(selectedRow);
                };
            });

    /// <summary>
    ///     Gets the column collection for the data grid, defining the mappings and minimum widths for each column.
    /// </summary>
    /// <value>
    ///     A <see cref="ColumnCollection" /> containing the columns for the data grid.
    /// </value>
    private static ColumnCollection Colonnes => [
        new DataGridTextColumn {
            MappingName = "Op1",
            MinimumWidth = 120
        },
        new DataGridTextColumn {
            MappingName = "Op2",
            MinimumWidth = 120
        },
        new DataGridTextColumn {
            MappingName = "Op3",
            MinimumWidth = 120
        },
        new DataGridTextColumn {
            MappingName = "Op4",
            MinimumWidth = 120
        },
        new DataGridTextColumn {
            MappingName = "Op5",
            MinimumWidth = 120
        }
    ];

    /// <summary>
    ///     Gets a view that displays a busy indicator.
    /// </summary>
    /// <remarks>
    ///     The busy indicator is displayed when the <see cref="CebToolkit.ViewModel.ViewTirage.IsBusy" /> property is set to
    ///     <c>true</c>. The indicator uses a circular material animation and is colored blue.
    /// </remarks>
    private static View Vueindicator => 
            new SfBusyIndicator {
                IndicatorColor = Colors.Yellow,
                AnimationType = AnimationType.CircularMaterial,
                ZIndex = 99
            }.Bind(SfBusyIndicator.IsRunningProperty, nameof(TirageContext.IsBusy))
        .Bind(IsVisibleProperty, nameof(TirageContext.IsBusy))
        .Bind(SfBusyIndicator.TitleProperty, nameof(TirageContext.ElapsedTime), stringFormat: "{0:hh\\:mm\\:ss\\.fff}");

    /// <summary>
    ///     Gets the popup view used to display results and related information.
    /// </summary>
    /// <value>
    ///     A <see cref="View" /> representing the popup, which includes various UI elements such as labels, box views, and a
    ///     collection view.
    /// </value>
    /// <remarks>
    ///     The popup is configured with specific styles and bindings to display the results of a "tirage" operation,
    ///     including the found result, the number of solutions, and the elapsed time.
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
            }.Bind(
                PopupStyle.StrokeProperty,
                nameof(TirageContext.Foreground)),
        ContentTemplate =
            new DataTemplate(
                () => Borderize(
                    new VerticalStackLayout {
                        Margin = 2,
                        HorizontalOptions = LayoutOptions.Center,
                        Spacing = 0.5,
                        Children = {
                            VueHeaderPopup,
                            VueSeparator,
                            VueDetailPopup,
                            VueSeparator,
                            VueFooterPopup
                        }
                    }))
    }.Bind(SfPopup.IsOpenProperty, nameof(TirageContext.Popup));

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
        .Bind(Label.TextColorProperty, nameof(TirageContext.Foreground));

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
            ItemTemplate =
                new DataTemplate(
                    () => new Label {
                        DisableLayout = true,
                        HorizontalTextAlignment = TextAlignment.Center
                    }.Bind(
                        Label.TextProperty))
        }.Bind(ItemsView.ItemsSourceProperty, "Solution.Operations")
        .AppThemeColorBinding(BackgroundColorProperty, AppShell.BackgroundLight, AppShell.BackgroundDark)
        .Center();

    private static View VueSeparator => new BoxView {
        HeightRequest = 2,
        CornerRadius = 0
    }.Bind(
        BoxView.ColorProperty,
        nameof(TirageContext.Foreground));

    private static View VueFooterPopup => new Grid {
        ColumnDefinitions = Columns.Define(Stars(5), Stars(4)),
        RowDefinitions = Rows.Define(Star, Star, Star),
        Children = {
            new Label().Text("Trouvé:").TextRight().Margin(2).FontSize(15).Row(0).Column(0),
            new Label()
                .Margin(2)
                .TextRight()
                .Bind(Label.TextProperty, "Tirage.Found")
                .Bind(Label.TextColorProperty, nameof(TirageContext.Foreground))
                .Row(0)
                .Column(1),

            new Label().Text("Nombre de solutions:").TextRight().Margin(2).FontSize(15).Row(1).Column(0),

            new Label()
                .Margin(2)
                .TextRight()
                .Bind(Label.TextProperty, "Tirage.Count")
                .Bind(Label.TextColorProperty, nameof(TirageContext.Foreground))
                .Row(1)
                .Column(1),

            new Label().Text("Durée:").TextRight().Margin(2).FontSize(15).Row(2).Column(0),

            new Label()
                .Margin(2)
                .TextRight()
                .Bind(Label.TextProperty, nameof(TirageContext.ElapsedTime), stringFormat: "{0:hh\\:mm\\:ss\\.fff}")
                .Bind(Label.TextColorProperty, nameof(TirageContext.Foreground))
                .Row(2)
                .Column(1)
        }
    };

    /// <summary>
    ///     Gets a view that displays the result of the computation, including the result value, whether the result was
    ///     found, the number of solutions, and the duration of the computation.
    /// </summary>
    /// <value>
    ///     A <see cref="View" /> that contains labels bound to the properties of the <see cref="ViewModel.ViewTirage" /> view
    ///     model.
    /// </value>
    private static View VueResultat => new Grid {
        HeightRequest = 40,
        ColumnDefinitions =
            DeviceInfo.Idiom == DeviceIdiom.Phone ? Columns.Define(Star, Star) : Columns.Define(Star, Star, Star, Star),
        RowDefinitions = DeviceInfo.Idiom == DeviceIdiom.Phone ? Rows.Define(Star, Star) : Rows.Define(Star),
        VerticalOptions = LayoutOptions.Center,
        Children = {
            VueLabelResultat(0, 0, nameof(TirageContext.Result)),
            VueLabelResultat(0, 1, "Tirage.Found",  "Trouvé: {0}"),
            VueLabelResultat(
                    DeviceInfo.Idiom == DeviceIdiom.Phone ? 1 : 0,
                    DeviceInfo.Idiom == DeviceIdiom.Phone ? 0 : 2, "Tirage.Count", "Nombre de solutions: {0}"),
            VueLabelResultat(
                    DeviceInfo.Idiom == DeviceIdiom.Phone ? 1 : 0,
                    DeviceInfo.Idiom == DeviceIdiom.Phone ? 1 : 3,
                    nameof(TirageContext.ElapsedTime),
                    @"Durée: {0:hh\:mm\:ss\.fff}")
        }
    }.Bind(Grid.IsVisibleProperty, nameof(TirageContext.IsComputed));

    /// <summary>
    ///     Gets the main stack layout of the page.
    /// </summary>
    /// <value>
    ///     A <see cref="VerticalStackLayout" /> representing the main stack layout of the page.
    /// </value>
    private static VerticalStackLayout MainStackLayout {
        get {
            VerticalStackLayout verticalStackLayout = [
                Borderize(VueSaisie), Borderize(VueResultat), VueSolutions, VuePopup
            ];
            if (DeviceInfo.Idiom != DeviceIdiom.Phone) verticalStackLayout.Insert(1, Borderize(VueAction));
            return verticalStackLayout;
        }
    }

    /// <summary>
    ///     Gets a <see cref="Grid" /> representing the view for displaying plaques.
    /// </summary>
    /// <remarks>
    ///     The layout of the grid varies depending on the platform. On Windows and MacCatalyst, it defines six columns. On
    ///     other platforms, it defines three rows and two columns.
    /// </remarks>
    /// <returns>A <see cref="Grid" /> representing the view for displaying plaques.</returns>
    private static Grid VuePlaques {
        get {
            var grid = new Grid();
            if (DeviceInfo.Idiom != DeviceIdiom.Phone) {
                grid.ColumnDefinitions = Columns.Define(Star, Star, Star, Star, Star, Star);
            } else {
                grid.RowDefinitions = Rows.Define(Star, Star);
                grid.ColumnDefinitions = Columns.Define(Star, Star, Star);
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
            }.Text("Hasard")
            .BindCommand(nameof(TirageContext.RandomCommand)),
        new MenuFlyoutItem {
                KeyboardAccelerators = {
                    new KeyboardAccelerator {
                        Key = "R",
                        Modifiers = KeyboardAcceleratorModifiers.Ctrl
                    }
                }
            }.BindCommand(nameof(TirageContext.SolveCommand))
            .Text("Résoudre"),
        new MenuFlyoutSeparator(), new MenuFlyoutSubItem {
            new MenuFlyoutItem {
                    KeyboardAccelerators = {
                        new KeyboardAccelerator {
                            Key = "X",
                            Modifiers = KeyboardAcceleratorModifiers.Ctrl | KeyboardAcceleratorModifiers.Shift
                        }
                    }
                }.Text("Excel")
                .BindCommand(nameof(TirageContext.ExportCommand), parameterSource: "excel"),

            new MenuFlyoutItem {
                    KeyboardAccelerators = {
                        new KeyboardAccelerator {
                            Key = "W",
                            Modifiers = KeyboardAcceleratorModifiers.Ctrl | KeyboardAcceleratorModifiers.Shift
                        }
                    }
                }.Text("Word")
                .BindCommand(nameof(TirageContext.ExportCommand), parameterSource: "word"),

            new MenuFlyoutItem {
                    KeyboardAccelerators = {
                        new KeyboardAccelerator {
                            Key = "J",
                            Modifiers = KeyboardAcceleratorModifiers.Ctrl | KeyboardAcceleratorModifiers.Shift
                        }
                    }
                }.Text("Json")
                .BindCommand(nameof(TirageContext.ExportCommand), parameterSource: "json"),

            new MenuFlyoutItem {
                    KeyboardAccelerators = {
                        new KeyboardAccelerator {
                            Key = "M",
                            Modifiers = KeyboardAcceleratorModifiers.Ctrl | KeyboardAcceleratorModifiers.Shift
                        }
                    }
                }.Text("Xml")
                .BindCommand(nameof(TirageContext.ExportCommand), parameterSource: "xml"),

            new MenuFlyoutItem {
                    KeyboardAccelerators = {
                        new KeyboardAccelerator {
                            Key = "H",
                            Modifiers = KeyboardAcceleratorModifiers.Ctrl | KeyboardAcceleratorModifiers.Shift
                        }
                    }
                }.Text("HTML")
                .BindCommand(nameof(TirageContext.ExportCommand), parameterSource: "html")
        }.Text("Export"),
        new MenuFlyoutItem {
                KeyboardAccelerators = {
                    new KeyboardAccelerator {
                        Key = "V",
                        Modifiers = KeyboardAcceleratorModifiers.Ctrl
                    }
                }
            }.Text("Vue")
            .BindCommand(nameof(TirageContext.UpdatePropertyCommand), parameterSource: "vue"),
        new MenuFlyoutItem {
                KeyboardAccelerators = {
                    new KeyboardAccelerator {
                        Key = "T",
                        Modifiers = KeyboardAcceleratorModifiers.Ctrl
                    }
                }
            }.Text("Thème")
            .BindCommand(nameof(TirageContext.UpdatePropertyCommand), parameterSource: "theme"),
        new MenuFlyoutItem {
                KeyboardAccelerators = {
                    new KeyboardAccelerator {
                        Key = "A",
                        Modifiers = KeyboardAcceleratorModifiers.Ctrl
                    }
                }
            }.Text("Auto")
            .BindCommand(nameof(TirageContext.UpdatePropertyCommand), parameterSource: "auto"),
        new MenuFlyoutSeparator(), new MenuFlyoutItem {
                KeyboardAccelerators = {
                    new KeyboardAccelerator {
                        Key = "Q",
                        Modifiers = KeyboardAcceleratorModifiers.Alt
                    }
                }
            }.Text("Quitter")
            .BindCommand(nameof(TirageContext.QuitterCommand))
    ];

    /// <summary>
    ///     Creates a label view for displaying the result with specified row and column positions.
    /// </summary>
    /// <param name="ligne">The row position of the label in the grid.</param>
    /// <param name="colonne">The column position of the label in the grid.</param>
    /// <param name="texte">The text to be displayed in the label.</param>
    /// <param name="fmt">The format string for the text.</param>
    /// <returns>A <see cref="Label" /> configured with bindings and layout properties.</returns>
    private static Label VueLabelResultat(int ligne, int colonne, string texte, string fmt = "{0}") => new Label()
        .Bold()
        .CenterVertical()
        .Bind(Label.TextColorProperty, nameof(TirageContext.Foreground))
        .Bind(Label.TextProperty, texte, stringFormat: fmt)
        .Row(ligne)
        .Column(colonne);


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
            comboBox.Column(index % 3).Row(index / 3);
    }

    /// <summary>
    ///     Creates a <see cref="SfComboBox" /> for a plaque at the specified index.
    /// </summary>
    /// <param name="index">The index of the plaque.</param>
    /// <returns>A <see cref="SfComboBox" /> for the plaque.</returns>
    private static SfComboBox PlaqueComboBox(int index) => new SfComboBox {
        ItemsSource = CebPlaque.DistinctPlaques
    }.Bind(
        DropDownListBase.TextProperty,
        $"Tirage.Plaques[{index}].Value",
        BindingMode.TwoWay);

    /// <summary>
    ///     Creates a bordered view with optional shadow effect.
    /// </summary>
    /// <param name="content">The content to be wrapped within the border.</param>
    /// <returns>A <see cref="Border" /> element containing the specified content.</returns>
    private static Border Borderize(View content) => new Border {
            StrokeThickness = 0.5,
            Content = content,
            Margin = 1
        }
        .Bind(IsEnabledProperty, nameof(TirageContext.IsBusy), BindingMode.OneWay, InvertedBoolConverter)
        .Bind(Border.StrokeProperty, nameof(TirageContext.Foreground));
}