using CebToolkit.ViewModel;

using CommunityToolkit.Maui.Markup;

using CompteEstBon;

using Syncfusion.Maui.Buttons;
using Syncfusion.Maui.Core;
using Syncfusion.Maui.Core.Converters;
using Syncfusion.Maui.DataGrid;
using Syncfusion.Maui.Inputs;
using Syncfusion.Maui.Inputs.DropDownControls;
using Syncfusion.Maui.Popup;

using static CommunityToolkit.Maui.Markup.GridRowsColumns;


namespace CebToolkit;

public partial class MainPage {
    private readonly InvertedBoolConverter _invertedBoolConverter = new();

    public MainPage() {
        Content = MainScrollView();
        DefineMenuContestuel(Content);
    }

    public ViewTirage ViewTirage { get; } = new();

    private View VueSolutions => 
        new Grid {
            RowDefinitions = Rows.Define(Star),
            Children = { VueGollectionSolutions, VueDataGridSolutions, VueBuzyindicator }
        };

    public View VueSaisie {
        get {
            var vue = new Grid() {
                    ColumnDefinitions = Columns.Define(
                        Star, Stars(2), Stars(2), Stars(2), Stars(2), Stars(2), Stars(2), Star, Stars(3))
                }
                .CenterVertical();
            vue.Add(new Label()
                    .Text("Plaques")
                    .Margin(2)
                    .CenterVertical()
                    .Start()
                    .Column(0));
            AddPlaques(vue);

            vue.Add(new Label().Text("Chercher").CenterVertical().Column(7));
            vue.Add(new SfNumericEntry {
                    CustomFormat = "000", Maximum = 999, Minimum = 100, HorizontalTextAlignment = TextAlignment.Center,
                    UpDownPlacementMode = NumericEntryUpDownPlacementMode.InlineVertical
                }
                .CenterHorizontal()
                .Bind(SfNumericEntry.ShowBorderProperty, nameof(ViewTirage.ThemeDark), BindingMode.Default,
                    _invertedBoolConverter)
                .Column(9).Bind(SfNumericEntry.ValueProperty, "Tirage.Search"));
        
        return vue;
    }
    }


    private View VueAction => new Grid {
            ColumnDefinitions = Columns.Define(
                GridLength.Star, GridLength.Star, GridLength.Star,
                GridLength.Star, GridLength.Star, GridLength.Star),
            Children = {
                new Button()
                    .Text("Résoudre")
                    .Bind(Button.CommandProperty)
                    .Invoke(b => {
                        b.CornerRadius = 5;
                        b.CommandParameter = "resolve";
                    })
                    .Margin(2)
                    .Column(0),
                new Button()
                    .Bind(Button.CommandProperty)
                    .Text("Hasard")
                    .Invoke(b => {
                        b.CornerRadius = 5;
                        b.CommandParameter = "random";
                    })
                    .Margin(2)
                    .Column(1),
                VueGridGrille.Column(2), VueExport.Column(3), VueTheme.Column(4), VueAuto.Column(5)
            }
        };



private Grid VueGridGrille => new() {
        ColumnDefinitions =
            Columns.Define(GridLength.Star, new GridLength(2, GridUnitType.Star)),
        HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Center, Children = {
            new Label {
                Text = "Grille:", BackgroundColor = Colors.Transparent, FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Fill, HorizontalTextAlignment = TextAlignment.End,
                VerticalOptions = LayoutOptions.Center
            }.Column(0),
            new SfSwitch().Bind(SfSwitch.IsOnProperty!, nameof(ViewTirage.VueGrille))
                .Column(1)
        }
    };

    private Grid VueExport => new() {
        ColumnDefinitions = Columns.Define(GridLength.Star, GridLength.Star), Children = {
            new Picker { ItemsSource = ViewTirage.ListeFormats }
                .Bind(Picker.SelectedItemProperty, nameof(ViewTirage.FmtExport), BindingMode.TwoWay)
                .Column(0),
            new Button { Text = "Export", CommandParameter = "export", Margin = 2 }
                .Bind(Button.CommandProperty, nameof(ViewTirage.ExportsCommmand))
                .Bind(Button.CommandParameterProperty, nameof(ViewTirage.FmtExport))
                .Column(1)
        }
    };

    private Grid VueTheme => new() {
        ColumnDefinitions = Columns.Define(GridLength.Star, GridLength.Star),
        Children = {
            new Label {
                Text = "Sombre", VerticalTextAlignment = TextAlignment.Center,
                HorizontalTextAlignment = TextAlignment.End
            }.Column(0),
            new SfSwitch().Bind(SfSwitch.IsOnProperty!, nameof(ViewTirage.ThemeDark)).Column(1)
        }
    };

