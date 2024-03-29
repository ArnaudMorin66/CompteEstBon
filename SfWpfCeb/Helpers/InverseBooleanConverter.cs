﻿//-----------------------------------------------------------------------
// <copyright file="InverseBooleanConverter.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Globalization;
using System.Windows.Data;


// ReSharper disable once CheckNamespace
namespace CompteEstBon.Helpers;

[ValueConversion(typeof(bool), typeof(bool))]
public class InverseBooleanConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is false;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException(
        );
}
