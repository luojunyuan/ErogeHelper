using System.IO;
using System.Text;
using Config.Net;
using ErogeHelper.Common.Languages;
using ErogeHelper.Function.Platform;
using ErogeHelper.Model.Repositories;
using ErogeHelper.Model.Services;
using ErogeHelper.ViewModel;
using ErogeHelper.ViewModel.HookConfig;
using ErogeHelper.ViewModel.Preference;
using ModernWpf.Controls;
using ReactiveUI;
using Refit;
using Splat;
using WK.Libraries.SharpClipboardNS;

namespace ErogeHelper.Function.Startup;

internal static class DI
{
    /// <summary>
    /// The Composite Root
    /// </summary>
    public static void RegisterServices()
    {
        RegisterLogger();
        LogHost.Default.Info($"Working directory: {AppContext.BaseDirectory}");

        RegisterViews();
        RegisterViewModels();

        RegisterDataServices();
        RegisterModelServices();

        RegisterInteractions();
        //RegisterCallbacks();

        // For shift-jis decode/encode
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Locator.CurrentMutable.Register(() => new CustomPropertyResolver(), typeof(ICreatesObservableForProperty));
    }

    private static void RegisterLogger()
    {
#if !DEBUG
        Locator.CurrentMutable.RegisterConstant<ILogger>(new NullLogger());
#else
        try
        {
            _ = Console.WindowWidth;
            Locator.CurrentMutable.RegisterConstant<ILogger>(new ConsoleLogger());
        }
        catch (IOException)
        {
            // Skip exception. No console window, cause OutputType target at WinExe
        }

        // Log in VisualStudio Output by default
#endif
    }

    /// <summary>
    /// Register view layer for <see cref="ItemViewLocator"/>
    /// </summary>
    private static void RegisterViews()
    {
        // ModernWpf 
        Locator.CurrentMutable.Register<IViewFor<GeneralViewModel>>(() => new View.Modern.Preference.GeneralPage());
        Locator.CurrentMutable.Register<IViewFor<AboutViewModel>>(() => new View.Modern.Preference.AboutPage());

        Locator.CurrentMutable.Register<IViewFor<HookThreadItemViewModel>>(() => new View.Modern.HookConfig.HookThreadItem());
        Locator.CurrentMutable.Register<IViewFor<TextRegExpViewModel>>(() => new View.Modern.HookConfig.TextRegExpDialog());
        Locator.CurrentMutable.Register<IViewFor<HCodeViewModel>>(() => new View.Modern.HookConfig.HCodeDialog());
    }

    /// <summary>
    /// Register viewmodel layer for views
    /// </summary>
    private static void RegisterViewModels()
    {
        Locator.CurrentMutable.Register(() => new PreferenceViewModel());
        Locator.CurrentMutable.Register(() => new GeneralViewModel());
        Locator.CurrentMutable.Register(() => new AboutViewModel());

        Locator.CurrentMutable.Register(() => new HookViewModel()); 
        Locator.CurrentMutable.Register(() => new TextRegExpViewModel());
        Locator.CurrentMutable.Register(() => new HCodeViewModel());
        Locator.CurrentMutable.Register(() => new RCodeViewModel());
    }

    /// <summary>
    /// Register data services
    /// </summary>
    private static void RegisterDataServices()
    {
        Locator.CurrentMutable.RegisterConstant(new ConfigurationBuilder<IEHConfigRepository>()
            .UseJsonFile(EHContext.ConfigFilePath)
            .Build());
        Locator.CurrentMutable.RegisterConstant(new ConfigurationBuilder<IGameInfoRepository>()
            .UseJsonFile(Path.Combine(EHContext.RoamingFolder, State.Md5 + ".json"))
            .Build());
    }

    /// <summary>
    /// Register model layer, various tools and functions
    /// </summary>
    private static void RegisterModelServices()
    {
        Locator.CurrentMutable.RegisterConstant<IGameWindowHooker>(new GameWindowHooker());
        Locator.CurrentMutable.RegisterLazySingleton<ITextractorService>(() => new TextractorCli());

        const string AniclanBaseUrl = "http://vnr.aniclan.com";
        Locator.CurrentMutable.Register(() => RestService.For<IHookCodeService>(AniclanBaseUrl,
            new RefitSettings { ContentSerializer = new XmlContentSerializer() }));

        Locator.CurrentMutable.RegisterConstant(new SharpClipboard());
    }

    /// <summary>
    /// Interactions between view and viewmodel
    /// </summary>
    private static void RegisterInteractions()
    {
        // ModernWpf
        Interactions.ContentDialog
            .RegisterHandler(async context =>
            {
                var result = await new ContentDialog()
                {
                    Title = context.Input,
                    CloseButtonText = Strings.Common_OK
                }.ShowAsync().ConfigureAwait(false);
                var yesOrNo = result switch
                {
                    ContentDialogResult.None => false,
                    ContentDialogResult.Primary => true,
                    ContentDialogResult.Secondary => true,
                    _ => false,
                };
                context.SetOutput(yesOrNo);
            });

        //Interactions.FolderBrowserDialog
        //    .RegisterHandler(context =>
        //    {
        //        var dialog = new VistaFolderBrowserDialog()
        //        {
        //            Description = context.Input.Description,
        //            UseDescriptionForTitle = true,
        //            SelectedPath = context.Input.RootFolder + "\\",
        //        };

        //        var output = dialog.ShowDialog() switch
        //        {
        //            true => dialog.SelectedPath,
        //            _ => string.Empty,
        //        };
        //        context.SetOutput(output);
        //    });
    }

    private static void RegisterCallbacks()
    {
        //EHInstance.Shutdown = WpfHelper.Shutdown;
        //EHInstance.InitEHVersion(ApplicationHelper.EHVersion);
    }
}
