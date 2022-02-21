using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using Config.Net;
using ErogeHelper.Model.DataServices;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.Repositories;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Model.Repositories.Migration;
using ErogeHelper.Model.Services;
using ErogeHelper.Model.Services.Function;
using ErogeHelper.Model.Services.Interface;
using ErogeHelper.Platform.MISC;
using ErogeHelper.Platform.RxUI;
using ErogeHelper.Platform.WinRTHelper;
using ErogeHelper.Shared;
using ErogeHelper.Shared.Contracts;
using ErogeHelper.Shared.Languages;
using ErogeHelper.View.CloudSave;
using ErogeHelper.View.HookConfig;
using ErogeHelper.View.MainGame;
using ErogeHelper.View.Preference;
using ErogeHelper.View.TextDisplay;
using ErogeHelper.ViewModel;
using ErogeHelper.ViewModel.CloudSave;
using ErogeHelper.ViewModel.HookConfig;
using ErogeHelper.ViewModel.MainGame;
using ErogeHelper.ViewModel.MainGame.AssistiveTouchMenu;
using ErogeHelper.ViewModel.Preference;
using ErogeHelper.ViewModel.TextDisplay;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using ModernWpf;
using ModernWpf.Controls;
using Ookii.Dialogs.Wpf;
using ReactiveUI;
using Refit;
using Splat;
using UpdateChecker;
using Vanara.PInvoke;
using Vanara.PInvoke.NetListMgr;
using WK.Libraries.SharpClipboardNS;

namespace ErogeHelper.Platform;

internal static class DI
{
    /// <summary>
    /// The Composite Root
    /// </summary>
    public static void RegisterServices()
    {
#if !DEBUG
        Locator.CurrentMutable.RegisterConstant<ILogger>(new NullLogger());
#endif

        var ehConfigRepository =
            new ConfigurationBuilder<IEHConfigRepository>().UseJsonFile(EHContext.ConfigFilePath).Build();
        // The view Locator
        Locator.CurrentMutable.RegisterConstant<IViewLocator>(new ItemViewLocator(ehConfigRepository));

        RegisterViews();
        RegisterViewModelsForViews();
        RegisterInteractions();

        RegisterDataServices(ehConfigRepository);
        var (gameWindowHookerService, sharpClipboard) =
            RegisterModelServices(ehConfigRepository);

        // https://stackoverflow.com/questions/30352447/using-reactiveuis-bindto-to-update-a-xaml-property-generates-a-warning/#31464255
        Locator.CurrentMutable.RegisterLazySingleton<ICreatesObservableForProperty>(() => new CustomPropertyResolver());


        // ViewModel->View callback 
        HookViewModel.EnableClipboardCallback = isUseClipboard => sharpClipboard.MonitorClipboard = isUseClipboard;
        HwndTools.IsGameFullscreenCallback = WpfHelper.IsGameForegroundFullscreen;

        // Model->ViewModel data flow
        gameWindowHookerService.BringKeyboardWindowTopDataFlow
            .Subscribe(_ => User32.BringWindowToTop(State.VirtualKeyboardWindowHandle));
    }

    /// <summary>
    /// Register data service such as repositories
    /// </summary>
    private static void RegisterDataServices(IEHConfigRepository ehConfigRepository)
    {
        Locator.CurrentMutable.RegisterConstant(ehConfigRepository);
        Locator.CurrentMutable.RegisterLazySingleton<IGameInfoRepository>(
            () => new GameInfoRepository(EHContext.DbConnectString));
        Locator.CurrentMutable.RegisterLazySingleton<ICommentRepository>(
            () => new CommentRepository(EHContext.DbConnectString));
        Locator.CurrentMutable.Register(() => RestService.For<IHookCodeService>(ConstantValue.AniclanBaseUrl,
            new RefitSettings { ContentSerializer = new XmlContentSerializer() }));
        Locator.CurrentMutable.Register(
            () => RestService.For<IEHServerApiRepository>(ehConfigRepository.ServerBaseUrl));

        // In memory state
        Locator.CurrentMutable.RegisterLazySingleton<IGameDataService>(() => new GameDataService());
        Locator.CurrentMutable.RegisterLazySingleton<IWindowDataService>(() => new WindowDataService());
    }

