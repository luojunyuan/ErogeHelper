using System.IO;
using System.Reactive;
using System.Windows;
using Config.Net;
using ErogeHelper.Model.DataServices;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.Repositories;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Model.Repositories.Migration;
using ErogeHelper.Model.Services;
using ErogeHelper.Model.Services.Interface;
using ErogeHelper.Platform;
using ErogeHelper.Shared;
using ErogeHelper.Shared.Contracts;
using ErogeHelper.Shared.Languages;
using ErogeHelper.View.Controllers;
using ErogeHelper.View.Dialogs;
using ErogeHelper.View.Items;
using ErogeHelper.View.Pages;
using ErogeHelper.View.Windows;
using ErogeHelper.ViewModel;
using ErogeHelper.ViewModel.Controllers;
using ErogeHelper.ViewModel.Dialogs;
using ErogeHelper.ViewModel.Items;
using ErogeHelper.ViewModel.Pages;
using ErogeHelper.ViewModel.Windows;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using ModernWpf.Controls;
using ReactiveUI;
using Refit;
using Splat;
using Vanara.PInvoke;
using Vanara.PInvoke.NetListMgr;
using MessageBox = ModernWpf.MessageBox;

namespace ErogeHelper.Functions;

internal static class DI
{
    public static void RegisterServices()
    {
        Locator.CurrentMutable.RegisterConstant<IViewLocator>(new ItemViewLocator());

        RegisterViews();
        RegisterViewModelsForViews();

        // DataService
        Locator.CurrentMutable.RegisterLazySingleton(
            () => new ConfigurationBuilder<IEHConfigRepository>().UseJsonFile(EHContext.ConfigFilePath).Build());
        Locator.CurrentMutable.RegisterLazySingleton<IGameInfoRepository>(
            () => new GameInfoRepository(EHContext.DbConnectString));
        Locator.CurrentMutable.RegisterLazySingleton<IGameDataService>(() => new GameDataService());
        Locator.CurrentMutable.RegisterLazySingleton<IWindowDataService>(() => new WindowDataService());
        Locator.CurrentMutable.Register(() => RestService.For<IHookCodeService>(ConstantValue.AniclanBaseUrl,
            new RefitSettings { ContentSerializer = new XmlContentSerializer() }));

        // Service
        Locator.CurrentMutable.Register<IUpdateService>(() => new UpdateService());
        Locator.CurrentMutable.RegisterLazySingleton<IGameWindowHooker>(() => new GameWindowHooker());
        Locator.CurrentMutable.RegisterLazySingleton<ITouchConversionHooker>(() => new TouchConversionHooker());
        if (Utils.HasWinRT)
        {
            Locator.CurrentMutable.RegisterLazySingleton<IMeCabService>(() => new MeCabWinRTService());
            MeCabWinRTService.MeCabWordWinRTCallback = WinRTHelper.JapaneseAnalyzer;
        }
        else
        {
            Locator.CurrentMutable.RegisterLazySingleton<IMeCabService>(() => new MeCabService());
        }
        Locator.CurrentMutable.Register(() => new NetworkListManager(), typeof(INetworkListManager));
        //if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
        if (true)
            Locator.CurrentMutable.RegisterLazySingleton<ITextractorService>(() => new TextractorCli());
        //else
        //{
        //    Locator.CurrentMutable.RegisterLazySingleton<ITextractorService>(() => new TextractorHost());
        //}

        // MISC
        // https://stackoverflow.com/questions/30352447/using-reactiveuis-bindto-to-update-a-xaml-property-generates-a-warning/#31464255
        Locator.CurrentMutable.RegisterLazySingleton<ICreatesObservableForProperty>(() => new CustomPropertyResolver());
    }

