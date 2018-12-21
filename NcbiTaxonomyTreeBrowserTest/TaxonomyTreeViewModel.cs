using System.ComponentModel;
using System.Runtime.CompilerServices;
using NcbiTaxonomyTreeBrowserTest.Annotations;

namespace NcbiTaxonomyTreeBrowserTest
{
    public class TaxonomyTreeViewModel : INotifyPropertyChanged
    {
        private bool showOnlyBdalNodes;
        private string searchSpecies;
        public TreeViewData TreeViewData { get; set; }

        public TaxonomyTreeViewModel()
        {
            TreeViewData = new TreeViewData((status, l, arg3) =>
            {

            });

            PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(SearchSpecies)))
            {
                TreeViewData.FindName(SearchSpecies);
            }
        }

        public string SearchSpecies
        {
            get => searchSpecies;
            set
            {
                if (value == searchSpecies) return;
                searchSpecies = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}