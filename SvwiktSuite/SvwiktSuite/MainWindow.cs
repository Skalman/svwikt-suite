using System;
using Gtk;
using Mono.Data.Sqlite;

public partial class MainWindow: Gtk.Window
{
	public MainWindow () : base (Gtk.WindowType.Toplevel)
	{
		Build ();
	}

	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
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

		//skapa Command
		SqliteCommand cmd = db.CreateCommand();

		//SQL query
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
	protected void OnButtonDbTestClicked (object sender, EventArgs e)
	{
		//kör databasläsningstest
		ReadFromDbTest();
	}

}
