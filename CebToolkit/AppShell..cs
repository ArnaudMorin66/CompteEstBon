using CebToolkit.ViewModel;

using CommunityToolkit.Maui.Markup;

using Syncfusion.Maui.Buttons;

using static CommunityToolkit.Maui.Markup.GridRowsColumns;


namespace CebToolkit;

/// <summary>
/// Represents the main application shell.
/// </summary>
public class AppShell : Shell {
    private static readonly ViewTirage TirageContext = App.Current.Services.GetService<ViewTirage>()!;

    /// <summary>
    /// Initializes a new instance of the <see cref="CebToolkit.AppShell"/> class.
    /// </summary>
    /// <remarks>
    /// This constructor sets up the main shell of the application, including the title, flyout background,
    /// toolbar items, and shell contents. It also configures platform-specific settings and binds the
    /// <see cref="CebToolkit.ViewModel.ViewTirage"/> view model to the shell.
    /// </remarks>
    public AppShell() {
        BindingContext = TirageContext;
        InitializeShell();
    }

    /// <summary>
    /// Gets the dark background color.
    /// </summary>
    public static Color BackgroundDark { get; } = App.FindResource<Color>("BackgroundDark")!;

    /// <summary>
    /// Gets the light background color.
    /// </summary>
    public static Color BackgroundLight { get; } = App.FindResource<Color>("BackgroundLight")!;

    /// <summary>
    /// Gets the view that represents the grid option in the configuration page.
    /// </summary>
    private static View VueOptionGrille => new Grid {
        ColumnDefinitions = Columns.Define(Star, Star),
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
                .Bind(SfSwitch.IsOnProperty!, nameof(TirageContext.VueGrille))
                .Column(1)
        }
    };

    /// <summary>
    /// Gets the view that represents the theme option in the configuration page.
    /// </summary>
    private static Grid VueOptionTheme => new() {
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
                .Column(1)
                .Bind(SfSwitch.IsOnProperty!, nameof(TirageContext.ThemeDark)).Column(1)
        }
    };

    /// <summary>
    /// Gets a <see cref="Grid"/> that provides options for automatic functionality.
    /// </summary>
    private static Grid VueOptionAuto => new() {
        ColumnDefinitions = Columns.Define(Star, Star),
        Children = {
            new Label()
                .Text("Auto:")
                .Bold()
                .FillHorizontal()
                .End()
                .CenterVertical()
                .Column(0),
            new SfSwitch()
                .Bind(SfSwitch.IsOnProperty!, nameof(TirageContext.Auto)).Column(1)
        }
    };

    /// <summary>
    /// Gets a <see cref="Grid"/> that provides options for exporting data.
    /// </summary>
    private static Grid VueOptionExport => new() {
        ColumnDefinitions = Columns.Define(Star, Star),
        Children = {
            new Picker {
                    ItemsSource = ViewTirage.ListeFormats
                }
                .Bind(Picker.SelectedItemProperty, nameof(TirageContext.FmtExport), BindingMode.TwoWay)
                .Column(0),
            new Button {
                    ImageSource = ImageSource.FromFile("excel.png"),
                    ContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Left, 10)
                }
                .Text("Export")
                .BindCommand(nameof(TirageContext.ExportCommand), parameterSource: string.Empty)
                .Column(1)
        }
    };

    /// <summary>
    /// Gets the view that serves as the header for the flyout menu in the application shell.
    /// </summary>
    private static View VueHeader => new Grid {
        HeightRequest = 50,
        Children = {
            new Image {
                Source = ImageSource.FromFile("favicon.ico")
            },
            new Label {
                Text = "Le Compte Est Bon",
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            }
        }
    }.AppThemeColorBinding(BackgroundProperty, BackgroundLight, BackgroundDark);

    /// <summary>
    /// Gets the view that serves as the footer for the flyout menu in the application shell.
    /// </summary>
    private View VueFooter => new Border {
        BackgroundColor = Colors.Transparent,
        Content = new VerticalStackLayout {
            Children = {
                VueOptionTheme,
                VueOptionGrille,
                VueOptionAuto,
                VueOptionExport,
                VueQuitter
            }
        }
    };

    /// <summary>
    /// Gets the view that serves as the Quitter button in the footer for the flyout menu in the application shell.
    /// </summary>
    private static View VueQuitter => new Button {
        Text = "Quitter",
        HeightRequest = 24
    }.BindCommand(nameof(TirageContext.QuitterCommand));

    /// <summary>
    /// Gets the view that serves as the title bar for the application shell.
    /// </summary>
    private static TitleBar VueTitleBar => new TitleBar {
        TrailingContent = new HorizontalStackLayout {
            BindingContext = TirageContext,
            Children = {
                new Label()
                    .Margin(2)
                    .Bind(Label.TextProperty, nameof(TirageContext.ElapsedTime), stringFormat: "{0:hh\\:mm\\:ss\\.fff}")
                    .Bind(IsVisibleProperty, nameof(TirageContext.IsBusy)),
                new Button {
                    ImageSource = ImageSource.FromFile("alea.png")
                }.BindCommand(nameof(TirageContext.RandomCommand)),
                new Button {
                    ImageSource = ImageSource.FromFile("calculer.png")
                }.BindCommand(nameof(TirageContext.SolveCommand))
            }
        }
    }
    .Bind(TitleBar.ForegroundColorProperty, "Foreground")
    .Bind(TitleBar.TitleProperty, nameof(TirageContext.Result), source: TirageContext)
    .Height(24);

    /// <summary>
    /// Initializes the shell of the application.
    /// </summary>
    private void InitializeShell() {
        Title = "Compte est bon";
        FlyoutBackgroundImageAspect = Aspect.AspectFit;
        FlyoutIcon = ImageSource.FromFile("favicon.ico");
        FlyoutBackground.AppThemeColorBinding(FlyoutBackgroundColorProperty, BackgroundLight, BackgroundDark);
        if (DeviceInfo.Platform == DevicePlatform.WinUI) SetNavBarIsVisible(this, false);

        ToolbarItems.Add(new ToolbarItem {
            IconImageSource = ImageSource.FromFile("calculer.png"),
            Text = "Résoudre"
        }.BindCommand(nameof(TirageContext.SolveCommand)));

        ToolbarItems.Add(new ToolbarItem {
            IconImageSource = ImageSource.FromFile("alea.png"),
            Text = "Hasard"
        }.BindCommand(nameof(TirageContext.RandomCommand)));

        Items.Add(new ShellContent {
            Route = "Ceb",
            Title = "Le Compte Est Bon",
            ContentTemplate = new DataTemplate(() => new CebPage()),
            Icon = ImageSource.FromFile("favicon.ico")
        });

        Items.Add(new ShellContent {
            Route = "Config",
            Title = "Configuration",
            ContentTemplate = new DataTemplate(() => new ConfigPage())
        });

        FlyoutFooterTemplate = new DataTemplate(() => VueFooter);
        FlyoutHeaderTemplate = new DataTemplate(() => VueHeader);
    }

    /// <summary>
    /// Called when the shell is appearing on the screen.
    /// </summary>
    protected override void OnAppearing() {
        base.OnAppearing();
        if (DeviceInfo.Platform == DevicePlatform.WinUI) Application.Current!.Windows[0].TitleBar = VueTitleBar;
    }
}
