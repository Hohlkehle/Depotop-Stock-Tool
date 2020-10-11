using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Depotop_Stock_Tool
{
    class StockLoader
    {
        public class StockRow
        {
            List<string> m_Line;
            int m_ID;
            string m_Model;
            int m_Order;
            public int ID { get { return m_ID; } set { m_ID = value; } }
            public string Model { get { return m_Model; } set { m_Model = value; } }
            public string SKU { get { return Valid ? Line[0] : String.Empty; } }
            public string Availabity { get { return Valid ? Line[1] : String.Empty; } }
            public int Order { get => m_Order; set => m_Order = value; }
            public bool Contains { get; private set; }
            public StockRow(List<string> line)
            {
                Line = line;
            }

            public bool Valid { get { return (Line != null && Line.Count > 0 && Line[0] != String.Empty); } }

            public List<string> Line { get => m_Line; set => m_Line = value; }

            public bool ContainsSet(DonnesFile.DonnesRow donnesRow)
            {
                Contains = SKU == donnesRow.SKU;
                return Contains;
            }

            public override string ToString()
            {
                return string.Join(", ", ToArray());
            }

            public string[] ToArray()
            {
                return new string[] { ID.ToString(), Model, SKU, Availabity, Order.ToString() };
            }
        }
        //Properties.Settings.Default.StockFilesName;  
        List<string> m_stockFiles;

        List<CsvFile> m_Stocks;
        int m_StockCount = 0;
        List<StockRow> m_Rows;
        string[][] m_RowsArray;
        //List<string> m_InputOptions;
        public int StockCount { get => m_StockCount; }
        internal List<CsvFile> Stocks { get => m_Stocks; set => m_Stocks = value; }
        internal List<StockRow> Rows { get => m_Rows; set => m_Rows = value; }
        public string[][] RowsArray { get => m_RowsArray; set => m_RowsArray = value; }

        //public List<string> InputOptions { get => m_InputOptions; set => m_InputOptions = value; }

        public StockLoader()
        {
            m_stockFiles = new List<string>();
            Stocks = new List<CsvFile>();
            Rows = new List<StockRow>();
        }

        public void LoadFiles()
        {
            DirectoryInfo d = new DirectoryInfo(Properties.Settings.Default.DataPath);
            FileInfo[] Files = d.GetFiles(Properties.Settings.Default.StockFilesName + "*");
            foreach (FileInfo file in Files)
            {
                m_stockFiles.Add(file.Name);
                var stock = new CsvFile(Properties.Settings.Default.DataPath + file.Name);
                //stock.Load();
                Stocks.Add(stock);
                m_StockCount++;
            }
            Console.WriteLine("StockLoader: " + "Loaded " + m_StockCount + " stock files");
        }

        public void ReadAll()
        {
            int order = 0;
            m_Rows = new List<StockRow>();
            foreach (var file in Stocks)
            {
                foreach (var line in file.LazyLoad())
                {
                    string v = ((int)Program.GetDouble(line[1], -1f)).ToString();
                    line[1] = v == "-1" ? line[1] : v;


                    var stockLine = new StockRow(line);
                    stockLine.Order = order;
                    if (!stockLine.Valid)
                        continue;

                    m_Rows.Add(stockLine);

                    Console.Write(String.Format("\rProgress {0:#}%", file.Progress * 100f));
                }
                order++;
            }
            //RowsArray = new string[m_Rows.Count][];

            //for (var i = 0; i < m_Rows.Count; i++)
            //{
            //    RowsArray[i] = m_Rows[i].ToArray();
            //}
        }

        public void Optimize(DonnesFile donnesFile)
        {
            Console.WriteLine(String.Format("Stock data optimisation started"));
            var dump = donnesFile.SkuDump();
            List<string> rem = new List<string>();
            var total = Rows.Count;
            var i = 0f;
            foreach (var row in SkuDump())
            {
                if (!dump.Contains(row))
                {
                    rem.Add(row);
                }
                i++;
                Console.Write(String.Format("\rProgress {0:#}%", (i / (float)total) * 100f));
            }

            Rows.RemoveAll(x => rem.Contains(x.SKU));
            Console.WriteLine(String.Format(" Done {0} -> {1}", total, Rows.Count));
        }

        public IEnumerable<StockRow> InumerateAll()
        {
            foreach (var row in Rows)
            {
                yield return row;
            }
        }

        public void MarkUp(DonnesFile donnesFile)
        {
            Console.WriteLine(String.Format("Markup Stocks started"));
            var total = donnesFile.Rows.Count;
            var i = 0f;
            foreach (var anounce in donnesFile.Rows)
            {
                foreach (var row in Rows)
                {
                    if (row.SKU == anounce.SKU)
                    {
                        row.Model = anounce.Model;
                        row.ID = anounce.ID;
                    }
                }
                i++;
                Console.Write(String.Format("\rProgress {0:#}%", (i / (float)total) * 100f));
            }
            Console.WriteLine(String.Format(" Done "));
        }

        public List<string> SkuDump()
        {
            List<string> dump = new List<string>();
            foreach (var row in Rows)
            {
                dump.Add(row.SKU);
            }
            return dump;
        }

        public List<string> ModelsDump()
        {
            List<string> dump = new List<string>();
            foreach (var row in Rows)
            {
                dump.Add(row.Model);
            }
            return dump;
        }
    }
}
