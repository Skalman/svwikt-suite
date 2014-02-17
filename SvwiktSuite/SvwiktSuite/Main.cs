using System;
using Gtk;
using System.IO;

namespace SvwiktSuite
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Application.Init();

            var mwApi = new MediaWikiApi(
                "https://sv.wiktionary.org",
                "C# app by https://sv.wiktionary.org/wiki/User:Skalman");

            var editCtrl = new EditController(mwApi);

            MainWindow win = new MainWindow(editCtrl);
            win.Show();
            Application.Run();
        }

    }
}