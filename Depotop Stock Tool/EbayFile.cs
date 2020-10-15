using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Depotop_Stock_Tool
{
  public  class EbayFile : CsvFile
    {
        private List<EbayRow> m_Rows;
        private int m_OptionsIndex = 0;
        private string m_AccName;
        public List<EbayRow> Rows { get => m_Rows; set => m_Rows = value; }
        public int OptionsIndex { get => m_OptionsIndex; set => m_OptionsIndex = value; }
        public string AccName { get => m_AccName; set => m_AccName = value; }

        public class EbayRow
        {
            List<string> m_Line;
            string[] m_Availabity = null;
            string m_SKU;
            public long ID { get { return Valid ? long.Parse(Line[0]) : 0; } }
            public string Model { get { return Valid ? Line[1] : String.Empty; } }
            public string SKU { get => m_SKU; set => m_SKU = value; }
            public int Quantity { get { return Valid ? int.Parse(Line[5]) : 0; } }
            public string[] Availabity { get => m_Availabity; set => m_Availabity = value; }
            public EbayRow(List<string> line)
            {
                Line = line;
            }

            public bool Valid { get { return (Line != null && Line.Count > 0 && Line[0] != String.Empty && Line[0].All(Char.IsDigit)); } }

            public List<string> Line { get => m_Line; set => m_Line = value; }
        }

        public EbayFile(string file) : base(file) 
        { 

        }

        public override void Load()
        {
            Rows = new List<EbayRow>();

            foreach (var line in LazyLoad())
            {
                var donnesLine = new EbayRow(line);

                if (!donnesLine.Valid)
                    continue;

                Rows.Add(donnesLine);

                Console.Write(String.Format("\rProgress {0:#}%", Progress * 100f));
            }
        }

        public IEnumerable<EbayRow> InumerateAll()
        {
            foreach (var row in Rows)
            {
                yield return row;
            }
        }

        internal void Optimize(StockLoader stockLoader)
        {
            Console.WriteLine(String.Format("Ebay data optimisation started"));
            var dump = stockLoader.ModelsDump();
            List<string> rem = new List<string>();
            var total = Rows.Count;
            var i = 0f;
            foreach (var row in ModelsDump())
            {
                if (!dump.Contains(row))
                {
                    rem.Add(row);
                }
                i++;
                Console.Write(String.Format("\rProgress {0:#}%", (i / (float)total) * 100f));
            }

            Rows.RemoveAll(x => rem.Contains(x.Model));
            Console.WriteLine(String.Format(" Done {0} -> {1}", total, Rows.Count));
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

        public List<long> IDDump()
        {
            List<long> dump = new List<long>();
            foreach (var row in Rows)
            {
                dump.Add(row.ID);
            }
            return dump;
        }
    }
}
