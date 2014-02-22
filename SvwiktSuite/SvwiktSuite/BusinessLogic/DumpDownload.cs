using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading;
using System.Xml;
using System.Web;
using ICSharpCode.SharpZipLib.BZip2;

namespace SvwiktSuite
{
	public enum ShortMonth
	{
		Jan,
		Feb,
		Mar,
		Apr,
		May,
		Jun,
		Jul,
		Aug,
		Sep,
		Oct,
		Nov,
		Dec
	}

	public class DumpDownload
	{
		public DateTime CurrentDumpTime { get; private set; }
		public DateTime NewDumpTime { get; private set; }
		public bool HasNew { get { return (NewDumpTime > CurrentDumpTime); } }
		public Uri Location { get; private set; }

		readonly Uri SvwiktDumpInfoUri = new Uri("http://dumps.wikimedia.org/svwiktionary/latest/svwiktionary-latest-pages-articles.xml.bz2-rss.xml");

		public DumpDownload() : this(DateTime.MinValue) 
		{
		}

		public DumpDownload(DateTime currentDumpTime)
		{
			Initialize();
		}

		public void Initialize()
		{
			try
			{
				var request = (HttpWebRequest)WebRequest.Create(SvwiktDumpInfoUri);
				var response = (HttpWebResponse)request.GetResponse();
				var stream = response.GetResponseStream();
				string content;

				using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
				{
					content = reader.ReadToEnd();
				}

				var xmlDoc = new XmlDocument();
				xmlDoc.LoadXml(content);

				var pubTime = xmlDoc.GetElementsByTagName("pubDate")[0].InnerText;
				var parsedPubTime = pubTime.Split(new Char[] { ' ', ':' });

				NewDumpTime = new DateTime(
					Convert.ToInt32(parsedPubTime[3]),
					(int)Enum.Parse(typeof(ShortMonth), parsedPubTime[2]),
					Convert.ToInt32(parsedPubTime[1]),
					Convert.ToInt32(parsedPubTime[4]),
					Convert.ToInt32(parsedPubTime[5]),
					Convert.ToInt32(parsedPubTime[6])
				);

				var description = xmlDoc.GetElementsByTagName("description")[1].InnerText;

				var regex = new Regex(@"<a href=""(.*?)"">");
				var match = regex.Match(description).Groups[1];
				if (match.Success)
				{
					Location = new Uri(match.Value);
				}
			}
			catch (Exception)
			{
				throw new Exception("Failed to get info about newest dump.");
			}
		}

		public void Download()
		{
			var webClient = new WebClient();
			var saveFileName = Path.GetFileName(Location.LocalPath);
			webClient.DownloadFile(Location, saveFileName);
		}

		public void Extract()
		{
			BZip2.Decompress(File.OpenRead(Path.GetFileName(Location.LocalPath)),
				File.Create(Path.GetFileNameWithoutExtension(Location.LocalPath)));
		}
	}
}