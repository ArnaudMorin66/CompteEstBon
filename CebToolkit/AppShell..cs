using CebToolkit.ViewModel;

using CommunityToolkit.Maui.Markup;

using Syncfusion.Maui.Buttons;

using static CommunityToolkit.Maui.Markup.GridRowsColumns;



namespace CebToolkit;

public  class AppShell : Shell {
    private readonly ViewTirage viewTirage = App.Current.Services.GetService<ViewTirage>()!;

    /// <summary>
    /// Initializes a new instance of the <see cref="CebToolkit.AppShell"/> class.
    /// </summary>
    /// <remarks>
    /// This constructor sets up the main shell of the application, including the title, flyout background,
    /// toolbar items, and shell contents. It also configures platform-specific settings and binds the 
    /// <see cref="CebToolkit.ViewModel.ViewTirage"/> view model to the shell.
    /// </remarks>
    public AppShell() {
        BindingContext = viewTirage;
        
        InitializeShell();
    }
    private void InitializeShell() {
        Title = "Compte est bon";
        FlyoutBackgroundColor = Color.FromArgb("4f4f4f");
        FlyoutBackgroundImageAspect = Aspect.AspectFit;
        FlyoutIcon = ImageSource.FromFile("favicon.ico");

        if (DeviceInfo.Platform == DevicePlatform.WinUI) {
            SetNavBarIsVisible(this, false);
        }

        new Label()
            .TextColor(Colors.White)
            .CenterVertical()
            .Bind(Label.TextProperty, "Date", source: viewTirage);
        
        ToolbarItems.Add(new ToolbarItem {
            IconImageSource = ImageSource.FromFile("resolve.png"),
            Text = "Résoudre"
        }.BindCommand(nameof(viewTirage.ResolveCommand)));

        ToolbarItems.Add(new ToolbarItem {
            IconImageSource = ImageSource.FromFile("random.png"),
            Text = "Hasard"
        }.BindCommand(nameof(viewTirage.RandomCommand)));

        ToolbarItems.Add(new ToolbarItem().Bind(MenuItem.TextProperty, "Date"));

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

    private Grid VueOptionGrille => new() {
        ColumnDefinitions =
            Columns.Define(Star, Star),
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

    /// <summary>
    /// Gets a <see cref="Grid"/> that provides options for automatic functionality.
    /// </summary>
    /// <remarks>
    /// This grid contains a <see cref="Label"/> for displaying the "Auto" text, a <see cref="SfSwitch"/> for toggling the automatic functionality,
    /// and sets the column definitions and text color.
    /// </remarks>
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
                .Bind(SfSwitch.IsOnProperty!, nameof(viewTirage.Auto)).Column(1)
        }
    };
    /// <summary>
    /// Gets a <see cref="Grid"/> that provides options for exporting data.
    /// </summary>
    /// <remarks>
    /// This grid contains a <see cref="Picker"/> for selecting export formats and a <see cref="Button"/> for initiating the export process.
    /// </remarks>
    private Grid VueOptionExport => new() {
        ColumnDefinitions = Columns.Define(Star, Star),
        Children = {
            new Picker {
                    ItemsSource = ViewTirage.ListeFormats
                }
                .TextColor(Colors.White)
                .Bind(Picker.SelectedItemProperty, nameof(viewTirage.FmtExport), BindingMode.TwoWay)
                .Column(0),

            new Button {
                    ImageSource = ImageSource.FromFile("excel.png"),
                    ContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Left, 10)
                }
                .Text("Export")
                .BindCommand(nameof(viewTirage.ExportCommand), parameterSource: "") //                )
                .Column(1)
        }
    };

    /// <summary>
    /// Gets the view that serves as the header for the flyout menu in the application shell.
    /// </summary>
    /// <remarks>
    /// The header view includes an image and a label with the text "Le Compte Est Bon".
    /// </remarks>
    private View VueHeader => new Grid() {
        BackgroundColor = Color.FromArgb("2f4f4f"),
        HeightRequest = 50,
        Children = {
            new Image() {
                Opacity = 0.6,
                Source = ImageSource.FromFile("favicon.ico")
            },
            new Label() {
                Text = "Le Compte Est Bon",
                TextColor = Colors.Black,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
            }
        },
    };

    /// <summary>
    /// Gets the view that serves as the footer for the flyout menu in the application shell.
    /// </summary>
    /// <remarks>
    /// The footer view includes options for theme, grid, auto, export, and a "Quitter" button.
    /// </remarks>
    private View VueFooter => new VerticalStackLayout() {
        BackgroundColor = Color.FromArgb("2f4f4f"),
        Children =
        {
            VueOptionTheme,
            VueOptionGrille,
            VueOptionAuto,
            VueOptionExport,
            new Button()
            {
                Text = "Quitter",
                Command = new Command(() => Application.Current?.Quit())
            }
        }
    };

    /// <summary>
    /// Gets the view that serves as the title bar for the application shell.
    /// </summary>
    /// <remarks>
    /// The title bar view includes a trailing label that displays the current date.
    /// </remarks>
    private TitleBar VueTitleBar => new TitleBar() {
            TrailingContent = new HorizontalStackLayout() {
                Children = {
                    new Label()
                        .TextColor(Colors.White)
                        .CenterVertical()
                        .Bind(Label.TextProperty, "Date", source: viewTirage),
                    new Button() {
                        ImageSource = ImageSource.FromFile("resolve.png")
                    }.BindCommand(nameof(viewTirage.ResolveCommand),source:this.viewTirage),
                    new Button() {
                        ImageSource = ImageSource.FromFile("random.png")
                    }.BindCommand(nameof(viewTirage.RandomCommand),source:viewTirage),


                }
            } 
        }
        .Bind(TitleBar.ForegroundColorProperty, "Foreground", source: viewTirage)
        .DynamicResource(TitleBar.BackgroundProperty, "LinearGradientBrushBase")
        .Bind(TitleBar.TitleProperty, nameof(viewTirage.Result), source: viewTirage);
    
    /// <summary>
    /// Called when the shell is appearing on the screen.
    /// </summary>
    protected override void OnAppearing() {
        base.OnAppearing();
        if (DeviceInfo.Platform == DevicePlatform.WinUI)
            Application.Current!.Windows[0].TitleBar = VueTitleBar;
    }


}