    private Grid VueAuto => new() {
        ColumnDefinitions = Columns.Define(GridLength.Star, GridLength.Star),
        Children = {
            new Label {
                Text = "Auto:", VerticalTextAlignment = TextAlignment.Center,
                HorizontalTextAlignment = TextAlignment.End
            }.Column(0),
            new SfSwitch().Bind(SfSwitch.IsOnProperty!, nameof(ViewTirage.Auto)).Column(1)
        }
    };

    private CollectionView VueGollectionSolutions => new CollectionView {
            HeightRequest = 400, ItemSizingStrategy = ItemSizingStrategy.MeasureAllItems,
            ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepScrollOffset, SelectionMode = SelectionMode.Single,
            VerticalScrollBarVisibility = ScrollBarVisibility.Always,
            ItemsLayout = new GridItemsLayout(ItemsLayoutOrientation.Vertical) { Span = 4 },
            ItemTemplate = new DataTemplate(() =>  VueSolutionsDetail)
        }
        .Bind(IsVisibleProperty, nameof(ViewTirage.VueGrille))
        .Bind(ItemsView.ItemsSourceProperty, "Tirage.Solutions")
        .Invoke(collectionView => collectionView.SelectionChanged += (sender, _) => {
            if (sender is CollectionView { SelectedItem: CebBase sol }) ViewTirage.ShowPopup(sol);
        });


    private Border VueSolutionsDetail => Borderize(
        new CollectionView {
                HeightRequest = 100, HorizontalOptions = LayoutOptions.Center, SelectionMode = SelectionMode.Single,
                ItemTemplate = new DataTemplate(() =>
                    new Label { HorizontalTextAlignment = TextAlignment.Center }.Bind(Label.TextProperty))
            }.Bind(ItemsView.ItemsSourceProperty, "Operations")
            .Invoke(collection => collection.SelectionChanged += (sender, _) => {
                if (sender is CollectionView { BindingContext: CebBase sol })
                    ViewTirage.ShowPopup(sol);
            }));


    private SfDataGrid VueDataGridSolutions => new SfDataGrid {
            HeightRequest = 400, AutoGenerateColumnsMode = AutoGenerateColumnsMode.None,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Always, ColumnWidthMode = ColumnWidthMode.Fill,
            SelectionMode = DataGridSelectionMode.Single, EnableDataVirtualization = true,
            Columns = [
                new DataGridTextColumn { HeaderText = "Opération 1", MappingName = "Op1" },
                new DataGridTextColumn { HeaderText = "Opération 2", MappingName = "Op2" },
                new DataGridTextColumn { HeaderText = "Opération 3", MappingName = "Op3" },
                new DataGridTextColumn { HeaderText = "Opération 4", MappingName = "Op4" },
                new DataGridTextColumn { HeaderText = "Opération 5", MappingName = "Op5" }
            ]
        }
        .Bind(IsVisibleProperty, nameof(ViewTirage.VueGrille), BindingMode.Default, _invertedBoolConverter)
        .Bind(SfDataGrid.ItemsSourceProperty, "Tirage.Solutions")
        .Invoke(datagrid => datagrid.SelectionChanged += (sender, _) => {
            if (sender is SfDataGrid { SelectedRow: CebBase sol })
                ViewTirage.ShowPopup(sol);
        });


    private SfBusyIndicator VueBuzyindicator => new SfBusyIndicator {
            AnimationType = AnimationType.DoubleCircle, IndicatorColor = Colors.Blue, HeightRequest = 400, ZIndex = 99
        }
        .Bind(IsVisibleProperty, nameof(ViewTirage.IsBusy))
        .Bind(SfBusyIndicator.IsRunningProperty, nameof(ViewTirage.IsBusy));

