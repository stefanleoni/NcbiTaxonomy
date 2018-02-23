using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace NcbiTaxonomyTreeBrowserTest.Converters
{
    public class BooleanConverter<T> : IValueConverter
    {
        public BooleanConverter(T trueValue, T falseValue, T nullValue)
        {
            True = trueValue;
            False = falseValue;
            Null = nullValue;
        }

        public T True { get; set; }
        public T False { get; set; }
        public T Null { get; set; }

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Null : value is bool && ((bool)value) ? True : False;
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is T && EqualityComparer<T>.Default.Equals((T)value, True);
        }
    }
}