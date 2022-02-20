using System;
using System.Globalization;
using System.Windows.Data;

namespace WPF_Application.Converter
{
    /// <summary>
    /// Converts a <see cref="bool"/> value to a string direction representation. 
    /// </summary>
    public class BoolToDirectionConverter : IValueConverter
    {
        /// <summary>
        /// Converts a <see cref="bool"/> <paramref name="value"/> to a string direction representation. 
        /// </summary>
        /// <remarks>
        /// Converts true to "Vorwärts" and false to "Rückwärts".
        /// </remarks>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is bool val ? val ? "Vorwärts" : "Rückwärts" : throw new NotSupportedException();

        /// <summary>
        /// Not supported. Throws a <see cref="NotSupportedException"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
    }
}
