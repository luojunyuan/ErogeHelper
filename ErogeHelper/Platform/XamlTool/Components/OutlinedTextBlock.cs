using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;

namespace ErogeHelper.Platform.XamlTool.Components;

[ContentProperty(nameof(Text))]
public class OutlinedTextBlock : FrameworkElement
{
    private const double StrokeThickness = 1;

    public OutlinedTextBlock()
    {
        UpdateOutlinePen(StrokeThickness);
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        EnsureGeometry();

        drawingContext.DrawGeometry(Fill, null, _textGeometry);
        drawingContext.PushClip(_clipGeometry);
        drawingContext.DrawGeometry(null, _outlinePen, _textGeometry);
        drawingContext.Pop();
    }

    // Cacualte size required by elements
    protected override Size MeasureOverride(Size availableSize)
    {
        EnsureFormattedText();

        // constrain the formatted text according to the available size
        // the Math.Min call is important - without this constraint (which seems arbitrary, but is the maximum allowable text width), things blow up when availableSize is infinite in both directions
        // the Math.Max call is to ensure we don't hit zero, which will cause MaxTextHeight to throw
        _formattedText!.MaxTextWidth = Math.Min(3579139, availableSize.Width);
        _formattedText!.MaxTextHeight = Math.Max(0.0001d, availableSize.Height);

        // return the desired size
        return new Size(Math.Ceiling(_formattedText.Width), Math.Ceiling(_formattedText.Height));
    }

    // Apply layout
    protected override Size ArrangeOverride(Size finalSize)
    {
        EnsureFormattedText();

        // update the formatted text with the final size
        _formattedText!.MaxTextWidth = finalSize.Width;
        _formattedText!.MaxTextHeight = Math.Max(0.0001d, finalSize.Height);

        // need to re-generate the geometry now that the dimensions have changed
        _textGeometry = null;

        return finalSize;
    }

