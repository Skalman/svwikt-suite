using System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SvwiktSuite
{
    public class ToolLabsApi
    {
        protected NetUtils netUtils;

        public ToolLabsApi(string userAgent = null)
        {
            netUtils = new NetUtils(
                defaultUri: "https://tools.wmflabs.org/svwiktionary/suite/",
                userAgent: userAgent);
        }

        public PageInflections GetInflections(string title)
        {
            return GetInflections(new string[] {title})[0];
        }

        public IList<PageInflections> GetInflections(
            IEnumerable<string> titles)
        {
            JObject json = netUtils.Get(new Dictionary<string, string>() {
                {"action", "get_inflections"},
                {"titles", string.Join("|", titles)},
            }
            );
            List<PageInflections> res = new List<PageInflections>();

            var pages = (IDictionary<string, JToken>)json ["inflections"];
            foreach (var pJson in pages)
            {
                var p = new PageInflections(
                    title: (string)pJson.Key,
                    touched: (string)pJson.Value ["page_touched"]);
                res.Add(p);

                var templates = (IList<JToken>)pJson.Value ["templates"];
                foreach (var tJson in templates)
                {
                    var t = new PageInflections.Template(
                        name: (string)tJson ["template"]);
                    p.Templates.Add(t);

                    var inflections = (IList<JToken>)tJson ["inflections"];
                    foreach (var iJson in inflections)
                    {
                        t.Inflections.Add(new PageInflections.Inflection(
                            value: (string)iJson ["value"],
                            type: (string)iJson ["type"])
                        );
                    }
                }
            }
            return res;
        }
    }
}