    /// <summary>
    /// Register viewmodel layer of windows, controllers and pages
    /// </summary>
    private static void RegisterViewModelsForViews()
    {
        Locator.CurrentMutable.RegisterLazySingleton(() => new MainGameViewModel());
        Locator.CurrentMutable.RegisterLazySingleton(() => new AssistiveTouchViewModel());
        Locator.CurrentMutable.RegisterLazySingleton(() => new TouchToolBoxViewModel());

        Locator.CurrentMutable.RegisterLazySingleton(() => new TextViewModel());

        Locator.CurrentMutable.Register(() => new HookViewModel());
        Locator.CurrentMutable.Register(() => new HCodeViewModel());

        Locator.CurrentMutable.Register(() => new CloudSaveViewModel());

        Locator.CurrentMutable.Register(() => new PreferenceViewModel());
        Locator.CurrentMutable.Register(() => new GeneralViewModel());
        Locator.CurrentMutable.Register(() => new MeCabViewModel());
        Locator.CurrentMutable.Register(() => new AboutViewModel());
    }

    /// <summary>
    /// Register view layer of windows, controllers, pages and items for <see cref="ItemViewLocator"/>
    /// </summary>
    private static void RegisterViews()
    {
        Locator.CurrentMutable.RegisterLazySingleton<IViewFor<MainGameViewModel>>(() => new MainGameWindow());
        Locator.CurrentMutable.RegisterLazySingleton<IViewFor<AssistiveTouchViewModel>>(() => new AssistiveTouch());
        Locator.CurrentMutable.RegisterLazySingleton<IViewFor<TouchToolBoxViewModel>>(() => new TouchToolBox());

        Locator.CurrentMutable.RegisterLazySingleton<IViewFor<TextViewModel>>(() => new TextWindow());

        Locator.CurrentMutable.Register<IViewFor<HookViewModel>>(() => new HookWindow());
        Locator.CurrentMutable.Register<IViewFor<HookThreadItemViewModel>>(() => new HookThreadItem());
        Locator.CurrentMutable.Register<IViewFor<HCodeViewModel>>(() => new HCodeDialog());

        Locator.CurrentMutable.Register<IViewFor<CloudSaveViewModel>>(() => new CloudSaveWindow());

        Locator.CurrentMutable.Register<IViewFor<PreferenceViewModel>>(() => new PreferenceWindow());
        Locator.CurrentMutable.Register<IViewFor<GeneralViewModel>>(() => new GeneralPage());
        Locator.CurrentMutable.Register<IViewFor<MeCabViewModel>>(() => new MeCabPage());
        Locator.CurrentMutable.Register<IViewFor<AboutViewModel>>(() => new AboutPage());
    }

    public static void RegisterInteractions()
    {
        Interactions.CheckGameFullscreen
            .RegisterHandler(context =>
                context.SetOutput(WpfHelper.IsGameForegroundFullscreen(context.Input)));

        Interactions.TerminateApp
            .RegisterHandler(context =>
            {
                App.Terminate();
                context.SetOutput(Unit.Default);
            });

        Interactions.MessageBoxConfirm
            .RegisterHandler(context =>
            {
                var result = MessageBox.Show(context.Input, "Eroge Helper");
                var yesOrNo = result switch
                {
                    MessageBoxResult.OK => true,
                    MessageBoxResult.Yes => true,
                    MessageBoxResult.No => false,
                    MessageBoxResult.Cancel => false,
                    MessageBoxResult.None => false,
                    null => false,
                    _ => throw new InvalidOperationException(),
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
                    _ => throw new InvalidOperationException(),
                };
                context.SetOutput(yesOrNo);
            });
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
            {
                window.Activate();
            }
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
                .ScanIn(typeof(_001AddGameInfoTable).Assembly)
                .ScanIn(typeof(_002AddUserTermTable).Assembly)
                .ScanIn(typeof(_003AddSaveDataCloudColumn).Assembly)
                .For.Migrations())
            .BuildServiceProvider(false);

        using var scope = microsoftServiceProvider.CreateScope();
        var runner = microsoftServiceProvider.GetRequiredService<IMigrationRunner>();
        // Note: May be the reason of ScrollViewer bug
        runner.MigrateUp();
    }
}
