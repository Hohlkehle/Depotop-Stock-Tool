using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Depotop_Stock_Tool
{
    public class DonnesFile : CsvFile
    {
        private List<DonnesRow> m_Rows;

        public List<DonnesRow> Rows { get => m_Rows; set => m_Rows = value; }

        public class DonnesRow
        {
            List<string> m_Line;
            string m_Availabity;
            public int ID { get { return Valid ? int.Parse(Line[0]) : 0; } }
            public string Model { get { return Valid ? Line[4] : String.Empty; } }
            public string SKU { get { return Valid ? Line[5] : String.Empty; } }
            public int Quantity { get { return Valid ? int.Parse(Line[7]) : 0; } }
            public string Availabity { get => m_Availabity; set => m_Availabity = value; }
            public DonnesRow(List<string> line)
            {
                Line = line;
            }

            public bool Valid { get { return (Line != null && Line.Count > 0 && Line[0] != String.Empty && Line[0].All(Char.IsDigit)); } }

            public List<string> Line { get => m_Line; set => m_Line = value; }
        }

        public DonnesFile(string file) : base(file) { }

        public override void Load()
        {
            Rows = new List<DonnesRow>();

            foreach (var line in LazyLoad())
            {
                var donnesLine = new DonnesRow(line);

                if (!donnesLine.Valid)
                    continue;

                Rows.Add(donnesLine);

                Console.Write(String.Format("\rProgress {0:#}%", Progress * 100f));
            }
        }

        public IEnumerable<DonnesRow> InumerateAll()
        {
            foreach(var row in Rows)
            {
                yield return row;
            }
        }

        internal void Optimize(StockLoader stockLoader)
        {
            Console.WriteLine(String.Format("Donnes data optimisation started"));
            var dump = stockLoader.SkuDump();
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

        public List<string> SkuDump()
        {
            List<string> dump = new List<string>();
            foreach(var row in Rows)
            {
                dump.Add(row.SKU);
            }
            return dump;
        }

        public List<string> IDDump()
        {
            List<string> dump = new List<string>();
            foreach (var row in Rows)
            {
                dump.Add(row.ID.ToString());
            }
            return dump;
        }

       
    }
}
