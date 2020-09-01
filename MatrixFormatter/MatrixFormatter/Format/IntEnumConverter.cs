using System;
using System.Globalization;
using Xamarin.Forms;

namespace MatrixFormatter.Format
{
    //From https://alexdunn.org/2017/05/16/xamarin-tip-binding-a-picker-to-an-enum/
    public class IntEnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) 
            => value is Enum ? (int) value : 0;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) 
            => value is int ? Enum.ToObject(targetType, value) : 0;
    }
}