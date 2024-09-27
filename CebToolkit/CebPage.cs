#region using

using CommunityToolkit.Maui.Converters;
using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Maui.Markup.LeftToRight;

using Syncfusion.Maui.Buttons;
using Syncfusion.Maui.Core;
using Syncfusion.Maui.DataGrid;
using Syncfusion.Maui.Inputs;
using Syncfusion.Maui.Inputs.DropDownControls;
using Syncfusion.Maui.Popup;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;
using SelectionMode = Microsoft.Maui.Controls.SelectionMode;

using CompteEstBon;
using CebToolkit.ViewModel;

#endregion

namespace CebToolkit;

public class  CebPage : ContentPage {
    private readonly InvertedBoolConverter _invertedBoolConverter = new();
    

    public CebPage() {
#if WINDOWS
        Application.Current!.Windows[0].TitleBar = VueTitleBar;
#endif
        BindingContext = ViewTirage;
        FlyoutBase.SetContextFlyout(this, MenuContext);
        Content = MainScrollView;
    }

    private ViewTirage ViewTirage { get; } = new();

    private View VueSolutions => new Grid {
        RowDefinitions = Rows.Define(Star),
        Children = {
            VueGollectionSolutions,
            VueDataGridSolutions,
            VueBuzyindicator
        }
    };

