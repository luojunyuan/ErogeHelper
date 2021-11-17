using System;
using System.Drawing;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Model.Services;
using ErogeHelper.Model.Services.Interface;
using ErogeHelper.Share;
using ErogeHelper.Share.Contracts;
using ErogeHelper.Share.Entities;
using ErogeHelper.Share.Languages;
using ErogeHelper.Share.Structs;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Vanara.PInvoke;

namespace ErogeHelper.ViewModel.Controllers
{
    public class AssistiveTouchViewModel : ReactiveObject
    { 
        private readonly IEhConfigRepository _ehConfigRepository;

        public AssistiveTouchViewModel(
            IWindowDataService? windowDataService = null,
            IEhConfigRepository? ehConfigDataService = null,
            IGameDataService? gameDataService = null,
            IGameInfoRepository? ehDbRepository = null,
            ITouchConversionHooker? touchConversionHooker = null,
            IGameWindowHooker? gameWindowHooker = null,
            TouchToolBoxViewModel? touchToolBoxViewModel = null)
        {
            windowDataService ??= DependencyResolver.GetService<IWindowDataService>();
            _ehConfigRepository = ehConfigDataService ?? DependencyResolver.GetService<IEhConfigRepository>();
            gameDataService ??= DependencyResolver.GetService<IGameDataService>();
            ehDbRepository ??= DependencyResolver.GetService<IGameInfoRepository>();
            touchConversionHooker ??= DependencyResolver.GetService<ITouchConversionHooker>();
            gameWindowHooker ??= DependencyResolver.GetService<IGameWindowHooker>();
            touchToolBoxViewModel ??= DependencyResolver.GetService<TouchToolBoxViewModel>();
            
#if !DEBUG // https://stackoverflow.com/questions/63723996/mouse-freezing-lagging-when-hit-breakpoint
            touchConversionHooker.Init();
#endif
            SwitchFullScreenIcon = SymbolName.FullScreen;
            SwitchFullScreenToolTip = Strings.GameView_SwitchFullScreen;
            LoseFocusEnable = ehDbRepository.GameInfo.IsLoseFocus;
            TouchBoxEnable = _ehConfigRepository.UseTouchToolBox;
            IsTouchToMouse = ehDbRepository.GameInfo.IsEnableTouchToMouse;

            BigSize = _ehConfigRepository.UseBigAssistiveTouchSize;

            _ehConfigRepository.WhenAnyValue(x => x.UseBigAssistiveTouchSize)
                .Skip(1)
                .Subscribe(useBigSize => _useBigSizeSubj.OnNext(useBigSize));

            // Flyout bar commands implements
            gameDataService
                .GameFullscreenChanged
                .Subscribe(isFullscreen =>
                {
                    if (isFullscreen)
                    {
                        SwitchFullScreenIcon = SymbolName.BackToWindow;
                        SwitchFullScreenToolTip = Strings.GameView_SwitchWindow;
                    }
                    else
                    {
                        SwitchFullScreenIcon = SymbolName.FullScreen;
                        SwitchFullScreenToolTip = Strings.GameView_SwitchFullScreen;
                    }
                });

            this.WhenAnyValue(x => x.LoseFocusEnable)
                .Skip(1)
                .Subscribe(v =>
                {
                    HwndTools.WindowLostFocus(windowDataService.MainWindowHandle, v);
                    ehDbRepository.UpdateLostFocusStatus(v);
                });

            this.WhenAnyValue(x => x.LoseFocusEnable)
                .ToPropertyEx(this, x => x.TouchBoxSwitcherVisible);
                
            this.WhenAnyValue(x => x.TouchBoxEnable)
                .Skip(1)
                .DistinctUntilChanged()
                .Subscribe(v => _ehConfigRepository.UseTouchToolBox = v);
            this.WhenAnyValue(x => x.LoseFocusEnable, x => x.TouchBoxEnable, (a, b) => a && b)
                .Skip(1)
                .Subscribe(v => touchToolBoxViewModel.TouchToolBoxVisible = v);

            this.WhenAnyValue(x => x.IsTouchToMouse)
                .Skip(1)
                .Subscribe(v =>
                {
                    touchConversionHooker.Enable = v;
                    ehDbRepository.UpdateTouchEnable(v);
                });

            SwitchFullScreen = ReactiveCommand.Create(() =>
                User32.BringWindowToTop(gameDataService.MainProcess.MainWindowHandle));
        }

        public bool BigSize;

        private readonly Subject<bool> _useBigSizeSubj = new();
        public IObservable<bool> UseBigSize => _useBigSizeSubj;

        public AssistiveTouchPosition AssistiveTouchPosition
        {
            get => JsonSerializer.Deserialize<AssistiveTouchPosition>(_ehConfigRepository.AssistiveTouchPosition)
                ?? EhContext.TouchPosition;
            set => _ehConfigRepository.AssistiveTouchPosition = JsonSerializer.Serialize(value);
        }

        [Reactive]
        public bool LoseFocusEnable { get; set; }

        [ObservableAsProperty]
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public bool TouchBoxSwitcherVisible { get; }

        [Reactive]
        public bool TouchBoxEnable { get; set; }

        [Reactive]
        public bool IsTouchToMouse { get; set; }

        [Reactive]
        public SymbolName SwitchFullScreenIcon { get; set; }

        [Reactive]
        public string SwitchFullScreenToolTip { get; set; }

        public ReactiveCommand<Unit, bool> SwitchFullScreen { get; }
    }
}
