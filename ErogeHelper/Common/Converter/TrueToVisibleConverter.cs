using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ErogeHelper.Common.Converter
{
    /// <summary>
    /// Contains the converter and convert back methods for the boolean to visibility conversions
    /// </summary>
    public sealed class TrueToVisibleConverter : IValueConverter
    {
        /// <summary>
        /// Used to convert a boolean to a visibility
        /// </summary>
        /// <param name="value">This is the boolean input</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>Returns a visibility</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not bool)
            {
                //If there is an issue with the input, return collapsed
                return Visibility.Collapsed;
            }
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Used to take a visibility and returns a visibility
        /// </summary>
        /// <param name="value">This is the boolean input</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>Returns a visibility</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Visibility)
            {
                //If there is an issue with the input, return collapsed
                return Visibility.Collapsed;
            }
            return (Visibility)value == Visibility.Visible;
        }
    }
}