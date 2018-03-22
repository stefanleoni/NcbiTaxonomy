using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCBITaxonomyTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Press key to start...");
            //var reader = new NcbiNodesParser(@"C:\Test\NcbiTaxonomy\nodes.dmp");
            var reader = new NcbiNodesParser();
            var namesReader = new NcbiNamesParser();
            var reader3 = new BrukerNodesParser(@"C:\Test\NcbiTaxonomy\bruker.dmp");

           // Console.ReadLine();
            Stopwatch w = new Stopwatch();
            Stopwatch w2 = new Stopwatch();
            Stopwatch wAll = new Stopwatch();
            w.Start();
            w2.Start();
            wAll.Start();
/* read parallel */
            var nodeTask = new Task<SortedDictionary<int, Node>>(() => reader.Read(@"C:\Test\NcbiTaxonomy\nodes.dmp"));
            nodeTask.Start();
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"nodes read in {w.Elapsed.TotalMilliseconds} ms");
            Console.ForegroundColor = ConsoleColor.Gray;
            w.Restart();
            var namesTask = new Task<Dictionary<int, TaxName>>(() => namesReader.Read(@"C:\Test\NcbiTaxonomy\names.dmp"));
            namesTask.Start();
            w.Stop();
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"names read in {w.Elapsed.TotalMilliseconds} ms");
            Console.ForegroundColor = ConsoleColor.Gray;

            Task.WaitAll(new Task[] {nodeTask, namesTask});
            var nodes = nodeTask.Result;
            var names = namesTask.Result;
   
/* read seq
            var nodes = reader.Read(@"C:\Test\NcbiTaxonomy\nodes.dmp");
            w.Stop();
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"nodes read in {w.Elapsed.TotalMilliseconds} ms");
            Console.ForegroundColor = ConsoleColor.Gray;
            w.Restart();
            var names = namesReader.Read(@"C:\Test\NcbiTaxonomy\names.dmp");
            w.Stop();
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"names read in {w.Elapsed.TotalMilliseconds} ms");
            Console.ForegroundColor = ConsoleColor.Gray;
            w.Restart();
            //var bruker = reader3.Read(reader.rankMap["no rank"]);
            reader.Add(@"C:\Test\NcbiTaxonomy\brukerNodes.dmp", nodes);
            namesReader.Add(names, @"C:\Test\NcbiTaxonomy\brukerNames.dmp");
            w.Stop();
*/

            w2.Stop();
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"names names and nodes in {w2.Elapsed.TotalMilliseconds} ms");
            Console.ForegroundColor = ConsoleColor.Gray;

    
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"bruker read in {w.Elapsed.TotalMilliseconds} ms");
            Console.ForegroundColor = ConsoleColor.Gray;
            w.Restart();

            w.Stop();
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"set bruker nodes {w.Elapsed.TotalMilliseconds} ms");
            Console.ForegroundColor = ConsoleColor.Gray;


            w.Restart();
            w2.Restart();
/* calc parallel */
            var calcNodesTask = new Task(() => reader.CalcAllNodesCount(nodes));
            calcNodesTask.Start();
            w.Stop();
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"calculated nodes in {w.Elapsed.TotalMilliseconds} ms");
            Console.ForegroundColor = ConsoleColor.Gray;

            w.Restart();
            var calcSpeciesTask = new Task(() => reader.CalcAllSpeciesCount(nodes));
            calcSpeciesTask.Start();
            w.Stop();
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"calculated species count in {w.Elapsed.TotalMilliseconds} ms");
            Console.ForegroundColor = ConsoleColor.Gray;
            Task.WaitAll(new Task[] {calcNodesTask, calcSpeciesTask});

