using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ErogeHelper.Platform.XamlTool;
using ErogeHelper.Shared.Contracts;
using ModernWpf.Controls;
using Splat;

namespace ErogeHelper.View.MainGame.AssistiveTouchMenu;

public partial class MenuItemToggleControl : IEnableLogger, IMenuItemBackground
{
    #region Symbol DependencyProperty
    /// <summary>Identifies the <see cref="Symbol"/> dependency property.</summary>
    public static readonly DependencyProperty SymbolProperty = DependencyProperty.Register(
        nameof(Symbol),
        typeof(Symbol),
        typeof(MenuItemToggleControl),
        new PropertyMetadata(Symbol.Emoji2, (d, e) => ((MenuItemToggleControl)d).OnSymbolChanged(e)));

    /// <summary>
    /// Gets or sets the Segoe MDL2 Assets glyph used as the icon content.
    /// </summary>
    public Symbol Symbol
    {
        get => (Symbol)GetValue(SymbolProperty);
        set => SetValue(SymbolProperty, value);
    }

    protected void OnSymbolChanged(DependencyPropertyChangedEventArgs e) =>
        ItemIcon.SetCurrentValue(SymbolIcon.SymbolProperty, (Symbol)e.NewValue);
    #endregion

    #region SymbolExtend DependencyProperty
    /// <summary>Identifies the <see cref="SymbolExtend"/> dependency property.</summary>
    public static readonly DependencyProperty SymbolExtendProperty = DependencyProperty.Register(
        nameof(SymbolExtend),
        typeof(SymbolExtend),
        typeof(MenuItemToggleControl),
        new PropertyMetadata(SymbolExtend.TBD, (d, e) => ((MenuItemToggleControl)d).OnSymbolExtendChanged(e)));

    /// <summary>
    /// Gets or sets the Segoe MDL2 Assets glyph used as the icon content.
    /// </summary>
    public SymbolExtend SymbolExtend
    {
        get => (SymbolExtend)GetValue(SymbolExtendProperty);
        set => SetValue(SymbolExtendProperty, value);
    }

    protected void OnSymbolExtendChanged(DependencyPropertyChangedEventArgs e) =>
        ItemIcon.SetCurrentValue(SymbolIcon.SymbolProperty, (Symbol)e.NewValue);
    #endregion

    #region Text DependencyProperty
    /// <summary>Identifies the <see cref="Text"/> dependency property.</summary>
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text),
        typeof(string),
        typeof(MenuItemToggleControl),
        new PropertyMetadata(string.Empty, (d, e) => ((MenuItemToggleControl)d).OnTextChanged(e)));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    protected void OnTextChanged(DependencyPropertyChangedEventArgs e) =>
        ItemText.SetCurrentValue(TextBlock.TextProperty, (string)e.NewValue);
    #endregion

    #region IsOn DependencyProperty
    /// <summary>Identifies the <see cref="IsOn"/> dependency property.</summary>
    public static readonly DependencyProperty IsOnProperty = DependencyProperty.Register(
        nameof(IsOn),
        typeof(bool),
        typeof(MenuItemToggleControl),
        new PropertyMetadata(false, (d, e) => ((MenuItemToggleControl)d).OnIsOnChanged(e)));

    public bool IsOn
    {
        get => (bool)GetValue(IsOnProperty);
        set => SetValue(IsOnProperty, value);
    }

    protected void OnIsOnChanged(DependencyPropertyChangedEventArgs e) =>
        SetItemForegroundColor((bool)e.NewValue ? ItemPressedColor : Brushes.White);
    #endregion

    public MenuItemToggleControl()
    {
        InitializeComponent();
    }

    public void TransparentBackground(bool transparent) =>
        SetCurrentValue(BackgroundProperty, transparent ? Brushes.Transparent : XamlResource.AssistiveTouchBackground);

    private static readonly Brush ItemPressedColor = new SolidColorBrush(Color.FromArgb(255, 111, 196, 241));

    private bool _buttonPressed;

    private void ItemOnPreviewMouseLeftButtonDown(object sender, InputEventArgs e)
    {
        _buttonPressed = true;
        SetItemForegroundColor(IsOn ? Brushes.DeepSkyBlue : Brushes.Gray);
    }

    private void ItemOnPreviewMouseLeave(object sender, InputEventArgs e)
    {
        _buttonPressed = false;
        SetItemForegroundColor(IsOn ? ItemPressedColor : Brushes.White);
    }

    private void SetItemForegroundColor(Brush color)
    {
        ItemIcon.SetCurrentValue(IconElement.ForegroundProperty, color);
        ItemText.SetCurrentValue(TextBlock.ForegroundProperty, color);
    }

    private void ItemOnPreviewMouseLeftButtonUp(object sender, InputEventArgs e)
    {
        if (_buttonPressed)
        {
            SetCurrentValue(IsOnProperty, !IsOn);
            _buttonPressed = false;
        }
    }

    private void ItemOnTouchUp(object sender, TouchEventArgs e)
    {
        if (_buttonPressed)
        {
            SetCurrentValue(IsOnProperty, !IsOn);
            _buttonPressed = false;
        }
    }
}
