using System.Reactive.Disposables;
using System.Windows;
using ErogeHelper.Shared.Enums;
using ReactiveUI;

namespace ErogeHelper.View.Preference;

public partial class MeCabPage
{
    public MeCabPage()
    {
        InitializeComponent();

        this.WhenActivated(d =>
        {
            this.OneWayBind(ViewModel,
                vm => vm.CanEnableMeCab,
                v => v.SelectMeCabDictButton.Visibility,
                alreadyHasDict => alreadyHasDict ? Visibility.Collapsed : Visibility.Visible).DisposeWith(d);
            this.BindCommand(ViewModel,
                vm => vm.SelectMeCabDict,
                v => v.SelectMeCabDictButton).DisposeWith(d);

            this.Bind(ViewModel,
                vm => vm.ShowJapanese,
                v => v.ShowJapaneseSwitch.IsOn).DisposeWith(d);
            this.OneWayBind(ViewModel,
                vm => vm.CanEnableMeCab,
                v => v.ShowJapaneseSwitch.IsEnabled).DisposeWith(d);

            this.Bind(ViewModel,
                vm => vm.KanaPosition,
                v => v.KanaPosButtons.SelectedItem,
                kanaType => kanaType switch
                {
                    KanaPosition.None => KanaDefault,
                    KanaPosition.Top => KanaTop,
                    KanaPosition.Bottom => KanaBottom,
                    _ => throw new NotSupportedException()
                },
                radioButton =>
                    radioButton == null ? KanaPosition.None : // Just hack, takes no effect for real situation
                    radioButton.Equals(KanaDefault) ? KanaPosition.None :
                    radioButton.Equals(KanaTop) ? KanaPosition.Top :
                    radioButton.Equals(KanaBottom) ? KanaPosition.Bottom :
                    throw new NotSupportedException()
                ).DisposeWith(d);

            this.Bind(ViewModel,
                vm => vm.KanaRuby,
                v => v.KanaRubyButtons.SelectedItem,
                kanaType => kanaType switch
                {
                    KanaRuby.Romaji => Romaji,
                    KanaRuby.Hiragana => Hiragana,
                    KanaRuby.Katakana => Katakana,
                    _ => throw new NotSupportedException()
                },
                radioButton =>
                    radioButton == null ? KanaRuby.Romaji :
                    radioButton.Equals(Romaji) ? KanaRuby.Romaji :
                    radioButton.Equals(Hiragana) ? KanaRuby.Hiragana :
                    radioButton.Equals(Katakana) ? KanaRuby.Katakana :
                    throw new NotSupportedException()
                ).DisposeWith(d);

            this.Bind(ViewModel,
                vm => vm.MojiDict,
                v => v.MojiDictToggle.IsOn).DisposeWith(d);
            this.Bind(ViewModel,
                vm => vm.JishoDict,
                v => v.JishoDictToggle.IsOn).DisposeWith(d);
        });
    }
}
