using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace SvwiktSuite
{
    public class Api
    {
        protected NetUtils netUtils;

        public Api(
            string defaultDomain = null,
            string userAgent = null)
        {
            if (defaultDomain == null)
                defaultDomain = "https://sv.wiktionary.org";

            netUtils = new NetUtils(
                defaultUri: defaultDomain,
                userAgent: userAgent);
        }

        public JObject Get(
            string parameters,
            string domain = null,
            bool cookies = false)
        {
            string uri = domain == null ? "" : domain;
            uri += "/w/api.php?format=json&maxlag=2";

            JObject json = netUtils.Get(
                uri: uri,
                parameters: parameters,
                cookies: cookies);

            if (json ["error"] != null && (string)json ["error"] ["code"] == "maxlag")
            {
                Console.WriteLine("Maxlag: Sleeping for 5 sec...");
                Thread.Sleep(5000);
                return Get(
                    parameters: parameters,
                    domain: domain,
                    cookies: cookies);
            }

            return json;
        }

        public JObject Get(IDictionary<string, string> parameters,
                                   string domain = null,
                                   bool cookies = false)
        {
            return Get(
                NetUtils.EncodeParameters(parameters),
                domain: domain,
                cookies: cookies);
        }

        protected CookieContainer Cookies = null;

        public JObject Post(IDictionary<string, string> parameters,
                                    string domain = null,
                                    string action = null,
                                    bool showAllData = false,
                                    bool cookies = true)
        {
            return Post(
                NetUtils.EncodeParameters(parameters),
                domain: domain,
                action: action,
                showAllData: showAllData,
                cookies: cookies);
        }

        public JObject Post(
            string data,
            string domain = null,
            string action = null,
            bool showAllData = false,
            bool cookies = true,
            string uriAppend = null)
        {
            string uri = domain == null ? "" : domain;
            uri += "/w/api.php?format=json&maxlag=2";

            if (action != null)
                uri += "&action=" + action;

            var json = netUtils.Post(
                uri: uri,
                data: data,
                cookies: cookies,
                showAllData: showAllData);

            if (json ["error"] != null && (string)json ["error"] ["code"] == "maxlag")
            {
                Console.WriteLine("Maxlag: Sleeping for 5 sec...");
                Thread.Sleep(5000);
                return Post(
                    data: data,
                    domain: domain,
                    action: action,
                    showAllData: showAllData,
                    cookies: cookies,
                    uriAppend : uriAppend);
            }

            return json;
        }

        protected string _signedInUser = null;

        public string SignedInUser
        {
            get { return _signedInUser; }
        }

        public bool SignIn(string username, string password)
        {
            var tokenResponse = Post(new Dictionary<string,string> {
                {"lgname", username},
            }, action: "login");
            // result should be: "NeedToken"
            var token = (string)tokenResponse ["login"] ["token"];
            var response = Post(new Dictionary<string,string> {
                {"lgname", username},
                {"lgpassword", password},
                {"lgtoken", token}
            }, action: "login");

            var isSuccess = (string)response ["login"] ["result"] == "Success";
            if (isSuccess)
                _signedInUser = username;
            return isSuccess;
        }

        protected string EditToken = null;

        public void SavePage(
            Page page,
            string summary,
            bool nocreate=false,
            bool bot=true)
        {
            SavePage(
                title: page.Title,
                wikitext: page.Text,
                summary: summary,
                nocreate: nocreate,
                bot: bot,
                timestamp: page.Timestamp);
        }

        public bool SavePage(
            string title,
            string wikitext,
            string summary,
            bool nocreate=false,
            bool bot=true,
            string timestamp=null)
        {
            if (EditToken == null)
            {
                var tokens = Get("action=tokens", cookies: true);
                EditToken = (string)tokens ["tokens"] ["edittoken"];
            }
            var editResponse = Post(
                new Dictionary<string, string>{
                    {"action", "edit"},
                    {"title", title},
                    {"text", wikitext},
                    {"summary", summary},
                    {nocreate ? "nocreate" : "", ""},
                    {"assert", "user"},
                    {"basetimestamp", timestamp},
                    {bot ? "bot" : "", ""},
                    {"token", EditToken},
                },
                cookies: true
            );

            if (editResponse ["error"] != null)
            {
                if ((string)editResponse ["error"] ["code"] == "badtoken")
                {
                    var loggedInInfo = Get("action=query&meta=userinfo&uiprop=groups", cookies: true);
                    if (loggedInInfo ["query"] ["userinfo"] ["anon"] != null)
                    {
                        _signedInUser = null;
                        EditToken = null;
                        throw new NotLoggedInException();
                    } else
                    {
                        throw new Exception(
                            "Logged in as " +
                            loggedInInfo ["query"] ["userinfo"] ["name"] +
                            " but has invalid token."
                        );
                    }
                } else if ((string)editResponse ["error"] ["code"] == "editconflict")
                {
                    throw new EditConflictException();
                }
            }

            if ((string)editResponse ["edit"] ["result"] == "Success")
            {
                return true;
            } else
            {
                return false;
            }
        }

        public class NotLoggedInException : Exception
        {
            public NotLoggedInException() : base()
            {
            }
        }

        public class EditConflictException : Exception
        {
            public EditConflictException() : base()
            {
            }
        }

        public Page GetPage(string title)
        {
            var response = Get(
                new Dictionary<string, string> {
                    {"action", "query"},
                    {"titles", title},
                    {"prop", "revisions"},
                    {"rvprop", "timestamp|content"},
                }
            );
            var pages = (IDictionary<string, JToken>)response ["query"] ["pages"];
            foreach (var page in pages)
            {
                return new Page(page.Value);
            }
            throw new Exception("Could not retrieve page '" + title + "'");
        }

        public IDictionary<string, Page> GetPages(IEnumerable<string> titles)
        {
            var response = Get(
                new Dictionary<string, string> {
                    {"action", "query"},
                    {"titles", string.Join("|", titles)},
                    {"prop", "revisions"},
                    {"rvprop", "timestamp|content"},
                }
            );
            var res = new Dictionary<string, Page>();
            var pages = (IDictionary<string, JToken>)response ["query"] ["pages"];
            foreach (var page in pages)
            {
                var p = new Page(page.Value);
                res [p.Title] = p;
            }
            return res;
        }

        public IDictionary<string, bool> PagesExist(
            string langCode, IList<string> pages,
            IDictionary<string, bool> addTo = null)
        {
            var res = new Dictionary<string, bool>();
            if (pages.Count <= 50)
            {
                PagesExistMax50(langCode, pages, res);
            } else
            {
                for (var i = 0; i < pages.Count; i += 50)
                {
                    PagesExistMax50(
                        langCode,
                        new List<string>(pages.Skip(i).Take(50)),
                        res);
                }
            }

            return res;
        }

        private void PagesExistMax50(
            string langCode, IList<string> pages,
            IDictionary<string, bool> addTo)
        {
            /*
            if (langCode != "ml") {
                foreach (var p in pages) {
                    addTo [p] = false;
                }
                return;
            }
            /**/
            if (pages.Count == 1)
            {
                // avoid normalizing and rerequesting
                addTo [pages [0]] = PageExists(langCode, pages [0]);
                return;
            }

            var response = Get(
                new Dictionary<string, string> {
                    {"action", "query"},
                    {"titles", string.Join("|", pages)}
                },
                "https://" + langCode + ".wiktionary.org",
                cookies: true
            );
            var pageDict = (IDictionary<string, JToken>)response ["query"] ["pages"];

            foreach (var kv in pageDict)
            {
                addTo [(string)kv.Value ["title"]] = (string)kv.Value ["missing"] != "";
            }
            var normalized = (IList<JToken>)response ["query"] ["normalized"];
            if (normalized != null)
            {
                foreach (var item in normalized)
                {
                    var from = (string)item ["from"];
                    var to = (string)item ["to"];
                    addTo [from] = addTo [to];
                }
            }
            // Unicode normalization - try again
            foreach (var p in pages)
            {
                if (!addTo.ContainsKey(p))
                {
                    addTo.Add(p, PageExists(langCode, p));
                }
            }
        }

        public bool PageExists(string langCode, string title)
        {
            var response = Get(
                new Dictionary<string, string> {
                    {"action", "query"},
                    {"titles", title}
                },
                "https://" + langCode + ".wiktionary.org"
            );
            var pageDict = (IDictionary<string, JToken>)response ["query"] ["pages"];

            foreach (var kv in pageDict)
            {
                return (string)kv.Value ["missing"] != "";
            }
            throw new Exception("Shouldn't be able to reach this");
        }

        public IEnumerable<Page>PagesInCategory(
            string category,
            int ns = -1,
            int step = 50,
            int maxPages = -1,
            string startAt = "")
        {
            string gcmcontinue = "";
            if (maxPages < 0)
            {
                // make it "infinitely" many
                maxPages = int.MaxValue;
            }
            int pagesLeft = maxPages;
            while (gcmcontinue != null && pagesLeft > 0)
            {
                step = Math.Min(pagesLeft, step);
                var response = Get(
                    new Dictionary<string, string> {
                        {"action", "query"},
                        {"generator", "categorymembers"},
                        {"prop", "revisions"},
                        {"rvprop", "timestamp|content"},
                        {"gcmtitle", "Category:" + category},
                        {"gcmnamespace", ns == -1 ? "" : ns.ToString()},
                        {"gcmlimit", step.ToString()},
                        {"gcmstartsortkeyprefix", startAt},
                        {"gcmcontinue", gcmcontinue},
                    }
                );
                try
                {
                    gcmcontinue = (string)response ["query-continue"] ["categorymembers"] ["gcmcontinue"];
                } catch (NullReferenceException)
                {
                    gcmcontinue = null;
                }
                var members = (IDictionary<string, JToken>)response ["query"] ["pages"];
                foreach (var m in members)
                {
                    yield return new Page(m.Value);
                }
                pagesLeft -= step;
            }
        }

        public string[] ExpandTemplates(
            IEnumerable<string> wikitext,
            bool includeComments = true)
        {
            var separator = "\n\n<expand templates separator>\n\n";
            var expanded = ExpandTemplates(
                string.Join(separator, wikitext),
                includeComments: includeComments);

            return Regex.Split(expanded, separator);
        }

        public string ExpandTemplates(
            string wikitext,
            bool includeComments = true)
        {
            var json = Get(new Dictionary<string, string> {
                {"action", "expandtemplates"},
                {"text", wikitext},
                {includeComments ? "includecomments" : "", "1"},
            }
            );
            return (string)json ["expandtemplates"] ["*"];
        }
    }
}

