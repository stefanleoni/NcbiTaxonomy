using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace NCBITaxonomyTest
{
    public class NcbiNamesParser
    {

        public NcbiNamesParser()
        {
        }

        public Dictionary<int, TaxName> Read(string fileName)
        {
            var result = new Dictionary<int, TaxName>(2000000);
           
            DoRead(fileName, result);
            return result;
        }

        private void DoRead(string fileName, Dictionary<int, TaxName> result)
        {
            using (FileStream fs = File.OpenRead(fileName))
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

                    
                }
            }
        }

        public void Add(Dictionary<int, TaxName> names, string fileName)
        {
            DoRead(fileName, names);
        }

        /*       public ArrayList ReadNames()
               {
                   var result = new ArrayList(2000000);
                   int line = 0;
                   using (FileStream fs = File.OpenRead(FileName))
                   using (BufferedStream bs = new BufferedStream(fs))
                   using (StreamReader sr = new StreamReader(bs))
                   {
                       string s;
                       while ((s = sr.ReadLine()) != null)
                       {
                           result.Add(s);
                       }
                   }
                   return result;
               }
       */
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
}