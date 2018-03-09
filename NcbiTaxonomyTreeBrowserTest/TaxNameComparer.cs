using System.Collections.Generic;

namespace NcbiTaxonomyTreeBrowserTest
{
    public class TaxNameComparer : IComparer<string>
    {
        #region Implementation of IComparer<in string>
        private readonly IComparer<string> _baseComparer;
        public TaxNameComparer(IComparer<string> baseComparer)
        {
            _baseComparer = baseComparer;
        }

        public int Compare(string x, string y)
        {
            if (!string.IsNullOrEmpty(x) && !string.IsNullOrEmpty(y)
                                         && x.StartsWith("environmental") && y.StartsWith("unclassi"))
            {
                return -1;
            }
            if (!string.IsNullOrEmpty(x) && !string.IsNullOrEmpty(y)
                                         &&  x.StartsWith("unclassi") && y.StartsWith("environmental") )
            {
                return 1;
            }
            if (x != null && x.StartsWith("unclass"))
            {
                return 1;
            }
            if (y != null && y.StartsWith("unclass"))
            {
                return -1;
            }
            if (x != null && x.StartsWith("environmental"))
            {
                return 1;
            }
            if (y != null && y.StartsWith("environmental"))
            {
                return -1;
            }
            return _baseComparer.Compare(x, y);
        }
        #endregion
    }
}