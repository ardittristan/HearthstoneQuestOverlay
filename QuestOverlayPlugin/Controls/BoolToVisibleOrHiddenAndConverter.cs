﻿using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace QuestOverlayPlugin.Controls
{
	internal class BoolToVisibleOrHiddenAndConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) =>
			values.All(v => v is bool b && b) ? Visibility.Visible : Visibility.Hidden;

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => null!;
	}
}
