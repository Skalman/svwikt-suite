using System;
using Gtk;
using System.IO;
using System.Xml.Serialization;
using System.Data.SQLite;
using System.Data.SQLite.Generic;

namespace SvwiktSuite
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Application.Init();
			MainWindow win = new MainWindow();
			win.Show();
			Application.Run();

			string folder = "..\\..\\data";
			SQLiteConnection db = new SQLiteConnection ("URI=file:" + System.IO.Path.Combine(folder, "svwiktionary-maininfl.db"));
			db.Open();
			SQLiteCommand cmd = db.CreateCommand();
			cmd.CommandText = "SELECT pagename, tmpl_syntax FROM main";
			SQLiteDataReader reader =  cmd.ExecuteReader();
			int count = 0;
			while (reader.Read() && count < 100) {
				Console.WriteLine(reader.GetString(0) + " : " + reader.GetString(1));
				count++;
			}
			reader.Close();
			reader = null;
			cmd.Dispose();
			cmd = null;
			db.Close();
			db = null;
		}
	}
}
