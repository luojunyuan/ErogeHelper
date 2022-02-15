using System.Reactive;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using ErogeHelper.Model.Repositories.Interface;
using ErogeHelper.Model.Services.Interface;
using ErogeHelper.Shared;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;
using Splat;

namespace ErogeHelper.ViewModel.HookConfig;

public class TextRegExpViewModel : ReactiveValidationObject
{
    public Interaction<(IEnumerable<long> Handles, string Text), 
        (bool CanSubmit, string RegExp, string CurrentText)> Show { get; } = new();
    
    public IDisposable TextCleanDisposal;

    public TextRegExpViewModel(
        ITextractorService? textractorService = null,
        IGameInfoRepository? gameInfoRepository = null)
    {
        textractorService ??= DependencyResolver.GetService<ITextractorService>();
        gameInfoRepository ??= DependencyResolver.GetService<IGameInfoRepository>();

        // QUESTION: Aggregate regexp in textractor setting?
        RegExp = gameInfoRepository.GameInfo.RegExp;

        TextCleanDisposal = textractorService.Data
            .Where(hp => SelectedHandles.Contains(hp.Handle))
            .Select(hp => hp.Text)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Do(text => CurrentText = text)
            .Subscribe(text => CurrentWrapperText = Utils.TextEvaluateWrapperWithRegExp(text, RegExp ?? string.Empty));

        var regexpValidation = this.ValidationRule(
            vm => vm.RegExp,
            CodeValidateRegExp,
            Shared.Languages.Strings.HookPage_InvalidRegExp);

        this.WhenAnyValue(x => x.RegExp, expr => string.IsNullOrEmpty(expr) || regexpValidation.IsValid)
            .Do(canSubmit => Observable.Return(canSubmit)
                .Where(cansubmit => cansubmit)
                .Subscribe(_ => 
                    CurrentWrapperText = Utils.TextEvaluateWrapperWithRegExp(CurrentText, RegExp ?? string.Empty)))
            .ToPropertyEx(this, x => x.CanSubmit);

        RegExp1 = ReactiveCommand.Create(() => RegExp = string.IsNullOrWhiteSpace(RegExp) ? Tag1 : $"{RegExp}|{Tag1}");
        RegExp2 = ReactiveCommand.Create(() => RegExp = string.IsNullOrWhiteSpace(RegExp) ? Tag2 : $"{RegExp}|{Tag2}");
        RegExp3 = ReactiveCommand.Create(() => RegExp = string.IsNullOrWhiteSpace(RegExp) ? Tag3 : $"{RegExp}|{Tag3}");
        RegExp4 = ReactiveCommand.Create(() => RegExp = string.IsNullOrWhiteSpace(RegExp) ? Tag4 : $"{RegExp}|{Tag4}");
        RegExp5 = ReactiveCommand.Create(() => RegExp = string.IsNullOrWhiteSpace(RegExp) ? Tag5 : $"{RegExp}|{Tag5}");
    }

    public IEnumerable<long> SelectedHandles { get; set; } = Enumerable.Empty<long>();

    public string CurrentText { get; set; } = string.Empty;

    private const string Tag1 = ".*(?=[「|『])";
    private const string Tag2 = "(?<=[」|』]).*";
    private const string Tag3 = "<.*?>";
    private const string Tag4 = "_r|<br>|#n|\\n|\\\\n";
    private const string Tag5 = "[\\x00-\\xFF]";

    public ReactiveCommand<Unit, string> RegExp1 { get; } 
    public ReactiveCommand<Unit, string> RegExp2 { get; } 
    public ReactiveCommand<Unit, string> RegExp3 { get; } 
    public ReactiveCommand<Unit, string> RegExp4 { get; }
    public ReactiveCommand<Unit, string> RegExp5 { get; }

    [Reactive]
    public string CurrentWrapperText { get; set; } = string.Empty;

    [Reactive]
    public string? RegExp { get; set; }

    [ObservableAsProperty]
    public bool CanSubmit { get; }

    private bool CodeValidateRegExp(string? pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            return true;

        if (pattern[^1] == '|')
            return false;

        const RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Compiled;

        try
        {
            _ = new Regex(pattern, options);
        }
        catch (ArgumentException)
        {
            this.Log().Debug("Checking RegExp format, it's fine exception");
            return false;
        }

        return true;
    }
}
