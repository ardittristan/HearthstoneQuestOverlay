using System;
using System.Globalization;
using System.Windows.Data;

namespace QuestOverlayPlugin.Controls
{
    #nullable enable
    public class ProgressBarFillWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2)
                return 0.0;
            if (!(values[0] is double num1))
                return 0.0;
            return values[1] is double num2 ? num2 * num1 : (object)0.0;
        }

        public object[]? ConvertBack(
            object value,
            Type[] targetTypes,
            object parameter,
            CultureInfo culture)
        {
            return null;
        }
    }
    #nullable restore
}
