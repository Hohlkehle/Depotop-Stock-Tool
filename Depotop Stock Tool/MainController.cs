using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Depotop_Stock_Tool
{
    class MainController
    {
        Options m_Options;
        DonnesResolver m_DonnesResolver;
        List<EbayFile> m_Externes;
        internal Options Options { get => m_Options; set => m_Options = value; }
        DonnesFile m_DonnesFile;
        StockLoader m_StockLoader;
        ExterneResolver m_ExterneResolver;

        public MainController(Options options)
        {
            Options = options;

        }

        public void LoadExterne()
        {
            Console.WriteLine(String.Format("Loading externes started"));
            var inputOptions = Options.GetAllOptionsNames();

            m_Externes = new List<EbayFile>();
            var optIndex = 0;
            foreach (var opt in inputOptions)
            {
                if (opt == Options.GetOptionsNames(Options.OptionsNames.Demande) || opt == Options.GetOptionsNames(Options.OptionsNames.Donnes) || opt == Options.GetOptionsNames(Options.OptionsNames.Input))
                    continue;
                var file = Options.GetPath(opt + "_*");
                
                if (!System.IO.File.Exists(file))
                    continue;

                Console.WriteLine(String.Format("EbayFile: {0}", file));

                var ebay = new EbayFile(file);
                ebay.Delimiter = Options.GetDelimiter(opt);
                ebay.OptionsIndex = optIndex;
                ebay.AccName = opt;
                ebay.Load();
                m_Externes.Add(ebay);
                optIndex++;
                Console.WriteLine();
            }
            Console.WriteLine(String.Format("Loaded {0} externe files", optIndex));
        }

        public void ResolveExterne()
        {
            Console.WriteLine(String.Format("Resolve Externe started"));

            m_StockLoader.MarkUp(m_DonnesFile);

            m_ExterneResolver = new ExterneResolver(m_Options);

            List<string[]> results = new List<string[]>();

            foreach (var ebay in m_Externes)
            {
                m_ExterneResolver.SetUp(m_StockLoader, ebay);
                m_ExterneResolver.Optimize();
                results.AddRange(m_ExterneResolver.Resolve());
            }

            m_ExterneResolver.Save(results);

            Console.WriteLine(String.Format(" Resolve Externe complete"));
        }

        public void LoadDonnes()
        {
            Console.WriteLine(String.Format("Loading donnes started"));
            var donnesFile = Options.GetPath(m_Options.DonnesFile);
            m_DonnesFile = new DonnesFile(donnesFile);

            m_DonnesFile.Load();

            Console.WriteLine(String.Format(" Loading complete"));
        }

        public void ResolveDonnes()
        {
            Console.WriteLine(String.Format("Resolve Donnes started"));

            m_DonnesResolver = new DonnesResolver(m_Options);

            m_DonnesResolver.SetUp(m_DonnesFile, m_StockLoader);

            m_DonnesResolver.Optimize();

            m_DonnesResolver.Resolve();

            Console.WriteLine(String.Format(" Resolve Donnes complete"));
        }

        public void LoadStocks()
        {
            Console.WriteLine(String.Format("Loading stock started"));
            m_StockLoader = new StockLoader();

            m_StockLoader.LoadFiles();
            m_StockLoader.ReadAll();
            Console.WriteLine(String.Format(" Loading complete"));
        }




    }
}
