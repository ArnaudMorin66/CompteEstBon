﻿using CebToolkit.ViewModel;

using CommunityToolkit.Maui.Markup;

using CompteEstBon;

using Syncfusion.Maui.Core;
using Syncfusion.Maui.Core.Converters;
using Syncfusion.Maui.DataGrid;
using Syncfusion.Maui.Inputs;
using Syncfusion.Maui.Inputs.DropDownControls;
using Syncfusion.Maui.Popup;

using static CommunityToolkit.Maui.Markup.GridRowsColumns;

using SelectionMode = Microsoft.Maui.Controls.SelectionMode;


namespace CebToolkit;

public class MainPage : ContentPage {
    private readonly InvertedBoolConverter _invertedBoolConverter = new();

    public MainPage() {
        Content = MainScrollView();
        DefineMenuContestuel(Content);
    }

    public ViewTirage ViewTirage { get; } = new();

    private View VueSolutions => new Grid {
        RowDefinitions = Rows.Define(Star),
        Children = { VueGollectionSolutions, VueDataGridSolutions, VueBuzyindicator }
    };

    public View VueSaisie =>
        new Grid {
                    ColumnDefinitions = Columns.Define(
                        Star, Stars(2), Stars(2), Stars(2), Stars(2), Stars(2), Stars(2), Star, Stars(3)),
                    Children = {
                        new Label()
                            .Text("Plaques")
                            .Margin(2)
                            .CenterVertical()
                            .Start()
                            .Column(0),
                        VuePlaques.Column(1).ColumnSpan(6),
                        new Label().Text("Chercher").CenterVertical().Column(7),
                        new SfNumericEntry {
                                CustomFormat = "000", Maximum = 999, Minimum = 100, HorizontalTextAlignment = TextAlignment.Center,
                                UpDownPlacementMode = NumericEntryUpDownPlacementMode.InlineVertical,

                            }
                            .CenterHorizontal()
                            .Bind(SfNumericEntry.ShowBorderProperty, nameof(ViewTirage.ThemeDark), BindingMode.TwoWay, _invertedBoolConverter)
                            .Column(8).Bind(SfNumericEntry.ValueProperty, "Tirage.Search")
                    }
                    
                }
                .CenterVertical();
        
    


    private View VueAction => new Grid {
        ColumnDefinitions = Columns.Define(
            GridLength.Star, GridLength.Star, GridLength.Star,
            GridLength.Star, GridLength.Star, GridLength.Star),
        Children = {
            new Button { CornerRadius = 5 }
                .Text("Résoudre")
                .BindCommand(parameterSource: "resolve")
                .Margin(2)
                .Column(0),
            new Button { CornerRadius = 5 }
                .BindCommand(parameterSource: "random")
                .Text("Hasard")
                .Margin(2)
                .Column(1),
            VueExport.Column(2), VueGridGrille.Column(3), VueTheme.Column(4), VueAuto.Column(5)
        }
    };


    private Grid VueGridGrille => new() {
        ColumnDefinitions =
            Columns.Define(GridLength.Star, new GridLength(2, GridUnitType.Star)),
        HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Center, Children = {
            new Label ()
            .Text("Grille:")
            .Bold()
            .FillHorizontal()
            .End()
            .CenterVertical()
            .BackgroundColor(Colors.Transparent)
                .Column(0),
            new Switch { OnColor = Colors.DarkViolet, ThumbColor = Colors.Green }
                .Bind(Switch.IsToggledProperty!, nameof(ViewTirage.VueGrille))
                .Column(1)
        }
    };

    private Grid VueExport => new() {
        ColumnDefinitions = Columns.Define(GridLength.Star, GridLength.Star), Children = {
            new Picker { ItemsSource = ViewTirage.ListeFormats }
                .Bind(Picker.SelectedItemProperty, nameof(ViewTirage.FmtExport), BindingMode.TwoWay)
                .Column(0),
            new Button { CornerRadius = 5 }
                .Text("Export")
                .Margin(2)
                .BindCommand(nameof(ViewTirage.ExportsCommmand), parameterSource:"export") //                )
                .Column(1)
        }
    };

