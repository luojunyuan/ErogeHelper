using System.Collections.ObjectModel;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using ErogeHelper.Model.DataServices.Interface;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Shared;
using ErogeHelper.Shared.Contracts;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace ErogeHelper.ViewModel.Preference;

public class DanmakuViewModel : ReactiveObject, IRoutableViewModel
{
    public IScreen HostScreen => throw new NotImplementedException();

    public string UrlPathSegment => PageTag.General;

    public DanmakuViewModel(
        IEHConfigRepository? ehConfigRepository = null,
        ICommentRepository? commentRepository = null,
        IGameDataService? gameDataService = null)
    {
        ehConfigRepository ??= DependencyResolver.GetService<IEHConfigRepository>();
        commentRepository ??= DependencyResolver.GetService<ICommentRepository>();
        gameDataService ??= DependencyResolver.GetService<IGameDataService>();

        DanmakuEnable = ehConfigRepository.UseDanmaku;
        this.WhenAnyValue(x => x.DanmakuEnable)
            .Skip(1)
            .Subscribe(v => ehConfigRepository.UseDanmaku = v);

        var pager = _pageParameters.WhenAnyValue(
            vm => vm.CurrentPage, vm => vm.PageSize, (page, size) => new PageRequest(page, size))
            .StartWith(new PageRequest(1, 20))
            .DistinctUntilChanged()
            .Sample(TimeSpan.FromMilliseconds(100));

        var danmakuList = new SourceList<DanmakuItemModel>();
        danmakuList.Connect()
            .Sort(SortExpressionComparer<DanmakuItemModel>.Descending(t => t.CreationTime))
            .Page(pager)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Do(changes => _pageParameters.Update(changes.Response))
            .Bind(out _comments)
            .Subscribe();

        Observable.Start(() => danmakuList.AddRange(
            commentRepository.GetAllCommentOfGame(gameDataService.Md5)
                .Select(x => new DanmakuItemModel()
                {
                    CreationTime = x.CreationTime,
                    Text = x.Text,
                    Danmaku = x.UserComment,
                    Username = x.Username
                })));
        // Sync simulator only need `ids`
    }

    [Reactive]
    public bool DanmakuEnable { get; set; }

    private readonly ReadOnlyObservableCollection<DanmakuItemModel> _comments;
    public ReadOnlyObservableCollection<DanmakuItemModel> Comments => _comments;

    private readonly PageParameterData _pageParameters = new(1, 20);

    public class DanmakuItemModel
    {
        public DateTime CreationTime { get; set; }

        public string Text { get; set; } = string.Empty;

        public string Danmaku { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;
    }
}
