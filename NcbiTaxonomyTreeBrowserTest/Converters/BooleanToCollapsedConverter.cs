using System.Windows;

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

}
