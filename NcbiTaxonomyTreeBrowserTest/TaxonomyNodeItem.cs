using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using NCBITaxonomyTest;

namespace NcbiTaxonomyTreeBrowserTest
{
    public class TaxonomyNodeItem : TreeViewItemBase
    {
        public static TreeViewData BaseData; 


        public string DisplayName { get; set; }
        public int Id
        {
            get { return Node.Id; }
        }

        public Node Node { get; set; }

        public int Level { get; set; }

        private IEnumerable<int> level2Nodes;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<TaxonomyNodeItem> ChildItems
        {
            get { return childItems; }
        }


        public ObservableCollection<TaxonomyNodeItem> childItems = new ObservableCollection<TaxonomyNodeItem>();

        public TaxonomyNodeItem(Node node, string displayName, int level)
        {
            Node = node;
            if (node.Id == 0)
            {
                throw new Exception(".-(");
            }
            //Id = node.Id;
            DisplayName = displayName;
            Level = level;
        }

    }
}