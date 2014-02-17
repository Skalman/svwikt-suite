using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace SvwiktSuite
{
    /// <summary>
    /// Settings sorter on the wiki. Retrieves the requested settings live.
    /// For an overview, see https://sv.wiktionary.org/wiki/Wiktionary:Anv%C3%A4ndare/Robotar/Anv%C3%A4ndbara_sidor.
    /// </summary>
    public class Settings
    {
        protected MediaWikiApi api;

        public Settings(MediaWikiApi api)
        {
            this.api = api;
        }

        public void LoadAll()
        {
            Load(
                approvedInflectionTemplates: true,
                languages: true,
                languageMisspellings: true,
                unofficialLanguages: true);
        }

        public void Load(
            bool approvedInflectionTemplates = false,
            bool languages = false,
            bool languageMisspellings = false,
            bool unofficialLanguages = false)
        {
            // Get pages.
            var pageTitles = new List<string>();
            if (approvedInflectionTemplates)
                pageTitles.Add(Pages.ait);
            if (languages)
                pageTitles.Add(Pages.lang);
            if (languageMisspellings)
                pageTitles.Add(Pages.langMisspell);
            if (unofficialLanguages)
                pageTitles.Add(Pages.langUnoff);

            var pages = api.GetPages(pageTitles);

            // Parse pages.
            if (approvedInflectionTemplates)
                ParseAit(pages [Pages.ait]);
            if (languages)
                ParseLang(pages [Pages.lang]);
            if (languageMisspellings)
                ParseLangMisspell(pages [Pages.langMisspell]);
            if (unofficialLanguages)
                ParseLangUnoff(pages [Pages.langUnoff]);
        }

        private static class Pages
        {
            public static string
                ait = "Wiktionary:Användare/Robotar/Godkända grammatikmallar",
                lang = "Wiktionary:Stilguide/Språknamn",
                langMisspell = "Wiktionary:Användare/Robotar/Språk/Felstavningar",
                langUnoff = "Wiktionary:Användare/Robotar/Språk/Inofficiella";
        };

        private string GetPageFromStart(Page page, string pageTitle)
        {
            if (page == null)
                page = api.GetPage(pageTitle);
            var pos = page.Text.IndexOf("<!-- START -->");
            if (pos == -1)
                throw new Exception(string.Format(
                    "[[{0}]]: Expected wikitext to contain '<!-- START -->'.",
                    pageTitle)
                );

            return page.Text.Substring(pos + "<!-- START -->".Length);
        }

        private bool IsContentLine(string line, string startChars, string pageTitle)
        {
            if (line == "" || line.StartsWith("<!--"))
                return false;

            if (!startChars.Contains(line.Substring(0, 1)))
                throw new Exception(string.Format(
                    "[[{0}]]: Found line '{1}'. Line must start with either '{2}' or '<!--'.",
                    pageTitle,
                    line,
                    string.Join("', '", startChars.ToCharArray())
                )
                );
            return true;
        }

        private void Assert(bool value, string pageTitle, string line, string message)
        {
            if (!value)
                throw new Exception(string.Format(
                    "[[{0}]]: Found line '{1}'. {2}",
                    pageTitle, line, message)
                );
        }

        private IList<string> _ait = null;

        public IList<string> ApprovedInflectionTemplates
        {
            get
            {
                if (_ait == null)
                    ParseAit();
                return _ait;
            }
        }

        private void ParseAit(Page page = null)
        {
            var text = GetPageFromStart(page, Pages.ait);
            var lines = text.Split('\n');
            var templates = new List<string>();
            foreach (var line in lines)
            {
                // Format: *<template>
                if (!IsContentLine(line, "*", Pages.ait))
                    continue;

                Assert(line.IndexOf('-') != -1, Pages.ait, line,
                       "Line must contain '-'.");

                Assert(line.IndexOf(' ') == -1, Pages.ait, line,
                       "Line must not contain a space.");

                templates.Add(line.Substring(1));
            }
            _ait = templates;
        }

        private IDictionary<string, string>
            _langByCode = null,
            _langByName = null;

        public IDictionary<string, string> LanguagesByCode
        {
            get
            {
                if (_langByCode == null)
                    ParseLang();
                return _langByCode;
            }
        }

        public IDictionary<string, string> LanguagesByName
        {
            get
            {
                if (_langByName == null)
                    ParseLang();
                return _langByName;
            }
        }

        private void ParseLang(Page page = null)
        {
            if (page == null)
                page = api.GetPage(Pages.lang);
            _langByCode = new Dictionary<string, string>();
            _langByName = new Dictionary<string, string>();
            foreach (Match match in Regex.Matches (page.Text, @"\n\{\{språk\|([^\|]+)\|([^\|]+)\|"))
            {
                _langByCode.Add(
                    match.Groups [2].Captures [0].Value,
                    match.Groups [1].Captures [0].Value
                );
                _langByName.Add(
                    match.Groups [1].Captures [0].Value,
                    match.Groups [2].Captures [0].Value
                );
            }
        }

        private IDictionary<string, string> _langMisspell = null;

        public IDictionary<string, string> LanguageMisspellings
        {
            get
            {
                if (_langMisspell == null)
                    ParseLangMisspell();
                return _langMisspell;
            }
        }

        private void ParseLangMisspell(Page page = null)
        {
            var text = GetPageFromStart(page, Pages.langMisspell);
            var lines = text.Split('\n');

            var res = new Dictionary<string, string>();

            foreach (var line in lines)
            {
                // Format: *<correct lang name>: <misspelling> | <misspelling>
                if (!IsContentLine(line, "*", Pages.langMisspell))
                    continue;

                var pos = line.IndexOf(": ");
                Assert(pos != -1, Pages.langMisspell, line,
                       "Line must contain ': '.");

                var correct = line.Substring(1, pos - 1);
                var misspellings = Regex.Split(line.Substring(pos + 2), @" \| ");

                foreach (var misspelling in misspellings)
                {
                    Assert(
                        misspelling.IndexOf('|') == -1 && misspelling.Trim() == misspelling,
                        Pages.langMisspell, line,
                        "Misspellings must be separated by ' | ', i.e. space, vertical bar, space.");

                    Assert(!res.ContainsKey(misspelling), Pages.langMisspell, line,
                           "Misspelling '" + misspelling + "' found twice.");

                    Assert(misspelling != correct, Pages.langMisspell, line,
                           "Misspelling the same as the correct form.");

                    res.Add(misspelling, correct);
                }
            }
            _langMisspell = res;
        }

        private IDictionary<string, string> _langUnoff = null;

        public IDictionary<string, string> LanguagesByNameUnofficial
        {
            get
            {
                if (_langUnoff == null)
                    ParseLangUnoff();
                return _langUnoff;
            }
        }

        private void ParseLangUnoff(Page page = null)
        {
            var text = GetPageFromStart(page, Pages.langUnoff);
            var lines = text.Split('\n');

            var res = new Dictionary<string, string>();
            foreach (var line in lines)
            {
                if (!IsContentLine(line, "*;", Pages.langUnoff))
                    continue;

                if (line [0] == ';')
                    continue;

                var pos = line.IndexOf(": \"");
                Assert(pos != -1, Pages.langUnoff, line,
                       "Line must contain ': \"'.");

                Assert(line.Length > pos + 3 && line.EndsWith("\""), Pages.langUnoff, line,
                       "Line must end with '\"'.");

                var lang = line.Substring(1, pos - 1);
                var code = line.Substring(pos + 3, line.Length - pos - 4);

                res[lang] = code;
            }
            _langUnoff = res;
        }

        private ISet<string> _wikts = null;

        public ISet<string> Wiktionaries
        {
            get
            {
                if (_wikts == null)
                {
                    // Get.
                    var map = api.Get("action=query&meta=siteinfo&siprop=interwikimap&sifilteriw=local");
                    _wikts = new SortedSet<string>();

                    foreach (var iw in (IEnumerable<JToken>)map["query"]["interwikimap"])
                    {
                        if ("https://" + iw ["prefix"] + ".wiktionary.org/wiki/$1" ==
                            "" + iw ["url"])
                        {
                            _wikts.Add((string)iw ["prefix"]);
                        }
                    }
                }
                return _wikts;
            }
        }
    }
}

