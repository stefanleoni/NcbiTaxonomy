using System;
using System.Globalization;
using System.Windows.Data;

namespace NcbiTaxonomyTreeBrowserTest
{
    public class TaxNodeToUrlConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TaxonomyNodeItem node)
            {
                var res =
                    $"https://www.ncbi.nlm.nih.gov/Taxonomy/Browser/wwwtax.cgi?mode=Tree&id={node.Id}&lvl=1&lin=f&keep=1&srchmode=1&unlock";
                return res;
            }

            return "https://www.ncbi.nlm.nih.gov/Taxonomy/taxonomyhome.html/";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}