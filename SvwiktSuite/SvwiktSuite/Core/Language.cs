using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace SvwiktSuite
{
    public class Language
    {
        protected MediaWikiApi Api;
        protected Settings Settings;

        public Language(MediaWikiApi api)
        {
            Api = api;
            Settings = new Settings(api);
        }

        public bool HasWiki(string langCode)
        {
            return Settings.Wiktionaries.Contains(langCode);
        }

        public string GetCode(string langName)
        {
            string val = null;
            if (Settings.LanguagesByName.TryGetValue(langName, out val))
                return val;
            else if (Settings.LanguageMisspellings.TryGetValue(langName, out val))
                return Settings.LanguagesByName [val];
            else if (Settings.LanguagesByNameUnofficial.TryGetValue(langName, out val))
                return val;
            else if (langName.StartsWith("{{") && langName.EndsWith("}}"))
                return langName.Substring(2, langName.Length - 4);
            else
                throw new LanguageException("Unrecognized language name '" + langName + "'");
        }

        public class LanguageException : Exception
        {
            public LanguageException(string message) : base(message)
            {
            }
        }

        public string GetName(string langCode)
        {
            return Settings.LanguagesByCode [langCode];
        }

        public bool CorrectMisspellings(Section section)
        {
            var newText = section.Text;
            foreach (var x in Settings.LanguageMisspellings)
            {
                if (newText.IndexOf(x.Key) != -1)
                {
                    newText = newText.Replace(
                        "\n*" + x.Key + ": ",
                        "\n*" + x.Value + ": ");
                }
            }
            if (newText.IndexOf("\n*{{") != -1)
            {
                newText = Regex.Replace(
                    newText,
                    @"\n\*\{\{([a-z-]+)\}\}:",
                    new MatchEvaluator(ExpandTemplateCallback)
                );
            }
            if (newText == section.Text)
            {
                return false;
            } else
            {
                section.Text = newText;
                return true;
            }
        }

        private string ExpandTemplateCallback(Match m)
        {
            return "\n*" + GetName(m.Groups [1].Captures [0].Value) + ":";
        }
    }
}

