using System;
using Gtk;
using System.IO;
using System.Xml.Serialization;
using System.Data.SQLite;

namespace SvwiktSuite
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Application.Init ();
			MainWindow win = new MainWindow ();
			win.Show ();
			Application.Run ();

			string folder = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
			var db = new SQLiteConnection (System.IO.Path.Combine (folder, "notes.db"));
			db.CreateTable<Note>();
			var note = new Note { Message = "Test Note" };
			db.Insert (note);
			Console.WriteLine ("{0}: {1}", note.Id, note.Message);
		}

		public class Note
		{
			[PrimaryKey, AutoIncrement]
			public int Id { get; set; }
			public string Message { get; set; }
		}
	}
}
