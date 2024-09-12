﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CebMaui.Helpers
{
	public class InverseBooleanConverter : IValueConverter {
		public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value is false;

		public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}