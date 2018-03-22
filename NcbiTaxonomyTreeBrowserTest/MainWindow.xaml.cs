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
using System.Windows.Navigation;

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

        private void Browser_OnNavigated(object sender, NavigationEventArgs e)
        {
            
        }
    }
}
