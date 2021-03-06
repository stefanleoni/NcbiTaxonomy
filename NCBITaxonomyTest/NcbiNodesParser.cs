﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NCBITaxonomyTest
{
    public class NcbiNodesParser
    {

        public NcbiNodesParser()
        {
            ClassNameMap = new SortedDictionary<int, string>();
        }

        private const int IndexId = 0;
        private const int IndexParent = 1;
        private const int IndexClassId = 2;

        const int RootNodeId = 131567;

        public SortedDictionary<int, Node> Read(string fileName)
        {
            SortedDictionary<int, Node> nodes = new SortedDictionary<int, Node>();

            DoRead(fileName, nodes);
            CalcLevels(nodes);
            // invert rankMap
            foreach (var item in rankMap)
            {
                ClassNameMap.Add(item.Value, item.Key);
            }
            return nodes;
        }

        public void Add(string fileName, SortedDictionary<int, Node> nodes)
        {
            DoRead(fileName, nodes);
            CalcLevels(nodes);
        }

        private void DoRead(string fileName, SortedDictionary<int, Node> result)
        {
            using (FileStream fs = File.OpenRead(fileName))
            using (BufferedStream bs = new BufferedStream(fs))
            using (StreamReader sr = new StreamReader(bs))
            {
                string s;
                while ((s = sr.ReadLine()) != null)
                {
                    bool root = false;
                    int[] lineParsed = ParseLine(s);
                    if (lineParsed[IndexId] == 1 || lineParsed[IndexParent] == 1)
                    {
                        //Console.WriteLine("1");
                        //continue;
                        //root = true;
                    }

                    if (lineParsed[IndexId] == 131567)
                    {
                        //Console.WriteLine("Root!");
                        root = true;
                    }

                    Node node;

                    if (!result.ContainsKey(lineParsed[IndexId]))
                    {
                        // current to result
                        node = new Node
                        {
                            Id = lineParsed[IndexId],
                            Parent = new Node {Id = lineParsed[IndexParent]},
                            ClassId = lineParsed[IndexClassId],
                        };
                        result.Add(lineParsed[IndexId], node);
                    }
                    else
                    {
                        // node already contained in list
                        node = result[lineParsed[IndexId]];
                        node.Parent = new Node {Id = lineParsed[IndexParent]};
                        node.ClassId = lineParsed[IndexClassId];
                    }

                    Node pNode = null;
                    //add current to parents childs
                    if (!result.ContainsKey(lineParsed[IndexParent]))
                    {
                        pNode = new Node {Id = lineParsed[IndexParent]};
                        pNode.Childs.Add(lineParsed[IndexId]);
                        //newNode.SpeciesCount++;
                        //newNode.NodesCount++;
                        result.Add(lineParsed[IndexParent], pNode);
                    }
                    else if (lineParsed[IndexParent] > 1)
                    {
                        pNode = result[lineParsed[IndexParent]];
                        pNode.Childs.Add(lineParsed[IndexId]);
                        //pNode.SpeciesCount++;
                        //pNode.NodesCount++;
                    }

                    //Level
                    if (node.Id == RootNodeId)
                    {
                        node.Level = 1;
                    }
                    else if (node.Parent.Id == RootNodeId)
                    {
                        node.Level = 2;
                    }
                    else if (pNode != null)
                    {
                        if (pNode.Level == 2)
                        {
                            node.Level = 3;
                        }

                        if (pNode.Level > 2)
                        {
                            node.Level = pNode.Level + 1;
                        }
                    }
                }
            }
        }


        int maxLevel = 5;
        void CalcLevels(SortedDictionary<int, Node> nodes)
        {
            int missingCount = 0;
            int lastCount = 0;
            do
            {
                lastCount = missingCount;
                var noLevel = nodes.Where(pair => pair.Value.Level < 1);
                var keyValuePairs = noLevel as KeyValuePair<int, Node>[] ?? noLevel.ToArray();
                missingCount = keyValuePairs.Count();
                //Console.WriteLine($"level miss {missingCount }");
                //var list2 = new List<int>();
                Parallel.ForEach(keyValuePairs, pair =>
                {
                    var m = pair.Value;
                    int parentId = m.Parent?.Id ?? RootNodeId;
                    var p = nodes[parentId];

                    if (m.Level == 0 && p.Level > 1)
                    {
                        m.Level = p.Level + 1;
                    }
                   // else
                    //{
                        //var g = nodes[parentId];
                        //if (m.Level == 0 && p.Level == 0
                        //                 && p.Level > 0)
                        //{
                        //    p.Level = p.Level + 1;
                        //    m.Level = p.Level + 1;
                        //}
                    //}
                    maxLevel = maxLevel < m.Level ? m.Level : maxLevel;
                });
            } while (missingCount > 0 && lastCount != missingCount);
        }

        public void CalcAllNodesCount(SortedDictionary<int, Node> nodes)
        {
            for(int i = maxLevel; i > 0; i--)
            {
                CalcNodesCount(nodes, i);
            } 

        }

        public void CalcNodesCount(SortedDictionary<int, Node> nodes, int level)
        {
            var parents = (from n in nodes where n.Value.Level == level select n.Value);

            //Console.WriteLine($"nodes parents = {parents.Count()}");

            foreach (var node in parents)
            {
                node.NodesCount += node.Childs.Count;
                foreach(var x in node.RemainingChildCounts)
                {
                    node.NodesCount += x;
                }
                var parent = nodes[node.Parent.Id];
                parent.RemainingChildCounts.Add(node.NodesCount);
            }

            
        }

        public void CalcAllSpeciesCount(SortedDictionary<int, Node> nodes)
        {
            if(ClassNameMap.ContainsKey("species".GetHashCode()))
            {
                var classId = "species".GetHashCode();
                for(int i = maxLevel; i > 0; i--)
                {
                    CalcSpeciesCount(nodes, i, classId);
                } 
            }
        }

        public void CalcSpeciesCount(SortedDictionary<int, Node> nodes, int level, int classId)
        {
            var parents = (from n in nodes where n.Value.Level == level select n.Value);


            //Console.WriteLine($"species parents = {parents.Count()}");

            foreach (var node in parents)
            {
                foreach (var nodeChild in node.Childs)
                {
                    var childNode = nodes[nodeChild];
                    if (childNode.ClassId == classId)
                    {
                        node.SpeciesCount++;
                    }

                    if (childNode.IsBruker)
                    {
                        node.BrukerCount++;
                    }
                }
                foreach (var x in node.RemainingSpeciesChildCounts)
                {
                    node.SpeciesCount += x;
                }
                foreach (var x in node.RemainingBrukerChildCounts)
                {
                    node.BrukerCount += x;
                }

                if (node.Parent.Id == 131567)
                {
                    Console.WriteLine("BcN");
                }
                var parent = nodes[node.Parent.Id];
                parent.RemainingSpeciesChildCounts.Add(node.SpeciesCount);
                parent.RemainingBrukerChildCounts.Add(node.BrukerCount);
            }
        }

        public SortedDictionary<int, string> ClassNameMap { get; private set; }

        //public List<int> grandParents = new List<int>();

        public Dictionary<string, int> rankMap = new Dictionary<string, int>();

        int[] ParseLine(string s)
        {
            int[] result = new int[3];
            var ind0 = s.IndexOf('|');
            var ind1 = s.IndexOf('|', ind0 + 1);
            var ind2 = s.IndexOf('|', ind1 + 1);
            result[0] = int.Parse(s.Substring(0, ind0 - 1));
            result[1] = int.Parse(s.Substring(ind0 + 2, ind1 - ind0 - 3));
            var rank = s.Substring(ind1 + 2, ind2 - ind1 - 3);
            if(!rankMap.ContainsKey(rank))
            {
                rankMap.Add(rank, rank.GetHashCode());
            }
            result[2] = rank.GetHashCode();
            return result;
        }
    }
}