using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using TrainDatabase.Z21Client.Enum;

namespace WPF_Application.Converter
{
    internal class TrackPowerToBoolConverter : IValueConverter
    {
        /// <summary>
        /// Converts a <paramref name="value"/> <see cref="TrackPower"/> object to a <see cref="bool"/> value. True if the <paramref name="value"/> is <see cref="TrackPower.ON"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is TrackPower tp ? tp == TrackPower.ON : throw new NotSupportedException();

        /// <summary>
        /// Not supported.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value is bool b ? (b ? TrackPower.ON : TrackPower.OFF) : throw new NotSupportedException();
    }
}
