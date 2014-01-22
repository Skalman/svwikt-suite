using System;
using Gtk;
using System.IO;

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
		}

	}
}
