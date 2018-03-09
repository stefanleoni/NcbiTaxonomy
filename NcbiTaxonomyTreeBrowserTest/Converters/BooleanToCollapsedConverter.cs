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
            base(Visibility.Visible, Visibility.Hidden, Visibility.Collapsed)
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
                return i > 0 ? Visibility.Visible : Visibility.Hidden;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
    
    public sealed class CountAndBrukerToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {

            int count = values[0] == DependencyProperty.UnsetValue ? 0 : (int)values[0];
            bool isBruker = values[1] != DependencyProperty.UnsetValue && (bool)values[1];
            if (count > 0 || isBruker)
            {
                return Visibility.Visible;
            }

            return Visibility.Hidden;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class CountAndBrukerToBooleanConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            int count = (int)values[0];
            bool isBruker = (bool)values[1];
            if (count > 0 || isBruker)
            {
                return true;
            }

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
