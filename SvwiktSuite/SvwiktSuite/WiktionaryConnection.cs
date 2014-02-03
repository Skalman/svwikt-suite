using System;
using System.Net;
using System.Web;

class WiktionaryConnection
{
    public WiktionaryConnection()
    {
        //var webClient = new WebClient();

        //testa hämta en webbsida, funkar ej...
        WebRequest req = WebRequest.Create(new Uri("http://sv.wiktionary.org/wiki/Special:Inloggning"));
        WebResponse res = req.GetResponse();
        Console.WriteLine(">" + res + res + "<");
    }

}
