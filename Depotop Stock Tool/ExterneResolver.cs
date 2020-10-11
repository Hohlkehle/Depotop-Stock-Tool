using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Depotop_Stock_Tool
{
    public class ExterneResolver
    {
        Options m_Options = new Options();
        StockLoader m_StockLoader;
        EbayFile m_EbayFile;
        List<string> m_InputOptions;
        List<string> m_EbayOptions;
        public Options Options { get => m_Options; set => m_Options = value; }
        public EbayFile EbayFile
        {
            get => m_EbayFile; set
            {
                m_EbayFile = value;
                Console.WriteLine(String.Format("ExterneResolver New EbayFile {0}", m_EbayFile.File));
            }
        }

        public ExterneResolver(Options options)
        {
            Options = options;
        }

        internal void SetUp(StockLoader stockLoader, EbayFile ebay)
        {
            if (m_StockLoader == null)
                m_StockLoader = stockLoader;

            EbayFile = ebay;

            m_InputOptions = Options.GetInputOptions();
            m_EbayOptions = Options.GetOptionsValues(EbayFile.AccName);
        }

        internal void Optimize()
        {
            EbayFile.Optimize(m_StockLoader);
        }

        internal List<string[]> Resolve()
        {
            Dictionary<string, string> compare = new Dictionary<string, string>();

            for (var i = 0; i < m_InputOptions.Count; i++)
            {
                compare[m_InputOptions[i]] = m_EbayOptions[i];
            }

            List<string[]>[] results = new List<string[]>[m_StockLoader.StockCount];
            for (var i = 0; i < results.Length; i++)
                results[i] = new List<string[]>();

           // Console.WriteLine(String.Format("Resolving"));
            var donnesCount = EbayFile.Rows.Count;
            int aCount = 0;
            foreach (var anounce in EbayFile.InumerateAll())
            {
                var selectedStock = from s in m_StockLoader.Rows
                                    where s.Model == anounce.Model
                                    orderby s.Order
                                    select s;

                foreach (var stock in selectedStock)
                {
                    var status = compare[stock.Availabity];
                    if((anounce.Quantity == 0 && status != "0") || anounce.Quantity != 0 && status == "0")
                        results[stock.Order].Add(new string[] { EbayFile.AccName, anounce.ID.ToString(), status });
                }

                aCount++;
                Console.Write(String.Format("\rResolving {0:#}%", ((float)aCount / (float)donnesCount) * 100f));
            }

            //Console.WriteLine(String.Format("Comparing"));

            HashSet<string[]> compareResult = new HashSet<string[]>();
            aCount = 0;
            var total = EbayFile.Rows.Count;
            foreach (var id in EbayFile.IDDump())
            {
                foreach (var stock in results)
                {
                    foreach (var line in stock)
                    {
                        if (line[1] != id)
                            continue;

                        compareResult.Add(line);
                    }
                }

                Console.Write(String.Format("\rComparing {0:#}%", ((float)aCount / (float)total) * 100f));
                aCount++;
            }

            return compareResult.ToList();
        }

        internal void Save(List<string[]> results)
        {
            Console.WriteLine(String.Format("Saving"));
            var csv = new StringBuilder();
            var aCount = 0f;
            foreach (var res in results)
            {
                var newLine = string.Format("{0}{1}{2}{3}{4}", res[0], Properties.Settings.Default.DefaultCsvDelimiter, res[1], Properties.Settings.Default.DefaultCsvDelimiter, res[2]);
                csv.AppendLine(newLine);

                Console.Write(String.Format("\rSaving {0:#}%", (aCount / (float)results.Count) * 100f));
                aCount++;
            }
            File.WriteAllText(Properties.Settings.Default.OutputPath + "Stock Ebay " + (DateTime.Now.ToString("dd.MM.yyyy")) + ".csv", csv.ToString());
        }
    }
}
