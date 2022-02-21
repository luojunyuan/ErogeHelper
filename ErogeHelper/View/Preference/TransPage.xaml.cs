using System.Reactive.Disposables;
using ReactiveUI;

namespace ErogeHelper.View.Preference;

public partial class TransPage
{
    public TransPage()
    {
        InitializeComponent();
        AiueoLink.NavigateUri = new Uri(Shared.Languages.Strings.TransPage_AiueoLink);
        TaeKimGrammerLink.NavigateUri = new Uri(Shared.Languages.Strings.TransPage_TaeKimGrammerLink);

        this.WhenActivated(d =>
        {
        });
    }
}