    /// <summary>
    /// Register model layer, various tools and functions
    /// </summary>
    private static (GameWindowHooker, SharpClipboard) RegisterModelServices(IEHConfigRepository ehConfigRepository)
    {
        if (Utils.HasWinRT)
        {
            Locator.CurrentMutable.RegisterLazySingleton<IToastManagement>(() => new ToastManagementWinRT());
            Locator.CurrentMutable.RegisterLazySingleton<ITTSService>(() => new TTSWinRT());
        }
        else
        {
            Locator.CurrentMutable.RegisterLazySingleton<IToastManagement>(() => new ToastManagement());
            Locator.CurrentMutable.RegisterLazySingleton<ITTSService>(() => new TTSService());
        }
        if (Utils.HasWinRT && !Directory.Exists(EHContext.MeCabDicFolder))
        {
            Locator.CurrentMutable.RegisterLazySingleton<IMeCabService>(() => new MeCabWinRTService());
            MeCabWinRTService.JapaneseAnalyzerCallback = JapaneseAnalyzerWinRT.JapaneseAnalyzer;
        }
        else
        {
            Locator.CurrentMutable.RegisterLazySingleton<IMeCabService>(() => new MeCabService());
        }

        Locator.CurrentMutable.Register<IUpdateService>(() => new UpdateService());
        Locator.CurrentMutable.Register(() => new ScenarioContext());
        var gameWindowHookerService = new GameWindowHooker();
        Locator.CurrentMutable.RegisterConstant<IGameWindowHooker>(gameWindowHookerService);
        Locator.CurrentMutable.RegisterLazySingleton(() => new NetworkListManager(), typeof(INetworkListManager));
        var sharpClipboard = new SharpClipboard();
        Locator.CurrentMutable.RegisterConstant(sharpClipboard);
#if !DEBUG // https://stackoverflow.com/questions/63723996/mouse-freezing-lagging-when-hit-breakpoint
        Locator.CurrentMutable.RegisterLazySingleton<ITouchConversionHooker>(() => new TouchConversionHooker());
#else
        Locator.CurrentMutable.RegisterLazySingleton<ITouchConversionHooker>(() => new TouchConversionHookerFake());
#endif
        // TODO: Can change it to compile macro
        // FIXME: Texehost.dll x86 can not use currently!
        if (RuntimeInformation.ProcessArchitecture != Architecture.X64) // Arm64
        {
            Locator.CurrentMutable.RegisterLazySingleton<ITextractorService>(() => new TextractorCli());
        }
        else
        {
            Locator.CurrentMutable.RegisterLazySingleton<ITextractorService>(() => new TextractorHost());
        }

        return (gameWindowHookerService, sharpClipboard);
    }

    /// <summary>
    /// Register viewmodel layer
    /// </summary>
    private static void RegisterViewModelsForViews()
    {
        Locator.CurrentMutable.RegisterLazySingleton(() => new MainGameViewModel());
        Locator.CurrentMutable.RegisterLazySingleton(() => new AssistiveTouchViewModel());
        Locator.CurrentMutable.RegisterLazySingleton(() => new MenuGameViewModel());
        Locator.CurrentMutable.Register(() => new DanmakuCanvasViewModel());

        Locator.CurrentMutable.Register(() => new TextViewModel());

        Locator.CurrentMutable.Register(() => new HookViewModel());
        Locator.CurrentMutable.Register(() => new HCodeViewModel());
        Locator.CurrentMutable.Register(() => new RCodeViewModel());
        Locator.CurrentMutable.Register(() => new TextRegExpViewModel());
        Locator.CurrentMutable.Register(() => new ReiPatcherTipViewModel());

        Locator.CurrentMutable.Register(() => new CloudSaveViewModel());

        Locator.CurrentMutable.Register(() => new PreferenceViewModel());
        Locator.CurrentMutable.Register(() => new GeneralViewModel());
        Locator.CurrentMutable.Register(() => new MeCabViewModel());
        Locator.CurrentMutable.Register(() => new TTSViewModel());
        Locator.CurrentMutable.Register(() => new DanmakuViewModel());
        Locator.CurrentMutable.Register(() => new TransViewModel());
        Locator.CurrentMutable.Register(() => new AboutViewModel());
    }

    /// <summary>
    /// Register view layer for <see cref="ItemViewLocator"/>
    /// </summary>
    private static void RegisterViews()
    {
        Locator.CurrentMutable.RegisterLazySingleton<IViewFor<MainGameViewModel>>(() => new MainGameWindow());
        Locator.CurrentMutable.Register<IViewFor<DanmakuCanvasViewModel>>(() => new DanmakuCanvas());

        Locator.CurrentMutable.Register<IViewFor<TextViewModel>>(() => new TextWindow());
        // Resolve FuriganaItemViewModel in ItemViewLocator

        Locator.CurrentMutable.Register<IViewFor<HookViewModel>>(() => new HookWindow());
        Locator.CurrentMutable.Register<IViewFor<HookThreadItemViewModel>>(() => new HookThreadItem());
        Locator.CurrentMutable.Register<IViewFor<HCodeViewModel>>(() => new HCodeDialog());
        Locator.CurrentMutable.Register<IViewFor<RCodeViewModel>>(() => new RCodeDialog());
        Locator.CurrentMutable.Register<IViewFor<TextRegExpViewModel>>(() => new TextRegExpDialog());
        Locator.CurrentMutable.Register<IViewFor<ReiPatcherTipViewModel>>(() => new ReiPatcherTipDialog());

        Locator.CurrentMutable.Register<IViewFor<CloudSaveViewModel>>(() => new CloudSaveWindow());

        Locator.CurrentMutable.Register<IViewFor<PreferenceViewModel>>(() => new PreferenceWindow());
        Locator.CurrentMutable.Register<IViewFor<GeneralViewModel>>(() => new GeneralPage());
        Locator.CurrentMutable.Register<IViewFor<MeCabViewModel>>(() => new MeCabPage());
        Locator.CurrentMutable.Register<IViewFor<DanmakuViewModel>>(() => new DanmakuPage());
        Locator.CurrentMutable.Register<IViewFor<TTSViewModel>>(() => new TTSPage());
        Locator.CurrentMutable.Register<IViewFor<TransViewModel>>(() => new TransPage());
        Locator.CurrentMutable.Register<IViewFor<AboutViewModel>>(() => new AboutPage());
    }

