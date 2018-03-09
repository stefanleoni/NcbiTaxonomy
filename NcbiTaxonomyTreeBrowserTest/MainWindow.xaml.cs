using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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
            //DataContext = this;
            //DriveInfo[] drives = DriveInfo.GetDrives();
            //foreach(DriveInfo driveInfo in drives)
            //    trvStructure.Items.Add(CreateTreeItem(driveInfo));
            Browser.Navigated += new NavigatedEventHandler(Target);

        }

        private void Target(object sender, NavigationEventArgs navigationEventArgs)
        {
            SetSilent(Browser, true); // make it silent
        }

        public static void SetSilent(WebBrowser browser, bool silent)
        {
            if (browser == null)
                throw new ArgumentNullException("browser");

            // get an IWebBrowser2 from the document
            IOleServiceProvider sp = browser.Document as IOleServiceProvider;
            if (sp != null)
            {
                Guid IID_IWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
                Guid IID_IWebBrowser2 = new Guid("D30C1661-CDAF-11d0-8A3E-00C04FC9E26E");

                object webBrowser;
                sp.QueryService(ref IID_IWebBrowserApp, ref IID_IWebBrowser2, out webBrowser);
                if (webBrowser != null)
                {
                    webBrowser.GetType().InvokeMember("Silent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.PutDispProperty, null, webBrowser, new object[] { silent });
                }
            }
        }


        [ComImport, Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IOleServiceProvider
        {
            [PreserveSig]
            int QueryService([In] ref Guid guidService, [In] ref Guid riid, [MarshalAs(UnmanagedType.IDispatch)] out object ppvObject);
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
                    //node.ChildItems.Add(new TaxonomyNodeItem(333, "new", node.Level+1));
                    //tokenSource.Cancel();
                    //tokenSource = new CancellationTokenSource();

                    //BindingOperations.EnableCollectionSynchronization(node.ChildItems, node.ChildItems);
                   
                    //var task =  node.QuerySecondLevelItems(tokenSource.Token);
                    //await task;
                    //var result = task.Result;

                    foreach(var sItem in node.ChildItems)
                    {
                        int ind = 0;
                        //var ids = result[sItem.Id];
                        try
                        {
                            var ids = TaxonomyNodeItem.BaseData.FindChilds(sItem.Id);
                            if (sItem.ChildItems.Count != ids.Count())
                            {
                                var nameMap = new SortedDictionary<string, int>(comparer);
                                BindingOperations.EnableCollectionSynchronization(sItem.ChildItems, sItem.childItems);

                                Task.Factory.StartNew(() => AddChilds(ids, nameMap, node, ind, sItem));

                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }
        }

        private static void AddChilds(IEnumerable<int> ids, SortedDictionary<string, int> nameMap, TaxonomyNodeItem taxonomyNodeItem, int ind,
            TaxonomyNodeItem sItem)
        {
            try
            {
                foreach (var id in ids)
                {
                    try
                    {
                        var name = TaxonomyNodeItem.BaseData.FindName(id);
                        nameMap.Add(name.name, id);
                    }
                    catch (Exception e1)
                    {
                        throw;
                    }
                }

            foreach (var orderedEntry in nameMap)
            {
                var nodeData = TaxonomyNodeItem.BaseData.FindNode(orderedEntry.Value);
                //                                if (!sItem.ChildItems.Any(nodeItem => nodeItem.Id == nodeData.Id))
                var newNode = new TaxonomyNodeItem(nodeData,
                    $"{orderedEntry.Key} - {TaxonomyNodeItem.BaseData.FindClassName(nodeData.ClassId)} L{nodeData.Level} ({nodeData.BrukerCount}/{nodeData.SpeciesCount}/{nodeData.NodesCount})",
                    taxonomyNodeItem.Level + 1);


                ind++;
                try
                {
                    //     if (!sItem.ChildItems.Contains(newNode))
                    {
                        sItem.ChildItems.Add(newNode);
                    }
                    //       else
                    {
                        //          Console.WriteLine("duplicate");
                    }
                }
                catch (Exception e2)
                {
                    throw;
                }
            }
            }
            catch (Exception e3)
            {
                throw;
            }
        }

        private TaxonomyNodeItem itemToSearch;

        private void ButtonX_OnClick(object sender, RoutedEventArgs e)
        {
            itemToSearch = TaxTree.SelectedItem as TaxonomyNodeItem;
        }

        private void ButtonY_OnClick(object sender, RoutedEventArgs e)
        {
            itemToSearch.IsSelected = true;
            itemToSearch.IsExpanded = true;
        }
    }

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

        public object Lock = new object();

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
                    foreach (var secondLevelitem in ChildItems)
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
    //                rootItem.ChildItems.Add(new TaxonomyNodeItem(bac, bac.ToString(), rootItem.Level + 1));
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
        private Dictionary<int, TaxName> names;

        public NcbiNodesParser NcbiNodesParser { get; set; }

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
            NcbiNodesParser = new NcbiNodesParser();
            nodes = NcbiNodesParser.Read(@"C:\Test\NcbiTaxonomy\nodes.dmp");
            NcbiNodesParser.Add(@"C:\Test\NcbiTaxonomy\brukerNodes.dmp",nodes);
            //foreach (var node in nodes)
            //{
            //    if (node.Value.Id == 0)
            //    {
            //        throw new Exception(".-(");
            //    }
            //}
            NcbiNamesParser = new NcbiNamesParser();
            names = NcbiNamesParser.Read(@"C:\Test\NcbiTaxonomy\names.dmp");
            NcbiNamesParser.Add(names, @"C:\Test\NcbiTaxonomy\BrukerNames.dmp");
            //var brukerReader = new BrukerNodesParser(@"C:\Test\NcbiTaxonomy\bruker.dmp");
            //var brukerNodes = brukerReader.Read(NcbiNodesParser.rankMap["no rank"]);
            //brukerReader.MergeBrukerNodesInto(nodes, names, brukerNodes);

            NcbiNodesParser.CalcAllNodesCount(nodes);
            NcbiNodesParser.CalcAllSpeciesCount(nodes);

            
                
            TaxonomyNodeItem.BaseData = this;
            try
            {
                var rootNode = FindNode(131567);
                var bacs = FindChilds(131567);
                var rootItem = new TaxonomyNodeItem(rootNode, $"{FindName(rootNode.Id).name} - {TaxonomyNodeItem.BaseData.FindClassName(rootNode.ClassId)} L{rootNode.Level} ({rootNode.BrukerCount}/{rootNode.SpeciesCount}/{rootNode.NodesCount})", 0);
                BindingOperations.EnableCollectionSynchronization(rootItem.ChildItems, rootItem.childItems);

                foreach (var bac in bacs)
                {
                    // resolve child
                    var node = nodes[bac];
                    var item = new TaxonomyNodeItem(node,
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
    }


    public class TreeViewItemBase : INotifyPropertyChanged
    {
        private bool isSelected;
        public bool IsSelected
        {
            get { return this.isSelected; }
            set
            {
                if(value != this.isSelected)
                {
                    this.isSelected = value;
                    NotifyPropertyChanged("IsSelected");
                }
            }
        }

        private bool isExpanded;
        public bool IsExpanded
        {
            get { return this.isExpanded; }
            set
            {
                if(value != this.isExpanded)
                {
                    this.isExpanded = value;
                    NotifyPropertyChanged("IsExpanded");
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if(this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }

}
