using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace QuestOverlayPlugin.Controls;

internal class BoolToVisibleOrHiddenConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        (bool)value ? Visibility.Visible : Visibility.Hidden;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null!;
}