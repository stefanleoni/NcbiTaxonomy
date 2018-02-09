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
            var reader = new NcbiNodesParser(@"C:\Test\NcbiTaxonomy\nodes.dmp");
            var reader2 = new NcbiNodesParser(@"C:\Test\NcbiTaxonomy\names.dmp");

            Console.ReadLine();
            Stopwatch w = new Stopwatch();
            w.Start();

            var nodes = reader.Read();

            w.Stop();

            Console.WriteLine($"Read {nodes.Length} lines.");
           // Console.WriteLine($"Read {lines2} lines.");
            Console.WriteLine($"in {w.Elapsed.Milliseconds} ms");

            Console.ReadLine();
        }
    }

    public class NcbiNodesParser
    {
        public string FileName { get; set; }

        public NcbiNodesParser(string fileName)
        {
            FileName = fileName;
        }

        public int[][] Read()
        {
            int[][] result = new int[2_000_000][];
            int line = 0;
            using (FileStream fs = File.OpenRead(FileName))
            using (BufferedStream bs = new BufferedStream(fs))
            using (StreamReader sr = new StreamReader(bs))
            {
                string s;
                while ((s = sr.ReadLine()) != null)
                {
                    result[line] = ParseLine(s);
                    line++;
                    //we're just testing read speeds    
                }
            }
            return result;
        }

        int[] ParseLine(string s)
        {
            int[] result = new int[3];
            var ind0 = s.IndexOf('|');
            var ind1 = s.IndexOf('|', ind0 + 1);
            var ind2 = s.IndexOf('|', ind1 + 1);
            result[0] = int.Parse(s.Substring(0, ind0 - 1));
            result[1] = int.Parse(s.Substring(ind0 + 2, ind1 - ind0 - 2));
            result[2] = s.Substring(ind1 + 2, ind2 - ind1 - 3).GetHashCode();
            return result;
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
