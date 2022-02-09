using System.Reactive.Linq;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Model.Services.Function;
using ErogeHelper.Shared;
using ReactiveUI;

namespace ErogeHelper.ViewModel.MainGame;

public class DanmakuCanvasViewModel : ReactiveObject, IActivatableViewModel
{
    public ViewModelActivator Activator => new();

    private readonly ScenarioContext _scenarioContext;
    private readonly ICommentRepository _commentRepository;

    public DanmakuCanvasViewModel(
        ScenarioContext? scenarioContext = null,
        ICommentRepository? commentRepository = null)
    {
        _scenarioContext = scenarioContext ?? DependencyResolver.GetService<ScenarioContext>();
        _commentRepository = commentRepository ?? DependencyResolver.GetService<ICommentRepository>();

        this.WhenActivated(d => d(_scenarioContext));
    }

    public IObservable<string> NewDanmakuTerm => 
        _scenarioContext
            .ScenarioHash
            .SelectMany(hash => _commentRepository.GetAllCommentByHash(hash))
            .Take(20);
}
