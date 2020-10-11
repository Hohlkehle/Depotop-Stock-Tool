using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Depotop_Stock_Tool
{
    public class CsvFile
    {
        private string m_Delimiter = Properties.Settings.Default.DefaultCsvDelimiter;

        Dictionary<int, List<string>> data;
        protected string m_file;

        public string File { get => m_file; }
        public virtual string Delimiter { get => m_Delimiter; set => m_Delimiter = value; }
        public Dictionary<int, List<string>> Data { get => data; }

        public bool Initialized { get { return (data != null && data.Count > 0); } }

        public float Progress { get; private set; }

        public CsvFile(string file)
        {
            m_file = file;
            data = new Dictionary<int, List<string>>();
        }

        public List<string> this[int index] { get { return (data != null && data.Count > 0) ? data[index] : new List<string>(); } }


        public virtual void Load()
        {
            int i = 0;
            try
            {
                using (var reader = new StreamReader(m_file))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var val = new List<string>(line.Split(new string[] { Delimiter }, StringSplitOptions.None));
                        data.Add(i, val.Select(s => s.Replace("\"", String.Empty)).ToList());
                        i++;
                    }
                }
            }
            catch (System.IO.IOException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
       
        public IEnumerable<List<string>> LazyLoad()
        {
            int i = 0;
            int total = TotalLines(m_file);
            using (var reader = new StreamReader(m_file))
            {

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var val = new List<string>(line.Split(new string[] { Delimiter }, StringSplitOptions.None));
                    val = val.Select(s => s.Replace("\"", String.Empty)).ToList();
                    data.Add(i, val);
                    i++;
                    Progress = (float)i / (float)total;
                    yield return val;
                }
            }
        }

        int TotalLines(string filePath)
        {
            using (StreamReader r = new StreamReader(filePath))
            {
                int i = 0;
                while (r.ReadLine() != null) { i++; }
                return i;
            }
        }
    }
}
