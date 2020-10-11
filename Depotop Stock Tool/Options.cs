using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Depotop_Stock_Tool
{
    public class Options
    {
        public enum OptionsNames
        {
            Input = 0, Donnes = 1, Demande = 2, DepotopFr = 3, Depotop = 4, LicorneGroup = 5, DepotopDe = 6, DepotopIt = 7, DepotopUk = 8
        }

        private CsvFile m_CsvFile;

        public string DonnesFile { get { return m_CsvFile.Data[0][(int)OptionsNames.Donnes] + "*"; } }
        public string DepotopFrFile { get { return m_CsvFile.Data[0][(int)OptionsNames.DepotopFr] + "_*"; } }
        public string DepotopFile { get { return m_CsvFile.Data[0][(int)OptionsNames.Depotop] + "_*"; } }
        public string LicorneGroupFile { get { return m_CsvFile.Data[0][(int)OptionsNames.LicorneGroup] + "_*"; } }
        public string DepotopDeFile { get { return m_CsvFile.Data[0][(int)OptionsNames.DepotopDe] + "_*"; } }
        public string DepotopItFile { get { return m_CsvFile.Data[0][(int)OptionsNames.DepotopIt] + "_*"; } }
        public string DepotopUkFile { get { return m_CsvFile.Data[0][(int)OptionsNames.DepotopUk] + "_*"; } }

        public Options()
        {
            m_CsvFile = new CsvFile(Properties.Settings.Default.OptionsFile);
            m_CsvFile.Load();

            Initialize();
        }

        private void Initialize()
        {

        }

        public string GetDelimiter(OptionsNames optionsNames)
        {
            if (optionsNames == OptionsNames.DepotopUk)
                return ",";
            return ";";
        }

        public string GetDelimiter(string fileName)
        {
            if (fileName.Contains("-uk"))
                return ",";
            return ";";
        }

        public Dictionary<string, string> GetOptionsData(OptionsNames optionsNames)
        {
            if (!m_CsvFile.Initialized)
                return new Dictionary<string, string>();

            Dictionary<string, string> data = new Dictionary<string, string>();
            foreach (KeyValuePair<int, List<string>> kvp in m_CsvFile.Data)
            {
                if (kvp.Key == 0)
                    continue;

                data.Add(kvp.Value[(int)OptionsNames.Input], kvp.Value[(int)optionsNames]);
            }

            return data;
        }

        public List<string> GetOptionsValues(OptionsNames optionsNames)
        {
            if (!m_CsvFile.Initialized)
                return new List<string>();

            int index = (int)optionsNames;
            List<string> opt = new List<string>();
            foreach (KeyValuePair<int, List<string>> kvp in m_CsvFile.Data)
            {
                if (kvp.Key == 0)
                    continue;

                opt.Add(kvp.Value[index]);
            }

            return opt;
        }

        public List<string> GetOptionsValues(string ColName)
        {
            var id = 0;
            var opt = GetAllOptionsNames();
            for(var i = 0; i < opt.Length; i++)
            {
                if(opt[i] == ColName)
                {
                    id = i;
                    break;
                }
            }

            return GetOptionsValues((OptionsNames)id);
        }

        public List<string> GetInputOptions()
        {
            int iVal = 0;
            var m_InputOptions = GetOptionsValues(Options.OptionsNames.Input);
            for (var i = 0; i < m_InputOptions.Count; i++)
            {
                if (int.TryParse(m_InputOptions[i], out iVal))
                {
                    m_InputOptions[i] = iVal.ToString();
                }
            }
            return m_InputOptions;
        }

        public string GetOptionsNames(OptionsNames optionsNames)
        {
            if (!m_CsvFile.Initialized)
                return "";

            return m_CsvFile[0][(int)optionsNames];
        }

        public string[] GetAllOptionsNames()
        {
            if (!m_CsvFile.Initialized)
                return new string[] { };

            return m_CsvFile[0].ToArray();
        }

        public static string GetPath(string sentence)
        {
            DirectoryInfo d = new DirectoryInfo(Properties.Settings.Default.DataPath);
            FileInfo[] Files = d.GetFiles(sentence);
            string str = "";
            foreach (FileInfo file in Files)
            {
                str = Properties.Settings.Default.DataPath + file.Name;
            }
            return str;
        }
    }
}
