using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Controls;
using ErogeHelper.ViewModel.HookConfig;
using ReactiveUI;

namespace ErogeHelper.View.HookConfig;

public partial class HookThreadItem : IViewFor<HookThreadItemViewModel>
{
    #region ViewModel DependencyProperty
    /// <summary>Identifies the <see cref="ViewModel"/> dependency property.</summary>
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
        nameof(ViewModel),
        typeof(HookThreadItemViewModel),
        typeof(HookThreadItem));

    public HookThreadItemViewModel? ViewModel
    {
        get => (HookThreadItemViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => SetValue(ViewModelProperty, (HookThreadItemViewModel?)value);
    }
    #endregion

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
