using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using NCBITaxonomyTest;

namespace NcbiTaxonomyTreeBrowserTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            //DriveInfo[] drives = DriveInfo.GetDrives();
            //foreach(DriveInfo driveInfo in drives)
            //    trvStructure.Items.Add(CreateTreeItem(driveInfo));
        }


        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
        }

        private TaxNameComparer comparer = new TaxNameComparer(StringComparer.CurrentCulture);

        private async void TaxonomyNodeItem_Expanded(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is TreeViewItem item)
            {
                if (item.Header is TaxonomyNodeItem node)
                {
                    //node.SecondLevelItems.Add(new TaxonomyNodeItem(333, "new", node.Level+1));
                    //tokenSource.Cancel();
                    //tokenSource = new CancellationTokenSource();

                    //BindingOperations.EnableCollectionSynchronization(node.SecondLevelItems, node.Lock);
                   
                    //var task =  node.QuerySecondLevelItems(tokenSource.Token);
                    //await task;
                    //var result = task.Result;

                    foreach(var sItem in node.SecondLevelItems)
                    {

                        //var ids = result[sItem.Id];
                        try
                        {
                            var ids = TaxonomyNodeItem.BaseData.FindChilds(sItem.Id);
                            if (sItem.SecondLevelItems.Count != ids.Count())
                            {
                                var nameMap = new SortedDictionary<string, int>(comparer);

                                foreach (var id in ids)
                                {
                                    var name = TaxonomyNodeItem.BaseData.FindName(id);
                                    nameMap.Add(name.name, id);
                                }
                                foreach (var orderedEntry in nameMap)
                                {
                                    var nodeData = TaxonomyNodeItem.BaseData.FindNode(orderedEntry.Value);
//                                if (!sItem.SecondLevelItems.Any(nodeItem => nodeItem.Id == nodeData.Id))
                                    {
                                        sItem.SecondLevelItems.Add(new TaxonomyNodeItem(nodeData,
                                            $"{orderedEntry.Key} - {TaxonomyNodeItem.BaseData.FindClassName(nodeData.classId)} L{nodeData.Level} ({nodeData.SpeciesCount}/{nodeData.NodesCount})",
                                            node.Level + 1));
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }
        }
    }

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

    public class TaxonomyNodeItem 
    {
        public static TreeViewData BaseData; 


        public string DisplayName { get; set; }
        public int Id
        {
            get { return Node.Id; }
        }

        public Node Node { get; set; }

        public int Level { get; set; }

        public object Lock = new object();

        private IEnumerable<int> level2Nodes;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<TaxonomyNodeItem> SecondLevelItems { get; set; }


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
            SecondLevelItems = new ObservableCollection<TaxonomyNodeItem>();
        }

            
        public async Task<IDictionary<int, IEnumerable<int>>> QuerySecondLevelItems(CancellationToken ct)
        {
            Trace.WriteLine($"enter QuerySecondLevelItems");

            try
            {
                var task =  Task.Factory.StartNew(() =>
                {
                    Dictionary<int, IEnumerable<int>> result = new Dictionary<int, IEnumerable<int>>();
                    Trace.WriteLine($"start task QuerySecondLevelItems");
                    ct.ThrowIfCancellationRequested();
                    foreach (var secondLevelitem in SecondLevelItems)
                    {
                        if (ct.IsCancellationRequested)
                        {
                            // Clean up here, then...
                            ct.ThrowIfCancellationRequested();
                        }

                        result.Add(secondLevelitem.Id, secondLevelitem.QuerySecondLevelChilds().ToArray<int>());

                    }
                    if (ct.IsCancellationRequested)
                    {
                        // Clean up here, then...
                        ct.ThrowIfCancellationRequested();
                    }
                    Trace.WriteLine($"end task QuerySecondLevelItems");
                    return result;
                }, ct);
                
                await task;
                return task.Result;
            }
            catch (AggregateException e)
            {
                foreach (var v in e.InnerExceptions)
                    Console.WriteLine(e.Message + " " + v.Message);
            }
            catch (OperationCanceledException operationCanceledException)
            {
                Trace.WriteLine($"Op Canceled QuerySecondLevelItems");
            }
            Trace.WriteLine($"leave QuerySecondLevelItems");
            return null;
        }

        private IEnumerable<int> QuerySecondLevelChilds()
        {
            var bacs = BaseData.FindChilds(Id);
            return bacs;
        
        }
    }


    //public class TreeViewData : ObservableCollection<TaxonomyNodeItem>
    //{
    //    private int[][] nodes;
    //    public NcbiNodesParser NcbiNodesParser { get; set; }

    //    public IEnumerable<int> FindChilds(int parent)
    //    {
    //        return from p in nodes where p != null && p[1] == parent select p[0];
    //    }

    //    public TreeViewData()
    //    {
    //        NcbiNodesParser = new NcbiNodesParser(@"C:\Test\NcbiTaxonomy\nodes.dmp");
    //        nodes = NcbiNodesParser.Read();
    //        TaxonomyNodeItem.baseData = this;
    //        try
    //        {
    //            var bacs = FindChilds(2);
    //            var rootItem = new TaxonomyNodeItem(2, "Bacteria", 0);
    //            foreach (var bac in bacs)
    //            {
    //                rootItem.SecondLevelItems.Add(new TaxonomyNodeItem(bac, bac.ToString(), rootItem.Level + 1));
    //            }
    //            Add(rootItem);
    //        }
    //        catch (Exception ex)
    //        {

    //        }            
    //    }
    //}

    public class TreeViewData : ObservableCollection<TaxonomyNodeItem>
    {
        private SortedDictionary<int, Node> nodes;
        private IDictionary<int, TaxName> names;

        public NcbiNodesParser2 NcbiNodesParser { get; set; }

        public NcbiNamesParser NcbiNamesParser { get; set; }

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

        public TreeViewData()
        {
            NcbiNodesParser = new NcbiNodesParser2(@"C:\Test\NcbiTaxonomy\nodes.dmp");
            nodes = NcbiNodesParser.Read();
            foreach (var node in nodes)
            {
                if (node.Value.Id == 0)
                {
                    throw new Exception(".-(");
                }
            }
            
            //NcbiNodesParser.CalcAllNodesCount(nodes);

            NcbiNamesParser = new NcbiNamesParser(@"C:\Test\NcbiTaxonomy\names.dmp");
            names = NcbiNamesParser.Read();

            TaxonomyNodeItem.BaseData = this;
            try
            {
                var rootNode = FindNode(131567);
                var bacs = FindChilds(131567);
                var rootItem = new TaxonomyNodeItem(rootNode, $"{FindName(rootNode.Id).name} - {TaxonomyNodeItem.BaseData.FindClassName(rootNode.classId)} L{rootNode.Level} ({rootNode.SpeciesCount}/{rootNode.NodesCount})", 0);
                foreach (var bac in bacs)
                {
                    // resolve child
                    var node = nodes[bac];
                    //
                    rootItem.SecondLevelItems.Add(new TaxonomyNodeItem(node, $"{FindName(bac).name} - {TaxonomyNodeItem.BaseData.FindClassName(node.classId)} L{node.Level} ({node.SpeciesCount}/{node.NodesCount})", rootItem.Level + 1));
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
    }
}
