using System.Windows;
using System.Windows.Controls;

namespace ErogeHelper.XamlTool.Behaviors;

public class ScrollToEndBehavior
{
    public static readonly DependencyProperty TextChangedProperty =
        DependencyProperty.RegisterAttached(
            "TextChanged",
            typeof(bool),
            typeof(ScrollToEndBehavior),
            new UIPropertyMetadata(false, OnTextChangedChanged)
        );

    /// <summary>Helper for getting <see cref="TextChangedProperty"/> from <paramref name="dependencyObject"/>.</summary>
    /// <param name="dependencyObject"><see cref="DependencyObject"/> to read <see cref="TextChangedProperty"/> from.</param>
    /// <returns>TextChanged property value.</returns>
    [AttachedPropertyBrowsableForType(typeof(DependencyObject))]
    public static bool GetTextChanged(DependencyObject dependencyObject)
    {
        return (bool)dependencyObject.GetValue(TextChangedProperty);
    }

    /// <summary>Helper for setting <see cref="TextChangedProperty"/> on <paramref name="dependencyObject"/>.</summary>
    /// <param name="dependencyObject"><see cref="DependencyObject"/> to set <see cref="TextChangedProperty"/> on.</param>
    /// <param name="value">TextChanged property value.</param>
    public static void SetTextChanged(DependencyObject dependencyObject, bool value)
    {
        dependencyObject.SetValue(TextChangedProperty, value);
    }

    private static void OnTextChangedChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        var newValue = (bool)e.NewValue;

        if (dependencyObject is not TextBox textBox || (bool)e.OldValue == newValue)
            return;

        static void Handler(object sender, TextChangedEventArgs _) => ((TextBox)sender).ScrollToEnd();

        if (newValue)
            textBox.TextChanged += Handler;
        else
        {
            textBox.TextChanged -= Handler;
        }
    }
}