    /// <summary>
    /// Interactions between view and viewmodel
    /// </summary>
    private static void RegisterInteractions()
    {
        Interactions.MessageBoxConfirm
            .RegisterHandler(context =>
            {
                var result = ShowMessageBox(context.Input);
                var yesOrNo = result switch
                {
                    MessageBoxResult.OK => true,
                    MessageBoxResult.Yes => true,
                    MessageBoxResult.No => false,
                    MessageBoxResult.Cancel => false,
                    MessageBoxResult.None => false,
                    _ => false,
                };
                context.SetOutput(yesOrNo);
            });

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

        Interactions.FolderBrowserDialog
            .RegisterHandler(context =>
            {
                var dialog = new VistaFolderBrowserDialog()
                {
                    Description = context.Input.Description,
                    UseDescriptionForTitle = true,
                    SelectedPath = context.Input.RootFolder + "\\",
                };

                var output = dialog.ShowDialog() switch
                {
                    true => dialog.SelectedPath,
                    _ => string.Empty,
                };
                context.SetOutput(output);
            });
    }

    public static MessageBoxResult? ShowMessageBox(string messageBoxText, string caption = "Eroge Helper")
    {
        var owner = Application.Current.Windows.Cast<Window>().FirstOrDefault((Window window) =>
            window.IsActive && window.ShowActivated);

        var messageBoxWindow = new MessageBoxWindow(messageBoxText, caption, MessageBoxButton.OK, null)
        {
            Owner = owner,
            WindowStartupLocation = (owner == null)
                ? WindowStartupLocation.CenterScreen : WindowStartupLocation.CenterOwner,
            Topmost = true
        };
        messageBoxWindow.ShowDialog();
        return messageBoxWindow.Result;
    }

    public static void ShowView<T>() where T : ReactiveObject
    {
        var viewName = typeof(T).ToString()[..^9] // erase ViewModel suffix
            .Replace("Model", string.Empty) + "Window";
        var windowType = Type.GetType(viewName) ?? throw new InvalidCastException(viewName);

        Window? targetWindow = null;
        Application.Current.Windows
            .Cast<Window>().ToList()
            .ForEach(w =>
            {
                if (w.GetType() == windowType)
                    targetWindow = w;
            });

        if (targetWindow is not null)
        {
            targetWindow.Activate();
            if (targetWindow.WindowState == WindowState.Minimized)
                targetWindow.WindowState = WindowState.Normal;
        }
        else
        {
            var view = DependencyResolver.GetService<IViewFor<T>>();
            if (view is not Window window)
                throw new TypeAccessException("View not implement IViewFor");
            window.Show();
            if (window is not MainGameWindow)
                window.Activate();
        }
    }

    public static void UpdateDatabase()
    {
        Directory.CreateDirectory(EHContext.RoamingEHFolder);

        var microsoftServiceProvider = new ServiceCollection()
            .AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddSQLite()
                .WithGlobalConnectionString(EHContext.DbConnectString)
                .ScanIn(typeof(AddGameInfoTable).Assembly)
                .ScanIn(typeof(AddUserTermTable).Assembly)
                .ScanIn(typeof(AddSaveDataCloudColumn).Assembly)
                .ScanIn(typeof(AddUseClipboardColumn).Assembly)
                .For.Migrations())
            .BuildServiceProvider(false);

        using var scope = microsoftServiceProvider.CreateScope();
        var runner = microsoftServiceProvider.GetRequiredService<IMigrationRunner>();
        // Note: May be the reason of upgrading to PointerMessage automatically
        runner.MigrateUp();
    }

    public static async void WarmingUp()
    {
        try
        {
            await new GitHubReleasesUpdateChecker("erogehelper", "erogehelper", false, "9.9.9.9").CheckAsync(default);
        }
        catch
        {
            // ignored
        }

        BrightnessAdjust.WarmUp();
    }
}