    private SfPopup VuePopup => new SfPopup {
            Margin = 4, AutoSizeMode = PopupAutoSizeMode.Height, WidthRequest = 400, ShowFooter = false,
            ShowHeader = false, AnimationDuration = 1, AnimationMode = PopupAnimationMode.SlideOnBottom,
            AnimationEasing = PopupAnimationEasing.SinOut, VerticalOptions = LayoutOptions.Fill, PopupStyle =
                new PopupStyle { CornerRadius = 10, StrokeThickness = 2 }.Bind(PopupStyle.StrokeProperty,
                    nameof(ViewTirage.Foreground)),
            ContentTemplate = new DataTemplate(() => {
                return new VerticalStackLayout {
                    Margin = 4, HorizontalOptions = LayoutOptions.Center, Spacing = 0.5, Children = {
                        new VerticalStackLayout {
                            Padding = 2, VerticalOptions = LayoutOptions.Start, Children = {
                                new Label()
                                    .FontSize(24)
                                    .CenterHorizontal()
                                    .CenterVertical()
                                    .Bind(Label.TextProperty, nameof(ViewTirage.Result))
                                    .Bind(Label.TextColorProperty, nameof(ViewTirage.Foreground))
                            }
                        },
                        new BoxView { HeightRequest = 2, CornerRadius = 0 }.Bind(BoxView.ColorProperty,
                            nameof(ViewTirage.Foreground)),
                        new VerticalStackLayout {
                            VerticalOptions = LayoutOptions.Center, Children = {
                                new CollectionView {
                                    Margin = 4, VerticalOptions = LayoutOptions.Center, ItemTemplate = new DataTemplate(
                                        () => new Label { HorizontalTextAlignment = TextAlignment.Center }
                                            .Bind(Label.TextProperty))
                                }.Bind(ItemsView.ItemsSourceProperty, "Solution.Operations")
                            }
                        },
                        new BoxView { HeightRequest = 2, CornerRadius = 0 }.Bind(BoxView.ColorProperty,
                            nameof(ViewTirage.Foreground)),
                        new VerticalStackLayout {
                            HorizontalOptions = LayoutOptions.Fill, Children = {
                                new VerticalStackLayout {
                                    HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Center,
                                    Children = {
                                        new Label {
                                                Margin = 2, FontSize = 15,
                                                HorizontalTextAlignment = TextAlignment.Center
                                            }
                                            .FormattedText(new Span { Text = "Trouvé = " },
                                                new Span()
                                                    .Bind(Span.TextProperty, "Tirage.Found")
                                                    .Bind(Label.TextColorProperty, nameof(ViewTirage.Foreground))),
                                        new Label {
                                                FontSize = 15, Margin = 2,
                                                HorizontalTextAlignment = TextAlignment.Center
                                            }
                                            .FormattedText(new Span { Text = "Nombre de solutions = " },
                                                new Span()
                                                    .Bind(Span.TextProperty, "Tirage.Count")
                                                    .Bind(Label.TextColorProperty,
                                                        nameof(ViewTirage.Foreground))),
                                        new Label {
                                                Margin = 2, FontSize = 15,
                                                HorizontalTextAlignment = TextAlignment.Center
                                            }
                                            .FormattedText(new Span { Text = "Durée = " },
                                                new Span()
                                                    .Bind(Span.TextProperty, "Tirage.Duree.TotalSeconds",
                                                        stringFormat: "{0:N3} s")
                                                    .Bind(Span.TextColorProperty, nameof(ViewTirage.Foreground)))
                                    }
                                }
                            }
                        }
                    }
                };
            })
        }
        .Bind(SfPopup.IsOpenProperty, nameof(ViewTirage.Popup));

    private View VueResultat => 
        new Grid {
            ColumnDefinitions = Columns.Define(Star, Star, Star, Star), HeightRequest = 50,
            VerticalOptions = LayoutOptions.Center, Children = {
                new Label { VerticalOptions = LayoutOptions.Center }
                    .FontSize(18)
                    .Bind(Label.TextProperty, nameof(ViewTirage.Result))
                    .Bind(IsVisibleProperty, nameof(ViewTirage.IsComputed))
                    .Bind(Label.TextColorProperty, nameof(ViewTirage.Foreground))
                    .Column(0),
                new Label { VerticalOptions = LayoutOptions.Center }
                    .FontSize(18)
                    .Bind(Label.TextProperty, "Tirage.Found", stringFormat: "Trouvé: {0}")
                    .Bind(IsVisibleProperty, nameof(ViewTirage.IsComputed))
                    .Bind(Label.TextColorProperty, nameof(ViewTirage.Foreground))
                    .Column(1),
                new Label { VerticalOptions = LayoutOptions.Center }
                    .FontSize(18)
                    .Bind(Label.TextProperty, "Tirage.Count", stringFormat: "Nombre de solutions: {0}")
                    .Bind(IsVisibleProperty, nameof(ViewTirage.IsComputed))
                    .Bind(Label.TextColorProperty, nameof(ViewTirage.Foreground))
                    .Column(2),
                new Label { VerticalOptions = LayoutOptions.Center }
                    .FontSize(18)
                    .Bind(Label.TextProperty, "Tirage.Duree.TotalSeconds", stringFormat: "Durée: {0:N3} s")
                    .Bind(IsVisibleProperty, nameof(ViewTirage.IsComputed))
                    .Bind(Label.TextColorProperty, nameof(ViewTirage.Foreground))
                    .Column(3)
            }
        };

