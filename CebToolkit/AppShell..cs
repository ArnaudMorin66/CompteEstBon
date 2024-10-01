using System.Windows.Input;

using CebToolkit.ViewModel;

using CommunityToolkit.Maui.Markup;

using Syncfusion.Maui.Buttons;

using static CommunityToolkit.Maui.Markup.GridRowsColumns;



namespace CebToolkit;

public  class AppShell : Shell {
    private readonly ViewTirage viewTirage = App.Current.Services.GetService<ViewTirage>()!;

    public AppShell():base() {

        Title = "Compte est bon";
        FlyoutBackgroundColor = Colors.DarkSlateGrey;
        FlyoutBackgroundImageAspect = Aspect.AspectFit;
        
#if WINDOWS
        SetNavBarIsVisible(this,false);
#endif
        Items.Add(new ShellContent() {
            Route = "Ceb",
            Title ="Le Compte Est Bon",
            ContentTemplate = new DataTemplate(()=> new CebPage()),
            Icon = new FileImageSource() {
                File = "favicon.ico"
            } 
        });
        Items.Add(new ShellContent() {
            Route = "Config",
            Title = "Configuration",
            ContentTemplate = new DataTemplate(() => new ConfigPage())
        });
        FlyoutHeaderTemplate = new DataTemplate(()=> new Image(){
            Source = new FileImageSource(){
            File = "ceb.png"
        }});
        
        FlyoutFooterTemplate = new DataTemplate(() => 
            new VerticalStackLayout() {
                BackgroundColor= Colors.DimGrey,
                Children = {
                    VueOptionTheme,
                    VueOptionGrille,
                    VueOptionAuto,
                    VueOptionExport,
                    new Button().Text("Quitter")
                        .Invoke((b)=> b.Clicked+= (_,_)=> Application.Current?.Quit())
                }
        });
        BindingContext = viewTirage;
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
                    ImageSource = new FileImageSource {
                        File = "excel.png"
                    },
                    ContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Left, 10)
                }
                .Text("Export")
                .BindCommand(nameof(viewTirage.ExportCommand), parameterSource: "") //                )
                .Column(1)
        }
    };

#if WINDOWS
    private TitleBar VueTitleBar => new TitleBar() {
        Icon = "favicon.ico",
        TrailingContent = new Label()
                .TextColor(Colors.White).CenterVertical().Bind(Label.TextProperty, "Date", source: viewTirage)
        
        

    }.Bind(TitleBar.ForegroundColorProperty, "Foreground", source: viewTirage)
    .DynamicResource(TitleBar.BackgroundProperty, "LinearGradientBrushBase")
    .Bind(TitleBar.TitleProperty, nameof(viewTirage.Result), source:viewTirage);

    protected override void OnAppearing() {
        base.OnAppearing();
        Application.Current!.Windows[0].TitleBar = VueTitleBar;
    }
#endif

    }