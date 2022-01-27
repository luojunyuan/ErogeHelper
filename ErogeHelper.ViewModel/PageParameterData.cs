using System.Reactive;
using System.Reactive.Linq;
using DynamicData.Operators;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace ErogeHelper.ViewModel;

public class PageParameterData : ReactiveObject
{
    public PageParameterData(int currentPage, int pageSize)
    {
        CurrentPage = currentPage;
        PageSize = pageSize;

        NextPageCommand = ReactiveCommand.Create(() => ++CurrentPage, Observable.Return(CurrentPage < PageCount));
        PreviousPageCommand = ReactiveCommand.Create(() => --CurrentPage, Observable.Return(CurrentPage > 1));
    }

    public ReactiveCommand<Unit, int> NextPageCommand { get; }
    public ReactiveCommand<Unit, int> PreviousPageCommand { get; }

    [Reactive]
    public int TotalCount { get; set; }

    [Reactive]
    public int PageCount { get; set; }

    [Reactive]
    public int CurrentPage { get; set; }

    [Reactive]
    public int PageSize { get; set; }

    public void Update(IPageResponse response)
    {
        CurrentPage = response.Page - 1;
        PageSize = response.PageSize;
        PageCount = response.Pages;
        TotalCount = response.TotalSize;
    }
}