    private Grid VueTheme {
        get {
            
            var grid =  new Grid {
                ColumnDefinitions = Columns.Define(GridLength.Star, GridLength.Star), Children = {
                    new Label()
                        .Text("Sombre:")
                        .Bold()
                        .FillHorizontal()
                        .End()
                        .CenterVertical()
                        .BackgroundColor(Colors.Transparent)
                        .Column(0),
                    new Switch {
                            OnColor = Colors.DarkViolet, ThumbColor = Colors.Green
                        }.Column(1)
                        .Bind(Switch.IsToggledProperty!, nameof(ViewTirage.ThemeDark)).Column(1)
                }
                
            };
            return grid;
        }
    }

    private Grid VueAuto => new() {
        ColumnDefinitions = Columns.Define(GridLength.Star, GridLength.Star),
        Children = {
            new Label ()
                .Text("Auto:")
                .Bold()
                .FillHorizontal()
                .End()
                .CenterVertical()
                .BackgroundColor(Colors.Transparent)
                .Column(0),
            new Switch { OnColor = Colors.DarkViolet, ThumbColor = Colors.Green }
                .Bind(Switch.IsToggledProperty!, nameof(ViewTirage.Auto)).Column(1)
        }
    };

    private CollectionView VueGollectionSolutions => new CollectionView {
            HeightRequest = 400, ItemSizingStrategy = ItemSizingStrategy.MeasureAllItems,
            ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepScrollOffset, SelectionMode = SelectionMode.Single,
            VerticalScrollBarVisibility = ScrollBarVisibility.Always,
            ItemsLayout = new GridItemsLayout(ItemsLayoutOrientation.Vertical) { VerticalItemSpacing = 2, Span = 4 },
            ItemTemplate = new DataTemplate(() => Borderize(Borderize(VueSolutionsDetail)))
        }
        .Bind(IsVisibleProperty, nameof(ViewTirage.VueGrille))
        .Bind(ItemsView.ItemsSourceProperty, "Tirage.Solutions")
        .Invoke(collectionView => collectionView.SelectionChanged += (sender, _) => {
            if (sender is CollectionView { SelectedItem: CebBase sol }) ViewTirage.ShowPopup(sol);
        });


    private View VueSolutionsDetail => new CollectionView {
            HeightRequest = 100, HorizontalOptions = LayoutOptions.Center, SelectionMode = SelectionMode.Single,
            ItemTemplate = new DataTemplate(() =>
                new Label { HorizontalTextAlignment = TextAlignment.Center }
                    .Bind(Label.TextProperty))
        }.Bind(ItemsView.ItemsSourceProperty, "Operations")
        .Invoke(collection => collection.SelectionChanged += (sender, _) => {
            if (sender is CollectionView { BindingContext: CebBase sol })
                ViewTirage.ShowPopup(sol);
        });


