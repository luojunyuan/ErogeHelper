using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using ErogeHelper.Common.Entities;
using ErogeHelper.Function;
using ErogeHelper.Function.NativeHelper;
using ErogeHelper.Model.Repositories;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;

namespace ErogeHelper.ViewModel.MainGame;

public class AssistiveTouchViewModel : ReactiveObject
{

    public AssistiveTouchViewModel(IEHConfigRepository? ehConfigRepository = null, IGameInfoRepository? gameInfoRepository = null)
    {
        ehConfigRepository ??= DependencyResolver.GetService<IEHConfigRepository>();
        gameInfoRepository ??= DependencyResolver.GetService<IGameInfoRepository>();

        AssistiveTouchPosition =
            JsonSerializer.Deserialize<AssistiveTouchPosition>(ehConfigRepository.AssistiveTouchPosition);
        this.WhenAnyValue(x => x.AssistiveTouchPosition)
            .Skip(1)
            .Throttle(EHContext.UserConfigOperationDelay)
            .Subscribe(pos => ehConfigRepository.AssistiveTouchPosition = JsonSerializer.Serialize(pos));

        BigTouchChanged = new(ehConfigRepository.AssistiveTouchBig);

        ehConfigRepository
            .WhenAnyValue(x => x.AssistiveTouchBig)
            .Skip(1)
            .Subscribe(BigTouchChanged.OnNext);

        LoseFocusEnable = gameInfoRepository.IsLoseFocus;
        this.WhenAnyValue(x => x.LoseFocusEnable)
            .Skip(1)
            .Subscribe(v => gameInfoRepository.IsLoseFocus = v);

        TouchToMouseEnable = gameInfoRepository.IsEnableTouchToMouse;
        this.WhenAnyValue(x => x.TouchToMouseEnable)
            .Skip(1)
            .Subscribe(v => gameInfoRepository.IsEnableTouchToMouse = v);

        this.WhenAnyValue(x => x.TouchToMouseEnable)
            .Do(v =>
            {
                if (v) TouchConversionHooker.Install();
                else TouchConversionHooker.UnInstall();
            }).Subscribe();

        // BAD access view directly
        System.Windows.Window? magWindow = null; 
        this.WhenAnyValue(x => x.AttachEnable)
          .Do(v =>
          {
              if (v)
              {
                  var data = ehConfigRepository.MagSourceInputString.Split(' ');
                  var ints = Array.ConvertAll(data, int.Parse);
                  magWindow = new View.Magnifier.MagWindow(ints[0], ints[1], ints[2], ints[3], ehConfigRepository.MagSmoothing);
                  magWindow.Show();
              }
              else
              {
                  magWindow?.Close();
              }
          }).Subscribe();
    }

    // Reactive attribute to enable INotifyPropertyChanged

    [Reactive]
    public AssistiveTouchPosition AssistiveTouchPosition { get; set; }

    public BehaviorSubject<bool> BigTouchChanged { get; }


    [Reactive]
    public bool LoseFocusEnable { get; set; }

    [Reactive]
    public bool TouchToMouseEnable { get; set; }


    [Reactive]
    public bool AttachEnable { get; set; }
}
