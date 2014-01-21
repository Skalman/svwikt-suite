using System;
using Gtk;
using System.IO;
//using System.Xml.Serialization;
using Mono.Data.Sqlite;

namespace SvwiktSuite
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Application.Init();
			/*
			MainWindow win = new MainWindow();
			win.Show();
			Application.Run();
			*/

			//kör databasläsningstest
			ReadFromDbTest();
		}

		/// <summary>
		/// Testmetod som läser från databas.
		/// </summary>
		public static void ReadFromDbTest()
		{
			//skapa connection string
			string folder = "../../data";
			string connectionString = "URI=file:" + System.IO.Path.Combine (folder, "test.db");

			//öppna databasen
			var db = new SqliteConnection(connectionString);
			//SQLiteConnection db = new SQLiteConnection(connectionString);
			db.Open();

			//SQL query
			SqliteCommand cmd = db.CreateCommand();
			cmd.CommandText = "SELECT pagename, tmpl_syntax FROM main";

			//läs de första 100 raderna från databasen och skriv ut dem i konsollen
			SqliteDataReader reader = cmd.ExecuteReader();
			int count = 0;
			while (reader.Read() && count < 100) {
				string column1Data = reader.GetString(0);
				string column2Data = reader.GetString(1);
				Console.WriteLine(String.Format ("{0} : {1}", column1Data, column2Data));
				count++;
			}

			//rensa upp
			reader.Close();
			reader = null;
			cmd.Dispose();
			cmd = null;
			db.Close();
			db = null;
		}
	}
}
