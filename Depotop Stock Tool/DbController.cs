using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Depotop_Stock_Tool
{
    class DbController
    {
        public const string SQLITE_LIB_NAME = "System.Data.SQLite.dll";
        private bool ISRESOLVED = false;
        string CREATE_TABLE_DONNES = "CREATE TABLE Donnes (id INTEGER, model VARCHAR (32), sku VARCHAR (48), quantity INTEGER (4), stock_1 VARCHAR (16), stock_2 VARCHAR (16), stock_3 VARCHAR (16));";
        string CREATE_TABLE_STOCK = "CREATE TABLE Stock (id INTEGER PRIMARY KEY AUTOINCREMENT, stock VARCHAR (16), order INTEGER (1));";

        private string m_DbFile = Properties.Settings.Default.StockDatabase;

        public string DbFile { get => m_DbFile; set => m_DbFile = value; }

        public DbController()
        {
            ResolveSqliteAssembly();

            if (!File.Exists(DbFile))
            {
                MakeDatabase();
            }

           /* DbProviderFactory fact = DbProviderFactories.GetFactory("System.Data.SQLite");
            using (DbConnection cnn = fact.CreateConnection())
            {
                cnn.ConnectionString = "Data Source=" + DbFile;
                cnn.Open();
            }*/
        }

        private void MakeDatabase()
        {
            try
            {
                using (var connection = new SQLiteConnection("Data Source=" + DbFile + ";Version=3"))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(CREATE_TABLE_DONNES, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                    using (var command = new SQLiteCommand(CREATE_TABLE_STOCK, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception sQLiteException)
            {
                Console.WriteLine(String.Format("sQLiteException {0}", sQLiteException.Message), "Error Initializing database");
            }

        }

        private void ResolveSqliteAssembly()
        {
            if (ISRESOLVED)
                return;

            AppDomain currentDomain = AppDomain.CurrentDomain;

            currentDomain.AssemblyResolve += new ResolveEventHandler((object sender, ResolveEventArgs args) =>
            {

                if (!args.Name.StartsWith(Properties.Settings.Default.SqlitePackage))
                    return args.RequestingAssembly;

                var sqliteLibDirectory = "";
                var currentAssembly = System.Reflection.Assembly.GetEntryAssembly();
                var currentDirectory = new System.IO.FileInfo(currentAssembly.Location).DirectoryName;

                if (IntPtr.Size == 4)
                    sqliteLibDirectory = System.IO.Path.Combine(currentDirectory, @"sqlite\x86", SQLITE_LIB_NAME);
                else
                    sqliteLibDirectory = System.IO.Path.Combine(currentDirectory, @"sqlite\x64", SQLITE_LIB_NAME);

                System.Reflection.Assembly assembly = System.Reflection.Assembly.LoadFrom(sqliteLibDirectory);
                Console.WriteLine(String.Format("Resolved Sqlite Assembly for {0} {1}", Properties.Settings.Default.SqlitePackage, sqliteLibDirectory));
                return assembly;
            });

            ISRESOLVED = true;
        }

        public void LoadDonnes(DonnesFile donnes)
        {
            using (var connection = new SQLiteConnection("Data Source=" + DbFile + ";Version=3"))
            using (var command = new SQLiteCommand("DELETE FROM Donnes", connection))
            {
                connection.Open();
                command.ExecuteNonQuery();

                foreach (var line in donnes.LazyLoad())
                {
                    var donnesLine = new DonnesFile.DonnesRow(line);

                    if (!donnesLine.Valid)
                        continue;

                    Console.Write(String.Format("\rProgress {0}%", donnes.Progress * 100f));
                    // Store to database
                    try
                    {
                        command.CommandText = "INSERT INTO Donnes (id, model, sku, quantity) VALUES (@id, @model, @sku, @quantity)";

                        command.Parameters.Add("@id", DbType.Int32).Value = donnesLine.ID;
                        command.Parameters.Add("@model", DbType.String).Value = donnesLine.Model;
                        command.Parameters.Add("@sku", DbType.String).Value = donnesLine.SKU;
                        command.Parameters.Add("@quantity", DbType.Int32).Value = donnesLine.Quantity;

                        command.ExecuteNonQuery();
                    }
                    catch (SQLiteException sQLiteException)
                    {
                        Console.WriteLine(String.Format("DbController: LoadDonnes from file sQLiteException {0}", sQLiteException.Message), "Error reading from database");
                    }
                }
            }
        }
    }
}
