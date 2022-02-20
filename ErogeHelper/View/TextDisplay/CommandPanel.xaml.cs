using System.Windows.Controls;
using ModernWpf.Controls;

namespace ErogeHelper.View.TextDisplay;

public partial class CommandPanel
{
    public CommandPanel()
    {
        InitializeComponent();
    }

#pragma warning disable CA1822 // Mark members as static
    private void ToggleSplitButtonOnClick(SplitButton sender, SplitButtonClickEventArgs args)
    {
        var textAlignSplitButton = (ToggleSplitButton)sender;

        textAlignSplitButton.Flyout.ShowAt(textAlignSplitButton);
        textAlignSplitButton.IsChecked = true;
    }

    private void ButtonOnClick(object sender, System.Windows.RoutedEventArgs e)
    {
        var textAlignButton = (Button)sender;
        var textAlignSymbolIcon = (SymbolIcon)textAlignButton.Content;

        CurrentTextAlignSymbol.SetCurrentValue(SymbolIcon.SymbolProperty, textAlignSymbolIcon.Symbol);

        TextAlignSplitButton.Flyout.Hide();
    }
}