    #region Text
    /// <summary>Identifies the <see cref="Text"/> dependency property.</summary>
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text),
        typeof(string),
        typeof(OutlinedTextBlock),
        new FrameworkPropertyMetadata(string.Empty, OnTextChanged));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    private static void OnTextChanged(DependencyObject dependencyObject,
        DependencyPropertyChangedEventArgs e)
    {
        var outlinedTextBlock = (OutlinedTextBlock)dependencyObject;
        outlinedTextBlock._formattedText = null;
        outlinedTextBlock._textGeometry = null;

        outlinedTextBlock.InvalidateMeasure();
        outlinedTextBlock.InvalidateVisual();

        // After render or measure, arrange, new FormattedText would be created (Invalidated)
    }
    #endregion

    #region Fill
    /// <summary>Identifies the <see cref="Fill"/> dependency property.</summary>
    public static readonly DependencyProperty FillProperty = DependencyProperty.Register(
        nameof(Fill),
        typeof(Brush),
        typeof(OutlinedTextBlock),
        new FrameworkPropertyMetadata(Brushes.White, FrameworkPropertyMetadataOptions.AffectsRender));

    public Brush Fill
    {
        get => (Brush)GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }
    #endregion


    // Text Element Property With FormattedText
    #region FontFamily
    /// <summary>Identifies the <see cref="FontFamily"/> dependency property.</summary>
    public static readonly DependencyProperty FontFamilyProperty = TextElement.FontFamilyProperty.AddOwner(
        typeof(OutlinedTextBlock),
        new FrameworkPropertyMetadata(OnFormattedTextUpdated));

    public FontFamily FontFamily
    {
        get => (FontFamily)GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }
    #endregion

    #region FontStyle
    /// <summary>Identifies the <see cref="FontStyle"/> dependency property.</summary>
    public static readonly DependencyProperty FontStyleProperty = TextElement.FontStyleProperty.AddOwner(
        typeof(OutlinedTextBlock),
        new FrameworkPropertyMetadata(OnFormattedTextUpdated));

    public FontStyle FontStyle
    {
        get => (FontStyle)GetValue(FontStyleProperty);
        set => SetValue(FontStyleProperty, value);
    }
    #endregion

    #region FontWeight
    /// <summary>Identifies the <see cref="FontWeight"/> dependency property.</summary>
    public static readonly DependencyProperty FontWeightProperty = TextElement.FontWeightProperty.AddOwner(
        typeof(OutlinedTextBlock),
        new FrameworkPropertyMetadata(OnFormattedTextUpdated));

    public FontWeight FontWeight
    {
        get => (FontWeight)GetValue(FontWeightProperty);
        set => SetValue(FontWeightProperty, value);
    }
    #endregion

    #region FontStretch
    /// <summary>Identifies the <see cref="FontStretch"/> dependency property.</summary>
    public static readonly DependencyProperty FontStretchProperty = TextElement.FontStretchProperty.AddOwner(
        typeof(OutlinedTextBlock),
        new FrameworkPropertyMetadata(OnFormattedTextUpdated));

    public FontStretch FontStretch
    {
        get => (FontStretch)GetValue(FontStretchProperty);
        set => SetValue(FontStretchProperty, value);
    }
    #endregion

    #region FontSize
    /// <summary>Identifies the <see cref="FontSize"/> dependency property.</summary>
    public static readonly DependencyProperty FontSizeProperty = TextElement.FontSizeProperty.AddOwner(
        typeof(OutlinedTextBlock),
        new FrameworkPropertyMetadata(OnFormattedTextUpdated));

    [TypeConverter(typeof(FontSizeConverter))]
    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }
    #endregion

    #region TextWrapping
    /// <summary>Identifies the <see cref="TextWrapping"/> dependency property.</summary>
    public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.Register(
        nameof(TextWrapping),
        typeof(TextWrapping),
        typeof(OutlinedTextBlock),
        new FrameworkPropertyMetadata(TextWrapping.NoWrap, OnFormattedTextUpdated));

    public TextWrapping TextWrapping
    {
        get => (TextWrapping)GetValue(TextWrappingProperty);
        set => SetValue(TextWrappingProperty, value);
    }
    #endregion

    private static void OnFormattedTextUpdated(DependencyObject dependencyObject,
        DependencyPropertyChangedEventArgs e)
    {
        var outlinedTextBlock = (OutlinedTextBlock)dependencyObject;
        outlinedTextBlock.UpdateFormattedText();
        outlinedTextBlock._textGeometry = null;

        outlinedTextBlock.InvalidateMeasure();
        outlinedTextBlock.InvalidateVisual();
    }

    private Pen? _outlinePen;

    private void UpdateOutlinePen(double strokeThickness)
    {
        _outlinePen = new Pen(Brushes.Black, strokeThickness)
        {
            DashCap = PenLineCap.Round,
            EndLineCap = PenLineCap.Round,
            LineJoin = PenLineJoin.Round,
            StartLineCap = PenLineCap.Round,
        };
    }

    private FormattedText? _formattedText;

    private void EnsureFormattedText()
    {
        if (_formattedText != null)
            return;

        _formattedText = new FormattedText(
            Text,
            CultureInfo.CurrentUICulture,
            FlowDirection,
            new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
            FontSize,
            Fill,
            VisualTreeHelper.GetDpi(this).PixelsPerDip);

        UpdateFormattedText();
    }

    private void UpdateFormattedText()
    {
        if (_formattedText == null)
            return;

        _formattedText.MaxLineCount = TextWrapping == TextWrapping.NoWrap ? 1 : int.MaxValue;

        _formattedText.SetFontSize(FontSize);
        _formattedText.SetFontWeight(FontWeight);
        _formattedText.SetFontFamily(FontFamily);
        _formattedText.SetFontStretch(FontStretch);
        UpdateOutlinePen(FontSize / 32);
    }

    private Geometry? _textGeometry;
    private PathGeometry? _clipGeometry;

    /// <summary>
    /// Generate geometry form FormattedText
    /// </summary>
    private void EnsureGeometry()
    {
        if (_textGeometry != null)
            return;

        EnsureFormattedText();
        _textGeometry = _formattedText!.BuildGeometry(new Point(0, 0));
        
        _clipGeometry = Geometry.Combine(
            new RectangleGeometry(new Rect(0, 0, ActualWidth, ActualHeight)),
            _textGeometry,
            GeometryCombineMode.Exclude,
            null);
    }
}
