#region using

using CebToolkit.ViewModel;

using CommunityToolkit.Maui.Converters;
using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Maui.Markup.LeftToRight;

using CompteEstBon;

using Syncfusion.Maui.Buttons;
using Syncfusion.Maui.Core;
using Syncfusion.Maui.DataGrid;
using Syncfusion.Maui.Inputs;
using Syncfusion.Maui.Inputs.DropDownControls;
using Syncfusion.Maui.Popup;

using static CommunityToolkit.Maui.Markup.GridRowsColumns;

using SelectionMode = Microsoft.Maui.Controls.SelectionMode;

#endregion

namespace CebToolkit;

public class CebPage : ContentPage {
    private readonly InvertedBoolConverter invertedBoolConverter = new();


    public CebPage() {
#if WINDOWS
        Application.Current!.Windows[0].TitleBar = VueTitleBar;
#endif
        BindingContext = ViewTirage;
        FlyoutBase.SetContextFlyout(this, MenuContext);
        Content = MainScrollView;
        this.AppThemeColorBinding(BackgroundColorProperty, Color.FromArgb("8fbc8f"), Colors.DarkSlateGrey);
    }

    private ViewTirage ViewTirage { get; } = App.Current.Services.GetService<ViewTirage>()!;

    private View VueSolutions => new Grid {
        RowDefinitions = Rows.Define(Star),
        Children = {
            VueGollectionSolutions,
            Vueindicator,
            VueDataGridSolutions
        }
    };
#if WINDOWS
    private View VueSaisie => new Grid {
            ColumnDefinitions = Columns.Define(
                 Star, Star, Star, Star, Star, Star, Star),
            Children = {

                VuePlaques.Column(0).ColumnSpan(6),
                new SfNumericEntry {
                        CustomFormat = "000",
                        Maximum = 999,
                        Minimum = 100,
                    }
                    .AppThemeBinding(SfNumericEntry.ShowBorderProperty, true, false)
                    .Column(6).Bind(SfNumericEntry.ValueProperty, "Tirage.Search")
            }
        }
        .CenterVertical();
#else
    private View VueSaisie => new Grid {
            RowDefinitions = Rows.Define(
                Star, Star, Star, Star),
            Children = {
                VuePlaques.Row(0).RowSpan(3),
                new SfNumericEntry {
                        CustomFormat = "000",
                        Maximum = 999,
                        Minimum = 100
                    }
                    .AppThemeBinding(SfNumericEntry.ShowBorderProperty, true, false)
                    .Row(4)
                    .Bind(SfNumericEntry.ValueProperty, "Tirage.Search")
            }
        }
        .CenterVertical();
#endif

