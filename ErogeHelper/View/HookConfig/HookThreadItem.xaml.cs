using System.Reactive.Disposables;
using System.Windows.Controls;
using ReactiveUI;

namespace ErogeHelper.View.HookConfig;

public partial class HookThreadItem
{
    public HookThreadItem()
    {
        InitializeComponent();

        this.WhenActivated(d =>
        {
            this.OneWayBind(ViewModel,
                vm => vm.Index,
                v => v.ThreadIndex.Text).DisposeWith(d);

            this.Bind(ViewModel,
                vm => vm.IsTextThread,
                v => v.TextRadioButton.IsChecked).DisposeWith(d);
            this.Bind(ViewModel,
                vm => vm.IsCharacterThread,
                v => v.CharacterRadioButton.IsChecked).DisposeWith(d);

            this.OneWayBind(ViewModel,
                vm => vm.HookCode,
                v => v.WholeBox.ToolTip).DisposeWith(d);
            this.OneWayBind(ViewModel,
                vm => vm.TotalText,
                v => v.TotalText.Text).DisposeWith(d);
        });
    }

    private RadioButton? _selectedRadioButton;

    private void OptionalRadioButtonCheck(object sender, System.Windows.RoutedEventArgs e)
    {
        if (sender is not RadioButton radioButton)
        {
            throw new InvalidCastException();
        }

        if (_selectedRadioButton is null)
        {
            _selectedRadioButton = radioButton;
        }
        else
        {
            if (radioButton.Equals(_selectedRadioButton))
            {
                radioButton.IsChecked = false;
                _selectedRadioButton = null;
            }
            else
            {
                _selectedRadioButton = radioButton;
            }
        }
    }
}
