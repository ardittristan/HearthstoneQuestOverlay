﻿namespace QuestOverlayPlugin.Stub
{
#if DEBUG

    using System;
    using System.Globalization;
    using System.Windows.Data;

    #nullable enable
    public class ProgressBarFillWidthConverterStub : IMultiValueConverter
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

#else

    using Hearthstone_Deck_Tracker.Controls.Overlay.Mercenaries;

    public class ProgressBarFillWidthConverterStub : ProgressBarFillWidthConverter { }

#endif
}