    private View VueAction => new Grid {
#if WINDOWS
        ColumnDefinitions = Columns.Define(Stars(2), Stars(2), Star),
#else
        ColumnDefinitions = Columns.Define(Star, Star),
        RowDefinitions = Rows.Define(Star, Star),
#endif
        Children = {
            new Button {
                    ImageSource = new FileImageSource {
                        File = "resolve.png"
                    },
                    ContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Left, 10)
                }
                .Text("Résoudre")
                .BindCommand(nameof(ViewTirage.CebCommand), parameterSource: "resolve")
#if WINDOWS
                .Column(0),
#else
                .Column(0).Row(0),
#endif
            new Button {
                    ImageSource = new FileImageSource {
                        File = "random.png"
                    },
                    ContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Left, 10)
                }
                .BindCommand(nameof(ViewTirage.CebCommand), parameterSource: "random")
                .Text("Hasard")
                .Column(1)
#if !WINDOWS
                .Row(0)
#endif
            ,
            VueOptionExport
#if WINDOWS
                .Column(2),
#else
                .Column(0).Row(1).ColumnSpan(2)
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
                .TextColor(Colors.White)
                .CenterVertical()
                .Column(0),
            new SfSwitch()
                .Bind(SfSwitch.IsOnProperty!, nameof(ViewTirage.VueGrille), source: ViewTirage)
                .Column(1)
        }
    };

    private Grid VueOptionExport => new() {
        ColumnDefinitions = Columns.Define(Star, Star),
        Children = {
            new Picker {
                    ItemsSource = ViewTirage.ListeFormats
                }
                .TextColor(Colors.White)
                .Bind(Picker.SelectedItemProperty, nameof(ViewTirage.FmtExport), BindingMode.TwoWay)
                .Column(0),

            new Button {
                    ImageSource = new FileImageSource {
                        File = "excel.png"
                    },
                    ContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Left, 10)
                }
                .Text("Export")
                .BindCommand(nameof(ViewTirage.ExportCommand), parameterSource: "") //                )
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
                .TextColor(Colors.White)
                .CenterVertical()
                .Column(0),
            new SfSwitch()
                .Column(1)
                .Bind(SfSwitch.IsOnProperty!, nameof(ViewTirage.ThemeDark), source: ViewTirage).Column(1)
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
                .TextColor(Colors.White)
                .Column(0),
            new SfSwitch()
                .Bind(SfSwitch.IsOnProperty!, nameof(ViewTirage.Auto), source: ViewTirage).Column(1)
        }
    };

    private CollectionView VueGollectionSolutions => new CollectionView {
            ItemSizingStrategy = ItemSizingStrategy.MeasureAllItems,
            ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepScrollOffset,
            SelectionMode = SelectionMode.Single,
            VerticalScrollBarVisibility = ScrollBarVisibility.Always,
            ItemsLayout = new GridItemsLayout(ItemsLayoutOrientation.Vertical) {
                VerticalItemSpacing = 2,
#if WINDOWS
                Span = 4
#else
                Span = 2
#endif
            },
            ItemTemplate = new DataTemplate(() => Borderize(VueSolutionsDetail, true))
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
        .AppThemeColorBinding(BackgroundColorProperty, Colors.CadetBlue, Color.FromArgb("2f4f4f"))
        .Center()
        .Invoke(collection => collection.SelectionChanged += (sender, _) => {
            if (sender is CollectionView { BindingContext: CebBase sol }) ViewTirage.ShowPopup(sol);
        });


    private SfDataGrid VueDataGridSolutions => new SfDataGrid {
            MinimumHeightRequest = 400,
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
            ]
        }
        .Bind(IsVisibleProperty, nameof(ViewTirage.VueGrille), BindingMode.Default, invertedBoolConverter)
        .Bind(SfDataGrid.ItemsSourceProperty, "Tirage.Solutions")
        .DynamicResource(StyleProperty, "DatagridSolutionsStyle")
        .Invoke(datagrid => datagrid.SelectionChanged += (sender, _) => {
            if (sender is SfDataGrid { SelectedRow: CebBase sol }) ViewTirage.ShowPopup(sol);
        });

    private View Vueindicator => Borderize(new SfBusyIndicator {
                //HeightRequest = 200,
                //WidthRequest = 200,
                IndicatorColor = Colors.Blue,
                AnimationType = AnimationType.CircularMaterial,
                ZIndex = 99
            }
            .Bind(SfBusyIndicator.IsRunningProperty, nameof(ViewTirage.IsBusy)))
        .Bind(IsVisibleProperty, nameof(ViewTirage.IsBusy));

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
                                .Row(2).Column(1)
                        }
                    }
                }
            })
        }
        .Bind(SfPopup.IsOpenProperty, nameof(ViewTirage.Popup));


    private View VueResultat => new Grid {
#if WINDOWS
                ColumnDefinitions = Columns.Define(Star, Star, Star, Star),
        RowDefinitions = Rows.Define(Star),
#else
            ColumnDefinitions = Columns.Define(Star, Star),
            RowDefinitions = Rows.Define(Star, Star),
#endif

            VerticalOptions = LayoutOptions.Center,
            Children = {
                new Label()
                    .CenterVertical()
                    .Bold()
                    .Bind(Label.TextProperty, nameof(ViewTirage.Result))
                    .Bind(Label.TextColorProperty, nameof(ViewTirage.Foreground))
#if !WINDOWS
                    .Row(0)
#endif
                    .Column(0),
                new Label()
                    .Bold()
                    .CenterVertical()
                    .Bind(Label.TextProperty, "Tirage.Found", stringFormat: "Trouvé: {0}")
                    .Bind(Label.TextColorProperty, nameof(ViewTirage.Foreground))
#if !WINDOWS
                    .Row(0)
#endif
                    .Column(1),
                new Label()
                    .Bold()
                    .CenterVertical()
                    .Bind(Label.TextProperty, "Tirage.Count", stringFormat: "Nombre de solutions: {0}")
                    .Bind(Label.TextColorProperty, nameof(ViewTirage.Foreground))
#if WINDOWS
                .Column(2),
#else
                    .Column(0).Row(1),
#endif
                new Label()
                    .CenterVertical()
                    .Bold()
                    .Bind(Label.TextProperty, "Tirage.Duree.TotalSeconds", stringFormat: "Durée: {0:N3} s")
                    .Bind(Label.TextColorProperty, nameof(ViewTirage.Foreground))
#if WINDOWS
                .Column(3)
#else
                    .Column(1).Row(1)
#endif
            }
        }.Bind(IsVisibleProperty, nameof(ViewTirage.IsComputed))
        .AppThemeColorBinding(BackgroundColorProperty, Color.FromArgb("8fbc8f"), Colors.DarkSlateGrey);


    private ScrollView MainScrollView => new() {
        Content =
            new VerticalStackLayout {
                Children = {
                    Borderize(VueSaisie, true),
                    Borderize(VueAction, true),
                    Borderize(VueResultat, true),
                    VueSolutions,
                    VuePopup
                }
            }
    };


    private Grid VuePlaques => new Grid {
#if WINDOWS
        ColumnDefinitions = Columns.Define(Star, Star, Star, Star, Star, Star)
#else
        RowDefinitions = Rows.Define(Star, Star, Star),
        ColumnDefinitions = Columns.Define(Star, Star)
#endif
    }.Invoke(g => {
        for (var i = 0; i < 6; i++)
            g.Add(
                new SfComboBox {
                        ItemsSource = ViewTirage.ListePlaques
                    }
                    .Bind(DropDownListBase.TextProperty, $"Tirage.Plaques[{i}].Value",
                        BindingMode.TwoWay)
#if WINDOWS
                    .Column(i));
#else
                    .Row(i / 2).Column(i % 2));
#endif
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
            .BindCommand(nameof(ViewTirage.CebCommand), parameterSource: "resolve")
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
            .Text("Inverser vue").BindCommand(nameof(ViewTirage.CebCommand), parameterSource: "vue"),
        new MenuFlyoutItem {
            KeyboardAccelerators = {
                new KeyboardAccelerator {
                    Key = "T",
                    Modifiers = KeyboardAcceleratorModifiers.Ctrl
                }
            }
        }.Text("Thème").BindCommand(nameof(ViewTirage.CebCommand), parameterSource: "theme"),
        new MenuFlyoutItem {
            KeyboardAccelerators = {
                new KeyboardAccelerator {
                    Key = "A",
                    Modifiers = KeyboardAcceleratorModifiers.Ctrl
                }
            }
        }.Text("Auto").BindCommand(nameof(ViewTirage.CebCommand), parameterSource: "auto")
    ];

    private Border Borderize(View content, bool isShadow = false) => new Border {
            StrokeThickness = 0.5,
#if WINDOWS
        MinimumHeightRequest = 50,
#endif
            Content = content
        }
        .Margin(new Thickness(2, 4))
        .Bind(IsEnabledProperty,
            nameof(ViewTirage.IsBusy), BindingMode.OneWay, invertedBoolConverter)
        .Bind(Border.StrokeProperty, nameof(ViewTirage.Foreground))
        .Invoke(b => {
            if (isShadow) b.DynamicResource(StyleProperty, "BorderWithShadowStyle");
        });

#if WINDOWS
    private TitleBar VueTitleBar => new TitleBar() {
        Icon = "favicon.ico",
        Content = new Grid() {
            HeightRequest = 32,
            ColumnDefinitions = Columns.Define(Stars(2), Stars(2), Stars(2)),
            VerticalOptions = LayoutOptions.Center,
            Children = {
                VueOptionTheme.Column(0),
                VueOptionGrille.Column(1),
                VueOptionAuto.Column(2),}
        },
        TrailingContent = new Label()
                .TextColor(Colors.White).CenterVertical().Bind(Label.TextProperty, "Date", source: ViewTirage)
        
        

    }.Bind(TitleBar.ForegroundColorProperty, "Foreground", source: ViewTirage)
    .DynamicResource(TitleBar.BackgroundProperty, "LinearGradientBrushBase")
    .Bind(TitleBar.TitleProperty, nameof(ViewTirage.Result), source:ViewTirage);
#endif
}