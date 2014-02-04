using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace SvwiktSuite
{
    public class NetUtils
    {
        public Uri BaseUri { get; set; }

        public string UserAgent { get; set; }

        protected CookieContainer Cookies { get; set; }

        public NetUtils(string defaultUri, string userAgent = null)
            : this(new Uri(defaultUri), userAgent)
        {
        }

        public NetUtils(Uri defaultUri = null, string userAgent = null)
        {
            BaseUri = defaultUri;
            UserAgent = userAgent;
        }

        public static string EncodeParameters(IDictionary<string, string> parameters)
        {
            if (parameters == null && parameters.Count == 0)
                return "";

            IEnumerable<string> list = parameters.Select(
                p => Uri.EscapeDataString(p.Key) +
                "=" + Uri.EscapeDataString(p.Value)
            );
            return string.Join("&", list);
        }

        protected HttpWebRequest PrepareRequest(Uri uri)
        {
            // Ignore invalid SSL certificates
            ServicePointManager.ServerCertificateValidationCallback = (a,b,c,d) => true;
            var req = (HttpWebRequest)WebRequest.Create(uri);
            req.UserAgent = UserAgent;
            req.MaximumAutomaticRedirections = 1;
            return req;
        }

        protected JObject GetResponseJson(HttpWebRequest req)
        {
            var res = (HttpWebResponse)req.GetResponse();
            Stream stream = res.GetResponseStream();
            var reader = new StreamReader(stream, Encoding.UTF8);
            var jsonText = reader.ReadToEnd();
            res.Close();
            reader.Close();
            return JObject.Parse(jsonText);
        }

        protected Uri MakeUri(string uri = null, string parameters = null)
        {
            if (uri == null && parameters == null)
                return BaseUri;

            Uri baseUri = BaseUri;

            if (baseUri != null && uri == null)
            {
                uri = baseUri.ToString();
                baseUri = null;
            }

            if (parameters != null)
            {
                if (uri.Contains("?"))
                    uri += "&" + parameters;
                else
                    uri += "?" + parameters;
            }

            if (baseUri != null)
                return new Uri(baseUri, uri);
            else
                return new Uri(uri);
        }

        public JObject Get(
            IDictionary<string, string> parameters,
            string uri = null,
            bool cookies = false)
        {
            return Get(
                uri: uri,
                parameters: EncodeParameters(parameters),
                cookies: cookies);
        }

        public JObject Get(
            string uri = null,
            string parameters = null,
            bool cookies = false)
        {
            Uri fullUri = MakeUri(uri, parameters);
            HttpWebRequest req = PrepareRequest(fullUri);
            if (cookies)
            {
                if (Cookies == null)
                    Cookies = new CookieContainer();
                req.CookieContainer = Cookies;
            }

            Console.WriteLine("GET {0}", fullUri.OriginalString);

            return GetResponseJson(req);
        }

        public JObject Post(
            string uri = null,
            string data = null,
            bool cookies = true,
            bool showAllData = false)
        {
            Uri fullUri = MakeUri(uri);
            HttpWebRequest req = PrepareRequest(fullUri);
            if (cookies)
            {
                if (Cookies == null)
                    Cookies = new CookieContainer();
                req.CookieContainer = Cookies;
            }
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";

            Console.WriteLine(
                "POST {0} (data: {1})",
                fullUri.OriginalString,
                showAllData ? data : Regex.Replace(
                    data,
                    @"((^|&)[^=]*)=[^&]+",
                    "$1=..."
            )
            );
            byte[] postData = Encoding.ASCII.GetBytes(data);
            req.ContentLength = postData.Length;
            Stream stream = req.GetRequestStream();
            stream.Write(postData, 0, postData.Length);
            stream.Close();

            return GetResponseJson(req);
        }
    }
}

