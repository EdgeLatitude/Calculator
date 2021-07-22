using System;
using System.Globalization;
using Xamarin.Forms;

namespace Calculator.Mobile.Converters
{
    internal class IsSelectedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture) =>
            value is bool boolValue ?
                boolValue ?
                    TextDecorations.Underline :
                    TextDecorations.None :
                TextDecorations.None;

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture) =>
            false;
    }
}
