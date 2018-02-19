using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace NCBITaxonomyTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //var reader = new NcbiNodesParser(@"C:\Test\NcbiTaxonomy\nodes.dmp");
            var reader = new NcbiNodesParser(@"C:\Test\NcbiTaxonomy\nodes.dmp");
            var reader2 = new NcbiNamesParser(@"C:\Test\NcbiTaxonomy\names.dmp");

            Console.ReadLine();
            Stopwatch w = new Stopwatch();
            w.Start();

            var nodes = reader.Read();
            //var names = reader2.Read();

            w.Stop();
            Console.WriteLine($"read in {w.Elapsed.TotalMilliseconds} ms");

            w.Reset();
            w.Start();


            var count = nodes.Count(pair => pair.Value.Level < 1);
            var leftovers = nodes.Where(pair => pair.Value.Level < 1);

            reader.CalcAllNodesCount(nodes);

            w.Stop();
            Console.WriteLine($"iterate leaves in {w.Elapsed.TotalMilliseconds} ms");

            //////////////////
            
            //////////////////

            long totalMemory = System.GC.GetTotalMemory(false);
            Console.WriteLine($"Read {nodes.Count} lines. Uses {(totalMemory / 1024f) / 1024f} Mb.");
            Console.ReadLine();

        }
    }


    public class NcbiNodeChildFinder
    {
        public SortedDictionary<int, Node> allNodes;

        public SortedDictionary<int, List<int>> childMaps = new SortedDictionary<int, List<int>>();
            
        public int nodesProcessed = 0;


        public void FindAllChilds()
        {
            foreach (var node in allNodes)
            {
                nodesProcessed++;
                if (nodesProcessed % 100 == 0)
                {
                    Console.WriteLine($"processed {nodesProcessed} nodes.");
                }
                FindChilds(node.Value);
            }
        }

        private void FindChilds(Node node)
        {

            var result = allNodes.Values.Where(item => item.Parent == node.Id);
            var findChilds = result as Node[] ?? result.ToArray();
            foreach (var child in findChilds)
            {
                if (!childMaps.ContainsKey(node.Id))
                {
                    childMaps[node.Id] = new List<int>();
                }
                childMaps[node.Id].Add(child.Id);
            }
        }
    }


    public class NcbiNamesParser
    {
        public string FileName { get; set; }

        public NcbiNamesParser(string fileName)
        {
            FileName = fileName;
        }

        public IDictionary<int, TaxName> Read()
        {
            var result = new Dictionary<int, TaxName>(2000000);
            int line = 0;
            using (FileStream fs = File.OpenRead(FileName))
            using (BufferedStream bs = new BufferedStream(fs))
            using (StreamReader sr = new StreamReader(bs))
            {
                string s;
                while ((s = sr.ReadLine()) != null)
                {
                    var lResult = ParseLine(s);
                    if (lResult.Item2.nameClass.Equals("scientific name"))
                    {
                        result.Add(lResult.Item1, lResult.Item2);
                    }
                    line++;
                }
            }
            return result;
        }

        public Tuple<int, TaxName> ParseLine(string s)
        {
            var ind0 = s.IndexOf('|');
            var ind1 = s.IndexOf('|', ind0 + 1);
            var ind2 = s.IndexOf('|', ind1 + 1);
            var ind3 = s.IndexOf('|', ind2 + 1);
            var id = int.Parse(s.Substring(0, ind0 - 1));
            TaxName names = new TaxName();
            names.name  = s.Substring(ind0 + 2, ind1 - ind0 - 3);
            names.uniqueName = s.Substring(ind1 + 2, ind2 - ind1 - 3);
            names.nameClass = s.Substring(ind2 + 2, ind3 - ind2 - 3);
            return new Tuple<int, TaxName>(id, names);
        }
    }

    public class TaxName
    {
        public string name;
        public string uniqueName;
        public string nameClass;
    }


    public class Node
    {
        public Node()
        {
            Childs = new List<int>();
            RemainingChildCounts = new List<int>();
            RemainingSpeciesChildCounts = new List<int>();
        }

        public int Id { get; set; }
        public int Parent { get; set; }
        public int classId { get; set; }

        public int SpeciesCount { get; set; }
        public int NodesCount { get; set; }
        public IList<int> RemainingChildCounts { get; set; }
        public IList<int> RemainingSpeciesChildCounts { get; set; }

        public int Level { get ; set;}

        public IList<int> Childs { get; private set; }

    }

    //Console.ReadLine();
    //Stopwatch w = new Stopwatch();
    //w.Start();
    //int lines = reader.Read();
    //int lines2 = reader2.Read();
    //w.Stop();

    //Console.WriteLine($"Read {lines} lines.");
    //Console.WriteLine($"Read {lines2} lines.");
    //Console.WriteLine($"in {w.Elapsed.Milliseconds} ms");

    //    w.Reset();
    //    w.Start();
    //    var AllLines = new string[1700000]; //only allocate memory here
    //    AllLines = File.ReadAllLines(@"C:\Test\NcbiTaxonomy\nodes.dmp");
    //    var AllLines2 = new string[2700000]; //only allocate memory here
    //    AllLines2 = File.ReadAllLines(@"C:\Test\NcbiTaxonomy\names.dmp");
    //    w.Stop();
    //    Console.WriteLine($"Read {AllLines.Length} lines.");
    //    Console.WriteLine($"Read {AllLines2.Length} lines.");
    //    Console.WriteLine($"in {w.Elapsed.Milliseconds} ms");

    //for (int x = 0; x < 15; x++)
    //{
    //    w.Reset();
    //    w.Start();
    //    for (int i = 0; i < AllLines.Length; i++)
    //    {
    //        //slow   var sa = AllLines[i].Split(new[] { '|' }, 3);
    //        var ind0 = AllLines[i].IndexOf('|');
    //        var ind1 = AllLines[i].IndexOf('|', ind0+1);
    //        var ind2 = AllLines[i].IndexOf('|', ind1+1);
    //        var id = AllLines[i].Substring(0, ind0 - 1);
    //        var parent = AllLines[i].Substring(ind0 + 2, ind1 - ind0 -2);
    //        var rank = AllLines[i].Substring(ind1+2, ind2 - ind1 -3);

    //    }
    //    w.Stop();
    //    Console.WriteLine($"parsed in {w.Elapsed.Milliseconds} ms");
    //}
    //List<int> super = new List<int>();
    //w.Reset();
    //w.Start();
    //for (int i = 0; i < AllLines.Length; i++)
    //{
    //    //slow   var sa = AllLines[i].Split(new[] { '|' }, 3);
    //    var ind0 = AllLines[i].IndexOf('|');
    //    var ind1 = AllLines[i].IndexOf('|', ind0 + 1);
    //    var ind2 = AllLines[i].IndexOf('|', ind1 + 1);
    //    var id = int.Parse(AllLines[i].Substring(0, ind0 - 1));
    //    var parent = int.Parse(AllLines[i].Substring(ind0 + 2, ind1 - ind0 - 2));
    //    var rank = AllLines[i].Substring(ind1 + 2, ind2 - ind1 - 3);
    //    //if(rank[0] == 's' && rank[1] == 'u' && rank[5] == 'f')
    //    //if(parent ==2 )
    //        if (parent == 57723)
    //        {
    //            super.Add(id);
    //    }
    //}
    //w.Stop();
    //Console.WriteLine($"parsed in {w.Elapsed.Milliseconds} ms");

}