    public Border Borderize(View content) => new Border { StrokeThickness = 0.5}
        .Content(content)
        .Margin(new Thickness(2, 4))
        .Bind(IsEnabledProperty,
            nameof(ViewTirage.IsBusy), BindingMode.OneWay, _invertedBoolConverter)
        .Bind(Border.StrokeProperty, nameof(ViewTirage.Foreground));

    public ScrollView MainScrollView() => new() {
        BindingContext = ViewTirage, Content =
            new VerticalStackLayout { Children = {
                Borderize( VueSaisie), Borderize( VueResultat), Borderize( VueAction), Borderize( VueSolutions), VuePopup
            } }
    };

    private void AddPlaques(Grid g) {
        var comboStyle = new Style<SfComboBox>();
        comboStyle.Add(DropDownListBase.ItemsSourceProperty, ViewTirage.ListePlaques);
        comboStyle.Add(SfComboBox.HorizontalTextAlignmentProperty, TextAlignment.Center);
        comboStyle.Add(SfDropdownEntry.IsClearButtonVisibleProperty, false);
        comboStyle.AddAppThemeBinding(SfComboBox.ShowBorderProperty, true, false);
        comboStyle.Add(SfDropdownEntry.TextColorProperty, ViewTirage.Foreground);

        for (var i = 0; i < 6; i++)
            g.Add(
                new SfComboBox().Style(comboStyle)
                    .Bind(DropDownListBase.TextProperty, $"Tirage.Plaques[{i}].Value",
                        BindingMode.TwoWay)
                    .Column(i + 1));
    }

    public void DefineMenuContestuel(BindableObject bindable) {
        var menu = new MenuFlyout {
            new MenuFlyoutItem {
                CommandParameter = "random", Text = "Hasard",
                KeyboardAccelerators = {
                    new KeyboardAccelerator { Key = "H", Modifiers = KeyboardAcceleratorModifiers.Ctrl }
                }
            }.Bind(MenuItem.CommandProperty),
            new MenuFlyoutItem {
                CommandParameter = "resolve", Text = "Resoudre",
                KeyboardAccelerators = {
                    new KeyboardAccelerator { Key = "R", Modifiers = KeyboardAcceleratorModifiers.Ctrl }
                }
            }.Bind(MenuItem.CommandProperty),
            new MenuFlyoutSeparator(), new MenuFlyoutSubItem {
                new MenuFlyoutItem { CommandParameter = "excel" }
                    .Text("Excel")
                    .Bind(MenuItem.CommandProperty, nameof(ViewTirage.ExportsCommmand)),
                new MenuFlyoutItem { CommandParameter = "word" }
                    .Text("Word")
                    .Bind(MenuItem.CommandProperty,
                        nameof(ViewTirage.ExportsCommmand)),
                new MenuFlyoutItem { CommandParameter = "json" }
                    .Text("Json")
                    .Bind(MenuItem.CommandProperty,
                        nameof(ViewTirage.ExportsCommmand)),
                new MenuFlyoutItem { CommandParameter = "xml" }
                    .Text("Xml")
                    .Bind(MenuItem.CommandProperty,
                        nameof(ViewTirage.ExportsCommmand)),
                new MenuFlyoutItem { CommandParameter = "html" }
                    .Text("HTML")
                    .Bind(MenuItem.CommandProperty,
                        nameof(ViewTirage.ExportsCommmand))
            }.Text("Export"),
            new MenuFlyoutItem {
                CommandParameter = "vue", Text = "Inverser vue",
                KeyboardAccelerators = {
                    new KeyboardAccelerator { Key = "V", Modifiers = KeyboardAcceleratorModifiers.Ctrl }
                }
            }.Bind(MenuItem.CommandProperty),
            new MenuFlyoutItem {
                CommandParameter = "theme", Text = "Modifier le thème",
                KeyboardAccelerators = {
                    new KeyboardAccelerator { Key = "T", Modifiers = KeyboardAcceleratorModifiers.Ctrl }
                }
            }.Bind(MenuItem.CommandProperty),
            new MenuFlyoutItem {
                CommandParameter = "auto", Text = "Mode auto",
                KeyboardAccelerators = {
                    new KeyboardAccelerator { Key = "A", Modifiers = KeyboardAcceleratorModifiers.Ctrl }
                }
            }.Bind(MenuItem.CommandProperty)
        };
        FlyoutBase.SetContextFlyout(bindable, menu);
    }
}

internal static class MainPageExtentions {
    public static Border Content(this Border border, View content) {
        border.Content=content;
        return border;
    }
    

    
}