    private SfDataGrid VueDataGridSolutions => new SfDataGrid {
            HeightRequest = 400, AutoGenerateColumnsMode = AutoGenerateColumnsMode.None,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Always, ColumnWidthMode = ColumnWidthMode.Fill,
            EnableDataVirtualization = true, SelectionMode = DataGridSelectionMode.Single,
            Columns = [
                new DataGridTextColumn { HeaderText = "Opération 1", MappingName = "Op1" },
                new DataGridTextColumn { HeaderText = "Opération 2", MappingName = "Op2" },
                new DataGridTextColumn { HeaderText = "Opération 3", MappingName = "Op3" },
                new DataGridTextColumn { HeaderText = "Opération 4", MappingName = "Op4" },
                new DataGridTextColumn { HeaderText = "Opération 5", MappingName = "Op5" }
            ],
            
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

    private View VueResultat => new Grid {
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

    public Border Borderize(View content) => new Border { StrokeThickness = 0.5 }
        .Content(content)
        .Margin(new Thickness(2, 4))
        .Bind(IsEnabledProperty,
            nameof(ViewTirage.IsBusy), BindingMode.OneWay, _invertedBoolConverter)
        .Bind(Border.StrokeProperty, nameof(ViewTirage.Foreground));

    public ScrollView MainScrollView() => new() {
        BindingContext = ViewTirage, Content =
            new VerticalStackLayout {
                Children = {
                    Borderize(VueSaisie), Borderize(VueResultat), Borderize(VueAction), Borderize(VueSolutions),
                    VuePopup
                }
            }
    };

    private Grid VuePlaques {
        get {
            var comboStyle = new Style<SfComboBox>();
            comboStyle.Add(DropDownListBase.ItemsSourceProperty, ViewTirage.ListePlaques);
            comboStyle.Add(SfComboBox.HorizontalTextAlignmentProperty, TextAlignment.Center);
            comboStyle.Add(SfDropdownEntry.IsClearButtonVisibleProperty, false);
            comboStyle.AddAppThemeBinding(SfComboBox.ShowBorderProperty, true, false);
            comboStyle.Add(SfDropdownEntry.TextColorProperty, ViewTirage.Foreground);
            var g = new Grid() { ColumnDefinitions = Columns.Define(Star, Star, Star, Star, Star, Star) };
            for (var i = 0; i < 6; i++)
                g.Add(
                    new SfComboBox().Style(comboStyle)
                        .Bind(DropDownListBase.TextProperty, $"Tirage.Plaques[{i}].Value",
                            BindingMode.TwoWay)
                        .Column(i));
            return g;
        }
    }

    public void DefineMenuContestuel(BindableObject bindable) {
        var menu = new MenuFlyout {
            new MenuFlyoutItem {
                    KeyboardAccelerators = {
                        new KeyboardAccelerator { Key = "H", Modifiers = KeyboardAcceleratorModifiers.Ctrl }
                    }
                }
                .Text("Hasard")
                .BindCommand(parameterSource: "random"),
            new MenuFlyoutItem {
                    KeyboardAccelerators = {
                        new KeyboardAccelerator { Key = "R", Modifiers = KeyboardAcceleratorModifiers.Ctrl }
                    }
                }
                .BindCommand(parameterSource: "resolve")
                .Text("Résoudre"),
            new MenuFlyoutSeparator(), new MenuFlyoutSubItem {
                new MenuFlyoutItem {
                    KeyboardAccelerators = {
                            new KeyboardAccelerator { Key = "X", Modifiers = KeyboardAcceleratorModifiers.Ctrl |KeyboardAcceleratorModifiers.Shift }
                        }
                        
                    }
                    .Text("Excel")
                    .BindCommand(nameof(ViewTirage.ExportsCommmand), parameterSource: "excel"),
                new MenuFlyoutItem(){
                        KeyboardAccelerators = {
                            new KeyboardAccelerator { Key = "W", Modifiers = KeyboardAcceleratorModifiers.Ctrl |KeyboardAcceleratorModifiers.Shift }
                        }

                    }
                    .Text("Word")
                    .BindCommand(nameof(ViewTirage.ExportsCommmand), parameterSource: "word"),
                new MenuFlyoutItem(){
                        KeyboardAccelerators = {
                            new KeyboardAccelerator { Key = "J", Modifiers = KeyboardAcceleratorModifiers.Ctrl |KeyboardAcceleratorModifiers.Shift }
                        }

                    }
                    .Text("Json")
                    .BindCommand(nameof(ViewTirage.ExportsCommmand), parameterSource: "json"),
                new MenuFlyoutItem(){
                        KeyboardAccelerators = {
                            new KeyboardAccelerator { Key = "M", Modifiers = KeyboardAcceleratorModifiers.Ctrl |KeyboardAcceleratorModifiers.Shift }
                        }

                    }
                    .Text("Xml")
                    .BindCommand(nameof(ViewTirage.ExportsCommmand), parameterSource: "xml"),
                new MenuFlyoutItem(){
                        KeyboardAccelerators = {
                            new KeyboardAccelerator { Key = "H", Modifiers = KeyboardAcceleratorModifiers.Ctrl |KeyboardAcceleratorModifiers.Shift }
                        }

                    }
                    .Text("HTML")
                    .BindCommand(nameof(ViewTirage.ExportsCommmand), parameterSource: "html")
            }.Text("Export"),
            new MenuFlyoutItem {
                    KeyboardAccelerators = {
                        new KeyboardAccelerator { Key = "V", Modifiers = KeyboardAcceleratorModifiers.Ctrl }
                    }
                }
                .Text("Inverser vue").BindCommand(parameterSource: "vue"),
            new MenuFlyoutItem {
                KeyboardAccelerators = {
                    new KeyboardAccelerator { Key = "T", Modifiers = KeyboardAcceleratorModifiers.Ctrl }
                }
            }.Text("Thème").BindCommand(parameterSource: "theme"),
            new MenuFlyoutItem {
                KeyboardAccelerators = {
                    new KeyboardAccelerator { Key = "A", Modifiers = KeyboardAcceleratorModifiers.Ctrl }
                }
            }.Text("Auto").BindCommand(parameterSource: "auto")
        };
        FlyoutBase.SetContextFlyout(bindable, menu);
    }
}

internal static class MainPageExtentions {
    public static Border Content(this Border border, View content) {
        border.Content = content;
        return border;
    }
}