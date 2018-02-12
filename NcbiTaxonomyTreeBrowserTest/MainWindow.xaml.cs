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
            DataContext = this;
            //DriveInfo[] drives = DriveInfo.GetDrives();
            //foreach(DriveInfo driveInfo in drives)
            //    trvStructure.Items.Add(CreateTreeItem(driveInfo));
        }

        private readonly BackgroundWorker worker = new BackgroundWorker();

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            NcbiNodesParser = new NcbiNodesParser(@"C:\Test\NcbiTaxonomy\nodes.dmp");
            nodes = NcbiNodesParser.Read();

            try
            {
                var bacs = FindChilds(2);
                var tvItem = new TreeViewItem { Header = "Bacteria", Tag = 2 };
                foreach (var bac in bacs)
                {
                    tvItem.Items.Add(new TreeViewItem { Header = bac, Tag = bac });
                }
                trvStructure.Items.Add(tvItem);
            }
            catch (Exception ex)
            {

            }            

            worker.DoWork += WorkerDoWork;
            worker.RunWorkerCompleted += WorkerRunWorkerCompleted;
        }

        private void WorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            
        }

        
        private void WorkerDoWork(object sender, DoWorkEventArgs e)
        {
            
        }

        private int[][] nodes;

        private SortedDictionary<int, List<int>> childMap = new SortedDictionary<int, List<int>>();

        private IEnumerable<int> FindChilds(int parent)
        {
            return from p in nodes where p != null && p[1] == parent select p[0];
        }

        public NcbiNodesParser NcbiNodesParser { get; set; }

        private async void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = e.Source as TreeViewItem;
            if(item != null 
               && item.Items.Count > 0)
            {
                System.Diagnostics.Trace.WriteLine($"expand item {item.Header}");
                // 1. get all child nodes
                // neccessary because we cannot access treevieitems i nanother thread
                var task = QueryChilds(item);
                await task;

                System.Diagnostics.Trace.WriteLine($"end expand item {item.Header}");

                // 3. pump childs of child nodes in child node items
                //foreach (var childItem in item.Items)
                //{
                //    if (childItem is TreeViewItem treeViewChildItem)
                //    {
                //        foreach (var tagListValue in tagList[(int)(treeViewChildItem.Tag)])
                //        {
                //            treeViewChildItem.Items.Add(new TreeViewItem {Header = tagListValue, Tag = tagListValue});
                //        }
                //    }
                //}

            }
        }

        private Task QueryChilds(TreeViewItem item)
        {
            System.Diagnostics.Trace.WriteLine($"query childs item {item.Header}");
            var requestList = new Dictionary<int, List<int>>();

            if (item.Items.Count == 1 && item.Items[0] is TreeViewItem && (item.Items[0] as TreeViewItem).Tag == null)
            {
                item.Items.Clear();

                if (childMap.ContainsKey((int) item.Tag))
                {
                    foreach (var childId in childMap[(int)item.Tag])
                    {
                        var newItem = new TreeViewItem {Header = childId, Tag = childId};
                        newItem.Items.Add(new TreeViewItem {Header = "Loading..."});
                        requestList.Add(childId, new List<int>());
                        item.Items.Add(newItem);
                    }
                }
            }
            else
            {
                foreach (var childItem in item.Items)
                {
                    if (childItem is TreeViewItem treeViewChildItem)
                    {

                        if (treeViewChildItem.Tag != null && !childMap.ContainsKey((int) treeViewChildItem.Tag))
                        {
                            requestList.Add((int) treeViewChildItem.Tag, new List<int>());
                            childMap[(int) treeViewChildItem.Tag] = null;
                            treeViewChildItem.Items.Add(new TreeViewItem {Header = "Loading..."});
                        }
                    }
                }
            }

            System.Diagnostics.Trace.WriteLine($"query childs item {item.Header} initiate background");
            string id = item.Header.ToString();

            // 2. query all childs of child nodes async 
            var task = Task.Run(() =>
            {
                System.Diagnostics.Trace.WriteLine($"query childs item {id} start background");
                try
                {
                    foreach (var childItem in requestList)
                    {
                        var bacs = FindChilds((int) childItem.Key);
                        foreach (var bac in bacs)
                        {
                            childItem.Value.Add(bac);
                        }

                        childMap[(int) childItem.Key] = childItem.Value;
                    }
                }
                catch (Exception ex)
                {
                }
                System.Diagnostics.Trace.WriteLine($"query childs item {id} end background");
            });
            System.Diagnostics.Trace.WriteLine($"query childs item {item.Header} end");
            return task;
        }


        private TreeViewItem CreateTreeItem(object o)
        {
            TreeViewItem item = new TreeViewItem();
            item.Header = o.ToString();
            item.Tag = o;
            item.Items.Add("Loading...");
            return item;
        }

        private void TrvStructure_OnCollapsed(object sender, RoutedEventArgs e)
        {
            //TreeViewItem item = e.Source as TreeViewItem;

            //if (item != null
            //    && item.Items.Count > 0)
            //{
            //    item.Items.Clear();
            //}
        }

        private CancellationTokenSource tokenSource = new CancellationTokenSource();

        private async void TaxonomyNodeItem_Expanded(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is TreeViewItem item)
            {
                if (item.Header is TaxonomyNodeItem node)
                {
                    //node.SecondLevelItems.Add(new TaxonomyNodeItem(333, "new", node.Level+1));
                    //tokenSource.Cancel();
                    //tokenSource = new CancellationTokenSource();
                    BindingOperations.EnableCollectionSynchronization(node.SecondLevelItems, node.Lock);
                   var task =  node.QuerySecondLevelItems(tokenSource.Token);
                    await task;
                    var result = task.Result;
                    foreach(var sItem in node.SecondLevelItems)
                    {
                        var ids = result[sItem.Id];
                        foreach (var nId in ids)
                        {
                            sItem.SecondLevelItems.Add(new TaxonomyNodeItem(nId, nId.ToString(), node.Level + 1));
                        }
                    }
                }
            }
        }
    }


    public class TaxonomyNodeItem 
    {
        public static TreeViewData baseData; 

        public string DisplayName { get; set; }
        public int Id { get; set; }

        public int Level { get; set; }

        public object Lock = new object();

        private IEnumerable<int> level2Nodes;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<TaxonomyNodeItem> SecondLevelItems { get; set; }


        public TaxonomyNodeItem(int id, string displayName, int level)
        {
            Id = id;
            DisplayName = displayName;
            Level = level;
            
            SecondLevelItems = new ObservableCollection<TaxonomyNodeItem>();
            //int max = theOneIsDone ? 1000 : 10000;
            //level++;
            //if (level < 4)
            //{
            //    for (int i = 0; i < 4; ++i)
            //    {
            //        SecondLevelItems.Add(new TaxonomyNodeItem(4545, "load...", level));
            //    }
            //}
            //theOneIsDone = true;
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
            var bacs = baseData.FindChilds(Id);
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
        private SortedDictionary<int, int[]> nodes;
        public NcbiNodesParser2 NcbiNodesParser { get; set; }

        public IEnumerable<int> FindChilds(int parent)
        {
            return from p in nodes where p != null && p[1] == parent select p[0];
        }

        public TreeViewData()
        {
            NcbiNodesParser = new NcbiNodesParser2(@"C:\Test\NcbiTaxonomy\nodes.dmp");
            nodes = NcbiNodesParser.Read();
            TaxonomyNodeItem.baseData = this;
            try
            {
                var bacs = FindChilds(2);
                var rootItem = new TaxonomyNodeItem(2, "Bacteria", 0);
                foreach (var bac in bacs)
                {
                    rootItem.SecondLevelItems.Add(new TaxonomyNodeItem(bac, bac.ToString(), rootItem.Level + 1));
                }
                Add(rootItem);
            }
            catch (Exception ex)
            {

            }            
        }
    }

}
