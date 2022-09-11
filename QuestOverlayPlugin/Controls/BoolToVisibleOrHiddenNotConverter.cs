using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace QuestOverlayPlugin.Controls;

internal class BoolToVisibleOrHiddenNotConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        (bool)value ? Visibility.Hidden : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null!;
}