/* calc seq           
            reader.CalcAllNodesCount(nodes);

            w.Stop();
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"calculated nodes in {w.Elapsed.TotalMilliseconds} ms");
            Console.ForegroundColor = ConsoleColor.Gray;

            w.Restart();
            reader.CalcAllSpeciesCount(nodes);
            w.Stop();
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"calculated species count in {w.Elapsed.TotalMilliseconds} ms");
            Console.ForegroundColor = ConsoleColor.Gray;
*/

            w2.Stop();
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"calculated all in {w2.Elapsed.TotalMilliseconds} ms");
            Console.ForegroundColor = ConsoleColor.Gray;

            //////////////////
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            wAll.Stop();
            Console.WriteLine();
            Console.WriteLine($"overall in {wAll.Elapsed.TotalMilliseconds} ms");
            
            //////////////////

            long totalMemory = System.GC.GetTotalMemory(false);
            Console.WriteLine($"Read {nodes.Count} lines using {(totalMemory / 1024f) / 1024f} Mb.");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.ReadLine();

        }
    }

    public class BrukerNodesParser
    {
        public string FileName { get; set; }

        public BrukerNodesParser(string fileName)
        {
            FileName = fileName;
            //ClassNameMap = new SortedDictionary<int, string>();
        }


        public List<NamedNode> ReadNodes()
        {
            var result = new List<NamedNode>();

            using (FileStream fs = File.OpenRead(FileName))
            using (BufferedStream bs = new BufferedStream(fs))
            using (StreamReader sr = new StreamReader(bs))
            {
                string s;
                while ((s = sr.ReadLine()) != null)
                {
                    string[] lineParsed = ParseLine(s);
                    int node = int.Parse(lineParsed[0]);
                    result.Add(new NamedNode { Id =  node, Name = lineParsed[1]});
                }
            }
            return result;
        }

        private const int IndexId = 0;
        private const int IndexParent = 1;
        private const int IndexName = 2;

        public SortedDictionary<int, Node> Read(int defaultClassId)
        {
            SortedDictionary<int, Node> result = new SortedDictionary<int, Node>();

            using (FileStream fs = File.OpenRead(FileName))
            using (BufferedStream bs = new BufferedStream(fs))
            using (StreamReader sr = new StreamReader(bs))
            {
                string s;
                while ((s = sr.ReadLine()) != null)
                {
                    string[] lineParsed = ParseLine(s);
                    int nodeId = int.Parse(lineParsed[IndexId]);
                    int parentNodeId = int.Parse(lineParsed[IndexParent]);

                    Node node;
                    if (!result.ContainsKey(nodeId))
                    {   // current to result
                        node = new Node
                        {
                            Id = nodeId,
                            Parent = new Node { Id =  parentNodeId},
                            //IsBruker = true,
                            ClassId = defaultClassId
                        };
                        result.Add(nodeId, node);
                    }
                    else
                    {   // node already contained in list
                        node = result[nodeId];
                        node.Parent = new Node { Id = parentNodeId };
                        node.ClassId = defaultClassId;
                    }

                }
            }

            return result;
        }

        string[] ParseLine(string s)
        {
            string[] result = new string[3];
            var ind0 = s.IndexOf('|');
            var ind1 = s.IndexOf('|', ind0 + 1);
            result[0] = s.Substring(0, ind0);
            result[1] = s.Substring(ind0 + 2, ind1 - ind0 - 3);
            result[2] = s.Substring(ind0 + 1);
            return result;
        }

        public void MergeBrukerNodesInto(SortedDictionary<int, Node> nodes, IDictionary<int, TaxName> names,
            SortedDictionary<int, Node> bruker)
        {
            int countAdd = 0;
            foreach (var brukerNode in bruker)
            {
                if (nodes.ContainsKey(brukerNode.Key))
                {
                    var node = nodes[brukerNode.Key];
                    //node.IsBruker = true;
                }
                else
                {
                    //parent
                    if (nodes.ContainsKey(brukerNode.Value.Parent.Id))
                    {
                        var parent = nodes[brukerNode.Value.Parent.Id];
                        brukerNode.Value.Parent = parent;
                        parent.Childs.Add(brukerNode.Key);
                    }

                    nodes.Add(brukerNode.Key, brukerNode.Value);
                    names.Add(brukerNode.Key, new TaxName{name = $"xx{countAdd}", uniqueName = $"xx{countAdd}", nameClass = "CB"});
                   // Console.WriteLine($"Node {brukerNode.Key} not contained in NCBI!");
                    countAdd++;
                }
            }
            Console.WriteLine($"Added {countAdd} bruker nodes!");
//splitt bruker in nodes and names 
        }

        public void FindAllByName(List<NamedNode> allBrukerNodes, string namesFilename)
        {
           // var result = new Dictionary<int, TaxName>(2000000);
            int line = 0;
            using (FileStream fs = File.OpenRead(namesFilename))
            using (BufferedStream bs = new BufferedStream(fs))
            using (StreamReader sr = new StreamReader(bs))
            {
                string s;
                while ((s = sr.ReadLine()) != null)
                {
                    foreach (var brukerNode in allBrukerNodes)
                    {
                        if(s.Contains(brukerNode.Name))
                        {
                            Console.WriteLine(">>Found ");
                        }
                    }
                    //var lResult = ParseLine(s);
                    //if (lResult.Item2.nameClass.Equals("scientific name"))
                    //{
                    //    result.Add(lResult.Item1, lResult.Item2);
                    //}
                    line++;
                }
            }
           // return result;
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

            var result = allNodes.Values.Where(item => item.Parent.Id == node.Id);
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
