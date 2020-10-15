using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Depotop_Stock_Tool
{
    class Program
    {
        static void Main(string[] args)
        {
            Options m_Options = new Options();
            //DbController m_DbController = new DbController();

            MainController m_MainController = new MainController(m_Options);

            m_MainController.LoadDonnes();
            m_MainController.LoadStocks();
            m_MainController.ResolveDonnes();
            m_MainController.LoadExterne();
            m_MainController.ResolveExterne();

            

            return;

            

            DirectoryInfo d = new DirectoryInfo(Properties.Settings.Default.DataPath);
            FileInfo[] Files = d.GetFiles(m_Options.DepotopUkFile);
            string str = "";
            foreach (FileInfo file in Files)
            {
                str = str + ", " + file.Name;
            }

            Console.WriteLine(str);
            Console.ReadKey();
            return ;

            var donnes = new CsvFile(m_Options.DonnesFile);
            donnes.Load();

            StockLoader m_StockLoader = new StockLoader();

            m_StockLoader.LoadFiles();

            var delimiter = m_Options.GetDelimiter(Options.OptionsNames.DepotopUk);
            

            var depotopFr = m_Options.GetOptionsNames(Options.OptionsNames.DepotopDe);

            var inputs = m_Options.GetOptionsValues(Options.OptionsNames.Input);

            var optionsData = m_Options.GetOptionsData(Options.OptionsNames.DepotopDe);

            for (var i = 0; i < inputs.Count; i++)
            {
                var key = inputs[i];

                Console.WriteLine(inputs[i] + " - " + optionsData[inputs[i]]);
            }
            
            

            Console.WriteLine(depotopFr);
            Console.ReadKey();
        }

        public static float GetDouble(string value, float defaultValue)
        {
            float result;

            //Try parsing in the current culture
            if (!float.TryParse(value, System.Globalization.NumberStyles.Any, CultureInfo.CurrentCulture, out result) &&
                //Then try in US english
                !float.TryParse(value, System.Globalization.NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out result) &&
                //Then in neutral language
                !float.TryParse(value, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out result))
            {
                result = defaultValue;
            }

            return result;
        }
    }
}
