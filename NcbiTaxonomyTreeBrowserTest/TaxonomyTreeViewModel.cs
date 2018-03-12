using System.ComponentModel;
using System.Runtime.CompilerServices;
using NcbiTaxonomyTreeBrowserTest.Annotations;

namespace NcbiTaxonomyTreeBrowserTest
{
    public class TaxonomyTreeViewModel : INotifyPropertyChanged
    {
        private bool showOnlyBdalNodes;
        public TreeViewData TreeViewData { get; set; }

        public TaxonomyTreeViewModel()
        {
            TreeViewData = new TreeViewData();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}