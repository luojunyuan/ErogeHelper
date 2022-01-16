using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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

    private event EventHandler? ClickEvent;

    public MenuItemControl()
    {
        InitializeComponent();

        Loaded += (s, e) =>
        {
            ItemIcon.SetCurrentValue(SymbolIcon.SymbolProperty, Symbol);
        };
    }

    private void MenuIconOnLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is SymbolIcon menuIcon)
        {
            menuIcon.Symbol = Symbol;
        }
    }

    private readonly static Brush ItemPressedColor = new SolidColorBrush(Color.FromArgb(255, 111, 196, 241));

    private void ItemOnPreviewMouseLeftButtonDown(object sender, MouseEventArgs e)
    {
        ItemIcon.SetCurrentValue(IconElement.ForegroundProperty, ItemPressedColor);
        ItemText.SetCurrentValue(TextBlock.ForegroundProperty, ItemPressedColor);
    }

    private void ItemOnPreviewMouseLeave(object sender, MouseEventArgs e)
    {
        ItemIcon.SetCurrentValue(IconElement.ForegroundProperty, Brushes.White);
        ItemText.SetCurrentValue(TextBlock.ForegroundProperty, Brushes.White);
    }

    private void ItemOnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (ItemIcon.Foreground != Brushes.White)
        {
            ItemIcon.SetCurrentValue(IconElement.ForegroundProperty, Brushes.White);
            ItemText.SetCurrentValue(TextBlock.ForegroundProperty, Brushes.White);

            ClickEvent?.Invoke(this, e);
        }
    }
}
