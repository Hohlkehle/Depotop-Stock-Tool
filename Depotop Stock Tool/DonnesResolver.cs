using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Depotop_Stock_Tool
{
    class DonnesResolver
    {
        Options m_Options = new Options();
        StockLoader m_StockLoader;
        DonnesFile m_DonnesFile;
        List<string> m_InputOptions;
        List<string> m_DonnesOptions;
        List<string> m_DemandeOptions;
        internal Options Options { get => m_Options; set => m_Options = value; }

        public DonnesResolver(Options options)
        {
            Options = options;
        }

        public void SetUp(DonnesFile donnesFile, StockLoader stockLoader)
        {
            m_DonnesFile = donnesFile;
            m_StockLoader = stockLoader;

            m_InputOptions = Options.GetInputOptions();
            m_DonnesOptions = Options.GetOptionsValues(Options.OptionsNames.Donnes);
            m_DemandeOptions = Options.GetOptionsValues(Options.OptionsNames.Demande);
        }

        public void Optimize()
        {
            m_DonnesFile.Optimize(m_StockLoader);
            m_StockLoader.Optimize(m_DonnesFile);
        }

        public void Resolve()
        {
            Dictionary<string, string[]> compare;
            compare = new Dictionary<string, string[]>();

            for (var i = 0; i < m_InputOptions.Count; i++)
            {
                if (m_DemandeOptions[i] == "date")
                    m_DemandeOptions[i] = DateTime.Now.ToString("dd.MM.yyyy");
                compare[m_InputOptions[i]] = new string[] { m_DonnesOptions[i], m_DemandeOptions[i] };
            }

            List<string[]>[] results = new List<string[]>[m_StockLoader.StockCount];
            for (var i = 0; i < results.Length; i++)
                results[i] = new List<string[]>();

            //Console.WriteLine(String.Format("Resolving"));
            
            var donnesCount = m_DonnesFile.Rows.Count;

            int aCount = 0;
            foreach (var anounce in m_DonnesFile.InumerateAll())
            {
                var selectedStock = from s in m_StockLoader.Rows
                                    where s.SKU == anounce.SKU
                                    orderby s.Order
                                    select s;

                foreach (var stock in selectedStock)
                {
                    var status = compare[stock.Availabity][0];
                    var demande = compare[stock.Availabity][1];

                    results[stock.Order].Add(new string[] { anounce.ID.ToString(), status, demande });
                }

                aCount++;
                Console.Write(String.Format("\rResolving {0:#}%", ((float)aCount / (float)donnesCount) * 100f));
            }

            //Console.WriteLine(String.Format("Comparing"));
            HashSet<string[]> compareResult = new HashSet<string[]>();
            aCount = 0;
            var total = m_DonnesFile.Rows.Count;
            foreach (var id in m_DonnesFile.IDDump())
            {
                foreach (var stock in results)
                {
                    foreach (var line in stock)
                    {
                        if (line[0] != id)
                            continue;

                        compareResult.Add(line);
                    }
                }

                Console.Write(String.Format("\rComparing {0:#}%", ((float)aCount / (float)total) * 100f));
                aCount++;
            }

            //Console.WriteLine(String.Format("Saving"));
            var csv = new StringBuilder();
            aCount = 0;
            foreach (var res in compareResult)
            {
                var newLine = string.Format("{0}{1}{2}{3}{4}", res[0], Properties.Settings.Default.DefaultCsvDelimiter, res[1], Properties.Settings.Default.DefaultCsvDelimiter, res[2]);
                csv.AppendLine(newLine);

                Console.Write(String.Format("\rSaving {0:#}%", ((float)aCount / (float)compareResult.Count) * 100f));
                aCount++;
            }
            File.WriteAllText(Properties.Settings.Default.OutputPath + "Stock OC " + (DateTime.Now.ToString("dd.MM.yyyy")) + ".csv", csv.ToString());
        }
    }
}
//for (var i = 0; i < m_StockLoader.StockCount; i++)
//{
//    int order = i;
//    int aCount = 0;
//    foreach (var anounce in m_DonnesFile.InumerateAll())
//    {

//        var selectedStock = from s in m_StockLoader.Rows
//                            where s.Order == order && s.SKU == anounce.SKU
//                            orderby s.Order
//                            select s;

//        foreach (var stock in selectedStock)
//        {
//            var status = compare[stock.Availabity][0];
//            var demande = compare[stock.Availabity][1];

//            results[order].Add(new string[] { anounce.ID.ToString(), status, demande });
//        }

//         //for (var r = 0; r < m_StockLoader.RowsArray.Length; r++)
//         //{
//         //    //return new string[] { ID.ToString(), Model, SKU, Availabity, Order.ToString() };
//         //    if(m_StockLoader.RowsArray[r][4] == order.ToString() && anounce.SKU == m_StockLoader.RowsArray[r][2])
//         //    {
//         //        var status = compare[m_StockLoader.RowsArray[r][3]][0];
//         //        var demande = compare[m_StockLoader.RowsArray[r][3]][1];

//         //        results[order].Add(new string[] { anounce.ID.ToString(), status, demande });
//         //    }
//         //}



//        //m_StockLoader.Rows.Where(s => { return s.Order == order && s.SKU == anounce.SKU; });


//        //foreach (var stock in m_StockLoader.InumerateAll())
//        //{
//        //    if (stock.Order == order && stock.SKU == anounce.SKU)
//        //    {
//        //        var status = compare[stock.Availabity][0];
//        //        var demande = compare[stock.Availabity][1];

//        //        results[order].Add(new string[] { anounce.ID.ToString(), status, demande });
//        //    }
//        //}
//        aCount++;
//        Console.Write(String.Format("\rProgress {0:#}%", ((float)aCount / (float)donnesCount) * 100f));

//    }
//}