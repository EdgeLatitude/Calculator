using System;
using System.Globalization;
using Xamarin.Forms;

namespace Calculator.Mobile.Converters
{
    internal class NegateBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture) =>
            value is bool boolValue ?
                !boolValue :
                false;

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture) =>
            Convert(value, targetType, parameter, culture);
    }
}
