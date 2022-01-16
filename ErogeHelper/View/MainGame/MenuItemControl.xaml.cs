using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ErogeHelper.Shared.Contracts;
using ModernWpf.Controls;

namespace ErogeHelper.View.MainGame;

public partial class MenuItemControl : UserControl
{
    #region Symbol DependencyProperty
    /// <summary>Identifies the <see cref="Symbol"/> dependency property.</summary>
    public static readonly DependencyProperty SymbolProperty = DependencyProperty.Register(
        nameof(Symbol),
        typeof(Symbol),
        typeof(MenuItemControl),
        new PropertyMetadata(Symbol.Emoji, (d, e) => ((MenuItemControl)d).OnSymbolChanged(e)));

    /// <summary>
    /// Gets or sets the Segoe MDL2 Assets glyph used as the icon content.
    /// </summary>
    public Symbol Symbol
    {
        get => (Symbol)GetValue(SymbolProperty);
        set => SetValue(SymbolProperty, value);
    }

    private void OnSymbolChanged(DependencyPropertyChangedEventArgs e) =>
        ItemIcon.SetCurrentValue(SymbolIcon.SymbolProperty, (Symbol)e.NewValue);
    #endregion

    #region SymbolExtend DependencyProperty
    /// <summary>Identifies the <see cref="SymbolExtend"/> dependency property.</summary>
    public static readonly DependencyProperty SymbolExtendProperty = DependencyProperty.Register(
        nameof(SymbolExtend),
        typeof(SymbolExtend),
        typeof(MenuItemControl),
        new PropertyMetadata(SymbolExtend.TBD, (d, e) => ((MenuItemControl)d).OnSymbolExtendChanged(e)));

    /// <summary>
    /// Gets or sets the Segoe MDL2 Assets glyph used as the icon content.
    /// </summary>
    public SymbolExtend SymbolExtend
    {
        get => (SymbolExtend)GetValue(SymbolExtendProperty);
        set => SetValue(SymbolExtendProperty, value);
    }

    private void OnSymbolExtendChanged(DependencyPropertyChangedEventArgs e) =>
        ItemIcon.SetCurrentValue(SymbolIcon.SymbolProperty, (Symbol)e.NewValue);
    #endregion

    #region Text DependencyProperty
    /// <summary>Identifies the <see cref="Text"/> dependency property.</summary>
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text),
        typeof(string),
        typeof(MenuItemControl),
        new PropertyMetadata(string.Empty, (d, e) => ((MenuItemControl)d).OnTextChanged(e)));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    private void OnTextChanged(DependencyPropertyChangedEventArgs e) =>
        ItemText.SetCurrentValue(TextBlock.TextProperty, (string)e.NewValue);
    #endregion

    public event EventHandler? ClickEvent;

    public MenuItemControl()
    {
        InitializeComponent();
    }

    private readonly static Brush ItemPressedColor = new SolidColorBrush(Color.FromArgb(255, 111, 196, 241));

    private void ItemOnPreviewMouseLeftButtonDown(object sender, InputEventArgs e)
    {
        ItemIcon.SetCurrentValue(IconElement.ForegroundProperty, ItemPressedColor);
        ItemText.SetCurrentValue(TextBlock.ForegroundProperty, ItemPressedColor);
    }

    private void ItemOnPreviewMouseLeave(object sender, MouseEventArgs e)
    {
        ItemIcon.SetCurrentValue(IconElement.ForegroundProperty, Brushes.White);
        ItemText.SetCurrentValue(TextBlock.ForegroundProperty, Brushes.White);
    }

    private void ItemOnPreviewMouseLeftButtonUp(object sender, InputEventArgs e)
    {
        if (ItemIcon.Foreground != Brushes.White)
        {
            ItemIcon.SetCurrentValue(IconElement.ForegroundProperty, Brushes.White);
            ItemText.SetCurrentValue(TextBlock.ForegroundProperty, Brushes.White);

            ClickEvent?.Invoke(this, e);
        }
    }
}
