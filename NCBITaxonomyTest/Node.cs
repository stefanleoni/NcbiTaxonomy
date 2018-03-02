using System.Collections.Generic;

namespace NCBITaxonomyTest
{
    public class Node
    {
        public Node()
        {
            Childs = new List<int>();
            RemainingChildCounts = new List<int>();
            RemainingSpeciesChildCounts = new List<int>();
            RemainingBrukerChildCounts = new List<int>();
        }

        public int Id { get; set; }
        public Node Parent { get; set; }
        public int ClassId { get; set; }

        public bool IsBruker => Id < 0;


        public int SpeciesCount { get; set; }
        public int NodesCount { get; set; }
        public int BrukerCount { get; set; }

        public IList<int> RemainingChildCounts { get; set; }
        public IList<int> RemainingSpeciesChildCounts { get; set; }
        public IList<int> RemainingBrukerChildCounts { get; set; }


        public int Level { get ; set;}

        public IList<int> Childs { get; private set; }

    }

    public class NamedNode
    {
        public int Id { get; set; }

        public string Name { get; set; }

    }
}