    private View VueSaisie => new Grid {
            ColumnDefinitions = Columns.Define(
                Star, Stars(2), Stars(2), Stars(2), Stars(2), Stars(2), Stars(2), Star, Stars(2)),
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
                        CustomFormat = "000",
                        Maximum = 999,
                        Minimum = 100,
                        HorizontalTextAlignment = TextAlignment.Center,
                        VerticalTextAlignment = TextAlignment.Center,
                        UpDownPlacementMode = NumericEntryUpDownPlacementMode.InlineVertical
                    }
                    .CenterHorizontal()
                    .Bind(SfNumericEntry.TextColorProperty, nameof(ViewTirage.Foreground))
                    .AppThemeBinding(SfNumericEntry.ShowBorderProperty, true, false)
                    .Column(8).Bind(SfNumericEntry.ValueProperty, "Tirage.Search")
            }
        }
        .CenterVertical();


    private View VueAction => new Grid {
#if WINDOWS
        ColumnDefinitions = Columns.Define(Stars(2), Stars(2), Star),
#else
        ColumnDefinitions = Columns.Define(
            Star, Star, Star, Star, Star, Star),
#endif
        Children = {
            new Button() 
                .Text("Résoudre")
                .BindCommand(nameof(ViewTirage.CebCommand), parameterSource: "resolve")
                .Column(0),
            new Button ()
                .BindCommand(nameof(ViewTirage.CebCommand), parameterSource: "random")
                .Text("Hasard")
                .Column(1),
            VueOptionExport.Column(2),
#if !WINDOWS
            VueOptionGrille.Column(3),
            VueOptionTheme.Column(4),
            VueOptionAuto.Column(5)
#endif
        }
    };


    private Grid VueOptionGrille => new() {
        ColumnDefinitions =
            Columns.Define(Star, Stars(2)),
        HorizontalOptions = LayoutOptions.Fill,
        VerticalOptions = LayoutOptions.Center,
        Children = {
            new Label()
                .Text("Grille:")
                .Bold()
                .FillHorizontal()
                .End()
                .CenterVertical()
                .BackgroundColor(Colors.Transparent)
                .Column(0),
            new SfSwitch()
                .Bind(SfSwitch.IsOnProperty!, nameof(ViewTirage.VueGrille), source:ViewTirage)
                .Column(1)
        }
    };

    private Grid VueOptionExport => new() {
        ColumnDefinitions = Columns.Define(Star, Star),
        Children = {
            new Picker() {
                    ItemsSource = ViewTirage.ListeFormats
                }
                .BackgroundColor(Colors.Transparent)
                .Bind(Picker.SelectedItemProperty,  nameof(ViewTirage.FmtExport), BindingMode.TwoWay)
                .Column(0),
            
            new Button()
                .Text("Export")
                .BindCommand(nameof(ViewTirage.ExportCommand), parameterSource: "export") //                )
                .Column(1)
        }
    };

    private Grid VueOptionTheme => new() {
        ColumnDefinitions = Columns.Define(Star, Star),
        Children = {
            new Label()
                .Text("Sombre:")
                .Bold()
                .FillHorizontal()
                .End()
                .CenterVertical()
                .BackgroundColor(Colors.Transparent)
                .Column(0),
            new SfSwitch()
                .Column(1)
                .Bind(SfSwitch.IsOnProperty!, nameof(ViewTirage.ThemeDark), source:ViewTirage).Column(1)
        }
    };


    private Grid VueOptionAuto => new() {
        ColumnDefinitions = Columns.Define(Star, Star),
        Children = {
            new Label()
                .Text("Auto:")
                .Bold()
                .FillHorizontal()
                .End()
                .CenterVertical()
                .BackgroundColor(Colors.Transparent)
                .Column(0),
            new SfSwitch()
                .Bind(SfSwitch.IsOnProperty!, nameof(ViewTirage.Auto), source:ViewTirage).Column(1)
        }
    };

    private CollectionView VueGollectionSolutions => new CollectionView {
            HeightRequest = 400,
            ItemSizingStrategy = ItemSizingStrategy.MeasureAllItems,
            ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepScrollOffset,
            SelectionMode = SelectionMode.Single,
            VerticalScrollBarVisibility = ScrollBarVisibility.Always,
            ItemsLayout = new GridItemsLayout(ItemsLayoutOrientation.Vertical) {
                VerticalItemSpacing = 2,
                Span = 4
            },
            ItemTemplate = new DataTemplate(() => VueSolutionsDetail)
        }
        .Bind(IsVisibleProperty, nameof(ViewTirage.VueGrille))
        .Bind(ItemsView.ItemsSourceProperty, "Tirage.Solutions")
        .Invoke(collectionView => collectionView.SelectionChanged += (sender, _) => {
            if (sender is CollectionView { SelectedItem: CebBase sol }) ViewTirage.ShowPopup(sol);
        });


    private View VueSolutionsDetail => new CollectionView {
            HeightRequest = 100,
            HorizontalOptions = LayoutOptions.Center,
            SelectionMode = SelectionMode.Single,
            ItemTemplate = new DataTemplate(() =>
                new Label() 
                    .TextCenterHorizontal()
                    .Fill()
                    .Bind(Label.TextProperty))
        }.Bind(ItemsView.ItemsSourceProperty, "Operations")
        .AppThemeColorBinding(CollectionView.BackgroundColorProperty,Colors.CadetBlue, Color.FromArgb("2f4f4f"))
        .Center()
        .Invoke(collection => collection.SelectionChanged += (sender, _) => {
            if (sender is CollectionView { BindingContext: CebBase sol }) ViewTirage.ShowPopup(sol);
        });


    private SfDataGrid VueDataGridSolutions => new SfDataGrid {
            HeightRequest = 400,
            AutoGenerateColumnsMode = AutoGenerateColumnsMode.None,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Always,
            ColumnWidthMode = ColumnWidthMode.Fill,
            EnableDataVirtualization = true,
            SelectionMode = DataGridSelectionMode.Single,
            Columns = [
                new DataGridTextColumn {
                    HeaderText = "Opération 1",
                    MappingName = "Op1"
                    
                },
                new DataGridTextColumn {
                    HeaderText = "Opération 2",
                    MappingName = "Op2"
                },
                new DataGridTextColumn {
                    HeaderText = "Opération 3",
                    MappingName = "Op3"
                },
                new DataGridTextColumn {
                    HeaderText = "Opération 4",
                    MappingName = "Op4"
                },
                new DataGridTextColumn {
                    HeaderText = "Opération 5",
                    MappingName = "Op5"
                }
            ],
            DefaultStyle = new DataGridStyle()
                .Bind(DataGridStyle.RowTextColorProperty, nameof(ViewTirage.Foreground))
                .AppThemeColorBinding(DataGridStyle.AlternateRowBackgroundProperty, Colors.DarkSeaGreen,
                    Colors.DarkSlateGray)
                .AppThemeColorBinding(DataGridStyle.HeaderRowBackgroundProperty, Colors.SlateGrey, Colors.DarkGreen)
                .AppThemeColorBinding(DataGridStyle.HeaderRowTextColorProperty, Colors.Yellow, Colors.White)
        }
        .Bind(IsVisibleProperty, nameof(ViewTirage.VueGrille), BindingMode.Default, _invertedBoolConverter)
        .Bind(SfDataGrid.ItemsSourceProperty, "Tirage.Solutions")
        .Invoke(datagrid => datagrid.SelectionChanged += (sender, _) => {
            if (sender is SfDataGrid { SelectedRow: CebBase sol }) ViewTirage.ShowPopup(sol);
        });


    private SfBusyIndicator VueBuzyindicator => new SfBusyIndicator {
            AnimationType = AnimationType.DoubleCircle,
            IndicatorColor = Colors.Blue,
            HeightRequest = 400,
            ZIndex = 99
        }
        .Bind(IsVisibleProperty, nameof(ViewTirage.IsBusy))
        .Bind(SfBusyIndicator.IsRunningProperty, nameof(ViewTirage.IsBusy));

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
                nameof(ViewTirage.Foreground)),
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
                            .Bind(Label.TextColorProperty, nameof(ViewTirage.Foreground))
                    }
                },
                new BoxView {
                    HeightRequest = 2,
                    CornerRadius = 0
                }.Bind(BoxView.ColorProperty,
                    nameof(ViewTirage.Foreground)),
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
                    nameof(ViewTirage.Foreground)),
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
                            .Bind(Label.TextColorProperty, nameof(ViewTirage.Foreground))
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
                            .Bind(Label.TextColorProperty, nameof(ViewTirage.Foreground))
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
                            .Bind(Label.TextColorProperty, nameof(ViewTirage.Foreground))
                            .Row(2).Column(1),

                    }
                }
            }
        })
            }
            .Bind(SfPopup.IsOpenProperty, nameof(ViewTirage.Popup));
    

            private View VueResultat => new Grid {
        ColumnDefinitions = Columns.Define(Star, Star, Star, Star),
        RowDefinitions = Rows.Define(Star),
        HeightRequest = 50,
        VerticalOptions = LayoutOptions.Center,
        Children = {
            new Label ()
                .CenterVertical()
                .FontSize(18)
                .Bind(Label.TextProperty, nameof(ViewTirage.Result))
                .Bind(Label.TextColorProperty, nameof(ViewTirage.Foreground))
                .Column(0),
            new Label()
                .CenterVertical()
                .FontSize(18)
                .Bind(Label.TextProperty, "Tirage.Found", stringFormat: "Trouvé: {0}")
                .Bind(Label.TextColorProperty, nameof(ViewTirage.Foreground))
                .Column(1),
            new Label()
                .CenterVertical()
                .FontSize(18)
                .Bind(Label.TextProperty, "Tirage.Count", stringFormat: "Nombre de solutions: {0}")
                .Bind(Label.TextColorProperty, nameof(ViewTirage.Foreground))
                .Column(2),
            new Label() 
                .CenterVertical()
                .FontSize(18)
                .Bind(Label.TextProperty, "Tirage.Duree.TotalSeconds", stringFormat: "Durée: {0:N3} s")
                .Bind(Label.TextColorProperty, nameof(ViewTirage.Foreground))
                .Column(3)
        }
    }.Bind(Grid.IsVisibleProperty, nameof(ViewTirage.IsComputed));


    private ScrollView MainScrollView => new() {
        Content =
            new VerticalStackLayout {
                Children = {
                    Borderize(VueSaisie),
                    Borderize(VueResultat),
                    Borderize(VueAction),
                    Borderize(VueSolutions),
                    VuePopup
                }
            }
    };


    private Grid VuePlaques => new Grid {
        ColumnDefinitions = Columns.Define(Star, Star, Star, Star, Star, Star)
    }.Invoke(g => {
        for (var i = 0; i < 6; i++)
            g.Add(
                new SfComboBox() {
                        ItemsSource = ViewTirage.ListePlaques
                    }
                    .Bind(DropDownListBase.TextProperty, $"Tirage.Plaques[{i}].Value",
                        BindingMode.TwoWay)
                    .DynamicResource(SfComboBox.StyleProperty, "SfComboboxStyle")
                    .Column(i));
    });

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
            .BindCommand(nameof(ViewTirage.CebCommand), parameterSource: "random"),
        new MenuFlyoutItem {
                KeyboardAccelerators = {
                    new KeyboardAccelerator {
                        Key = "R",
                        Modifiers = KeyboardAcceleratorModifiers.Ctrl
                    }
                }
            }
            .BindCommand(nameof(ViewTirage.CebCommand),parameterSource: "resolve")
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
                .BindCommand(nameof(ViewTirage.ExportCommand), parameterSource: "excel"),
            new MenuFlyoutItem {
                    KeyboardAccelerators = {
                        new KeyboardAccelerator {
                            Key = "W",
                            Modifiers = KeyboardAcceleratorModifiers.Ctrl | KeyboardAcceleratorModifiers.Shift
                        }
                    }
                }
                .Text("Word")
                .BindCommand(nameof(ViewTirage.ExportCommand), parameterSource: "word"),
            new MenuFlyoutItem {
                    KeyboardAccelerators = {
                        new KeyboardAccelerator {
                            Key = "J",
                            Modifiers = KeyboardAcceleratorModifiers.Ctrl | KeyboardAcceleratorModifiers.Shift
                        }
                    }
                }
                .Text("Json")
                .BindCommand(nameof(ViewTirage.ExportCommand), parameterSource: "json"),
            new MenuFlyoutItem {
                    KeyboardAccelerators = {
                        new KeyboardAccelerator {
                            Key = "M",
                            Modifiers = KeyboardAcceleratorModifiers.Ctrl | KeyboardAcceleratorModifiers.Shift
                        }
                    }
                }
                .Text("Xml")
                .BindCommand(nameof(ViewTirage.ExportCommand), parameterSource: "xml"),
            new MenuFlyoutItem {
                    KeyboardAccelerators = {
                        new KeyboardAccelerator {
                            Key = "H",
                            Modifiers = KeyboardAcceleratorModifiers.Ctrl | KeyboardAcceleratorModifiers.Shift
                        }
                    }
                }
                .Text("HTML")
                .BindCommand(nameof(ViewTirage.ExportCommand), parameterSource: "html")
        }.Text("Export"),
        new MenuFlyoutItem {
                KeyboardAccelerators = {
                    new KeyboardAccelerator {
                        Key = "V",
                        Modifiers = KeyboardAcceleratorModifiers.Ctrl
                    }
                }
            }
            .Text("Inverser vue").BindCommand(nameof(ViewTirage.CebCommand),parameterSource: "vue"),
        new MenuFlyoutItem {
            KeyboardAccelerators = {
                new KeyboardAccelerator {
                    Key = "T",
                    Modifiers = KeyboardAcceleratorModifiers.Ctrl
                }
            }
        }.Text("Thème").BindCommand(nameof(ViewTirage.CebCommand),parameterSource: "theme"),
        new MenuFlyoutItem {
            KeyboardAccelerators = {
                new KeyboardAccelerator {
                    Key = "A",
                    Modifiers = KeyboardAcceleratorModifiers.Ctrl
                }
            }
        }.Text("Auto").BindCommand(nameof(ViewTirage.CebCommand),parameterSource: "auto")
    ];

    private Border Borderize(View content) => new Border {
            StrokeThickness = 0.5,
            MinimumHeightRequest = 50,
            Content = content
        }
        .Margin(new Thickness(2, 4))
        .Bind(IsEnabledProperty,
            nameof(ViewTirage.IsBusy), BindingMode.OneWay, _invertedBoolConverter)
        .Bind(Border.StrokeProperty, nameof(ViewTirage.Foreground));

#if WINDOWS
    private TitleBar VueTitleBar => new TitleBar() {
        Icon = "favicon.ico",
        HorizontalOptions = LayoutOptions.Center,
        TrailingContent = new Grid()
        { 
            HeightRequest=32,
            ColumnDefinitions = Columns.Define(Stars(2),Stars(2), Stars(2),Star),
            VerticalOptions = LayoutOptions.Center,
            Children = {
                VueOptionTheme.Column(0),
                VueOptionGrille.Column(1),
                VueOptionAuto.Column(2),
            new Label().CenterVertical().Bind(Label.TextProperty, "Date", source: ViewTirage)
                .Column(3),
            
        } }

    }.Bind(TitleBar.ForegroundColorProperty, "Foreground", source: ViewTirage)
    .Bind(TitleBar.TitleProperty, nameof(ViewTirage.Result), source:ViewTirage);
#endif
}
