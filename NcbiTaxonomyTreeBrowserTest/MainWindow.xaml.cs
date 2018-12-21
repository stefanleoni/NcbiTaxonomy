using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Navigation;
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


        public static readonly DependencyProperty ShowOnlyBrukerNodesProperty = DependencyProperty.Register(
            "ShowOnlyBrukerNodes", typeof(bool), typeof(MainWindow), new PropertyMetadata(default(bool)));

        public bool ShowOnlyBrukerNodes
        {
            get { return (bool) GetValue(ShowOnlyBrukerNodesProperty); }
            set { SetValue(ShowOnlyBrukerNodesProperty, value); }
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
                    foreach(var sItem in node.ChildItems)
                    {
                        int ind = 0;
                        try
                        {
                            var ids = TaxonomyNodeItem.BaseData.FindChilds(sItem.Id);
                            if (sItem.ChildItems.Count != ids.Count())
                            {
                                var nameMap = new SortedDictionary<string, int>(comparer);
                                BindingOperations.EnableCollectionSynchronization(sItem.ChildItems, sItem.childItems);

                                Task.Factory.StartNew(() => AddChilds(sItem, ids, nameMap, node, ind));

                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }
        }

        private static void AddChilds(TaxonomyNodeItem parentItem, IEnumerable<int> ids, SortedDictionary<string, int> nameMap, TaxonomyNodeItem taxonomyNodeItem, int ind)
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
                var newNode = new TaxonomyNodeItem(parentItem, nodeData,
                    $"{orderedEntry.Key} - {TaxonomyNodeItem.BaseData.FindClassName(nodeData.ClassId)} L{nodeData.Level} ({nodeData.BrukerCount}/{nodeData.SpeciesCount}/{nodeData.NodesCount})",
                    taxonomyNodeItem.Level + 1);

                ind++;
                try
                {
                    //     if (!sItem.ChildItems.Contains(newNode))
                    {
                        parentItem.ChildItems.Add(newNode);
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
            ExpandParent(itemToSearch);
            itemToSearch.IsSelected = true;
            itemToSearch.IsExpanded = true;
        }

        public void ExpandParent(TaxonomyNodeItem item)
        {
            var current = item;
            while(current.ParentItem != null)
            {
                current.ParentItem.IsExpanded = true;
                current = current.ParentItem;
            }
        }

        private void Browser_OnNavigated(object sender, NavigationEventArgs e)
        {
            
        }


        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems?.Count > 0)
            {
                var nodeView = e.AddedItems[0] as ListViewNode;
                foreach (var taxTreeItem in TaxTree.Items)
                {
                    if (taxTreeItem is TaxonomyNodeItem iii)
                    {
                        if (iii.Id == nodeView?.Node.Id)
                        {
                            iii.IsExpanded = true;
                            iii.IsSelected = true;
                            return;
                        }
                        if (Find(iii, nodeView)) return;
                    }
                }
                //Todo now find in tree !!!!
            }
        }

        private static bool Find(TaxonomyNodeItem iii1, ListViewNode nodeView)
        {
            if (iii1 == null) return false;

            foreach (var taxonomyNodeItem in iii1.ChildItems)
            {
                if (taxonomyNodeItem.Id == nodeView?.Node.Id)
                {
                    iii1.IsExpanded = true;
                    iii1.IsSelected = true;
                    return true;
                }
                return Find(taxonomyNodeItem, nodeView);
            }
            return false;
        }
    }
}
