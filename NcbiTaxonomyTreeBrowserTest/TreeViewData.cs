using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using NCBITaxonomyTest;

namespace NcbiTaxonomyTreeBrowserTest
{
    public class TreeViewData : ObservableCollection<TaxonomyNodeItem>
    {
        private SortedDictionary<int, Node> nodes;
        private Dictionary<int, TaxName> names;
        private bool isNcbiVisible;

        public NcbiNodesParser NcbiNodesParser { get; set; }

        public NcbiNamesParser NcbiNamesParser { get; set; }

        public bool IsNcbiVisible
        {
            get { return isNcbiVisible; }
            set
            {
                isNcbiVisible = value;
            }
        }

        public IEnumerable<int> FindChilds(int parent)
        {
            //return from p in nodes where p != null && p[1] == parent select p[0];
            return nodes[parent].Childs;
        }

        public TaxName FindName(int id)
        {
            return names[id];
        }

        public string FindClassName(int id)
        {
            return NcbiNodesParser.ClassNameMap[id];
        }

        private TaxDumpSource taxDumpSource;

        public TreeViewData(Action<DownloadProgressStatus, long, long> progressInfo)
        {
            SearchResult = new ObservableCollection<ListViewNode>();
            NcbiNodesParser = new NcbiNodesParser();
            taxDumpSource = new TaxDumpSource(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Bruker", "NcbiDump"));
            taxDumpSource.Create(progressInfo);

            //nodes = NcbiNodesParser.Read(@"C:\Test\NcbiTaxonomy\nodes.dmp");
            var nodeTask = new Task<SortedDictionary<int, Node>>(() =>
            {
                var _nodes =  NcbiNodesParser.Read(taxDumpSource.NodesDumpFile);
                NcbiNodesParser.Add(taxDumpSource.BrukerNodesDumpFile, _nodes);
                return _nodes;
            });
            nodeTask.Start();

            

            NcbiNamesParser = new NcbiNamesParser();
            //names = NcbiNamesParser.Read(@"C:\Test\NcbiTaxonomy\names.dmp");
            var namesTask = new Task<Dictionary<int, TaxName>>(() =>
            {
                var _names = NcbiNamesParser.Read(taxDumpSource.NamesDumpFile);
                NcbiNamesParser.Add(_names, taxDumpSource.BrukerNamesDumpFile);
                return _names;
            });
            namesTask.Start();
            //var brukerReader = new BrukerNodesParser(@"C:\Test\NcbiTaxonomy\bruker.dmp");
            //var brukerNodes = brukerReader.Read(NcbiNodesParser.rankMap["no rank"]);
            //brukerReader.MergeBrukerNodesInto(nodes, names, brukerNodes);
            Task.WaitAll(new Task[] { nodeTask, namesTask });
            nodes = nodeTask.Result;
            names = namesTask.Result;

            NcbiNodesParser.CalcAllNodesCount(nodes);
            NcbiNodesParser.CalcAllSpeciesCount(nodes);

            IsNcbiVisible = false;
                
            TaxonomyNodeItem.BaseData = this;
            try
            {
                var rootNode = FindNode(131567);
                var bacs = FindChilds(131567);
                //var rootItem = new TaxonomyNodeItem(rootNode, $"{FindName(rootNode.Id).name} - {TaxonomyNodeItem.BaseData.FindClassName(rootNode.ClassId)} L{rootNode.Level} ({rootNode.BrukerCount}/{rootNode.SpeciesCount}/{rootNode.NodesCount})", 0);
                var rootItem = new TaxonomyNodeItem(null, rootNode, $"{FindName(rootNode.Id).name} - {TaxonomyNodeItem.BaseData.FindClassName(rootNode.ClassId)} ({rootNode.BrukerCount}/{rootNode.SpeciesCount}/{rootNode.NodesCount})", 0);
                BindingOperations.EnableCollectionSynchronization(rootItem.ChildItems, rootItem.childItems);

                foreach (var bac in bacs)
                {
                    // resolve child
                    var node = nodes[bac];
                    var item = new TaxonomyNodeItem(null, node,
                        $"{FindName(bac).name} - {TaxonomyNodeItem.BaseData.FindClassName(node.ClassId)} L{node.Level} ({rootNode.BrukerCount}/{node.SpeciesCount}/{node.NodesCount})",
                        rootItem.Level + 1);
                    BindingOperations.EnableCollectionSynchronization(item.ChildItems, item.childItems);

                    rootItem.ChildItems.Add(item);
                }
                Add(rootItem);
            }
            catch (Exception ex)
            {

            }            
        }

        public Node FindNode(int id)
        {
            return nodes[id];
        }

        public ObservableCollection<ListViewNode> SearchResult { get; set; }

        public void FindName(string searchSpecies)
        {
            SearchResult.Clear();

            var result = names.Where(pair => pair.Value.name.StartsWith(searchSpecies));
            if (result != null)
            {
                foreach (var keyValuePair in result)
                {
                    SearchResult.Add(new ListViewNode(nodes[keyValuePair.Key], keyValuePair.Value.name));
                }
            }

            //var result = names.FirstOrDefault(pair => pair.Value.name.StartsWith(searchSpecies));
                //if (result.Value != null)
                //{
                //    var node = nodes[result.Key];
                //    if (this[0].Id == node.Id)
                //    {
                //        this[0].IsExpanded = true;
                //        this[0].IsSelected = true;
                //        return;
                //    }
                //    var found = WalkTree(this[0], node.Id);

                //}
        }

        //bool WalkTree(TaxonomyNodeItem taxonomyNodeItem, int nodeId)
        //{
        //    if (taxonomyNodeItem.ChildItems?.Any() == true)
        //    {
        //        foreach (var childItem in taxonomyNodeItem.ChildItems)
        //        {
        //            if (childItem.Id == nodeId)
        //                return true;
        //            return WalkTree(childItem, nodeId);
        //        }
        //    }
        //    return false;
        //}
    }
}