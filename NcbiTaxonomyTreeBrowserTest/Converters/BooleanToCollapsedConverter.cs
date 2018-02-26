using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NcbiTaxonomyTreeBrowserTest.Converters
{
    public sealed class BooleanToCollapsedConverter : BooleanConverter<Visibility>
    {
        public BooleanToCollapsedConverter() :
            base(Visibility.Collapsed, Visibility.Visible, Visibility.Collapsed) { }
    }

    public sealed class NullBooleanToVisibleConverter : BooleanConverter<Visibility>
    {
        public NullBooleanToVisibleConverter() :
            base(Visibility.Collapsed, Visibility.Collapsed, Visibility.Visible) { }
    }

    public sealed class BooleanToVisibleConverter : BooleanConverter<Visibility>
    {
        public BooleanToVisibleConverter() :
            base(Visibility.Visible, Visibility.Collapsed, Visibility.Collapsed)
        { }
    }

    public sealed class IntToVisibleConverter : IValueConverter
    {
        public IntToVisibleConverter() 
        { }

        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int i)
            {
                return i > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
    
}
