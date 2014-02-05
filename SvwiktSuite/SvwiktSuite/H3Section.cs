using System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace SvwiktSuite
{
    public class H3Section
    {
        public H3Section(string pageTitle, string text)
        {
            PageTitle = pageTitle;
            Text = text;
        }

        /// <summary>
        /// Page title of the page that this section belongs to. May be null.
        /// </summary>
        public string PageTitle { get; set; }

        /// <summary>
        /// The beginning of the section, starting with ===Header===
        /// until the H3 template.
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        /// Any H3 templates and wikitext until the bold line.
        /// </summary>
        public string H3Template { get; set; }

        /// <summary>
        /// The bold line, until the pronunciation guide or definitions begin.
        /// </summary>
        public string BoldLine { get; set; }

        /// <summary>
        /// The pronunciation guide, if any.
        /// </summary>
        public string Pronunciation { get; set; }

        /// <summary>
        /// List of definitions.
        /// </summary>
        public IList<string> Definitions { get; set; }

        /// <summary>
        /// Common info after the definitions and before translations.
        /// </summary>
        public string CommonInfo { get; set; }

        /// <summary>
        /// The translation section
        /// </summary>
        public string TranslationSection { get; set; }

        /// <summary>
        /// Text after the translation section.
        /// </summary>
        public string Footer { get; set; }

        public string HeaderName
        {
            get
            {
                var lineEnd = Header.IndexOf('\n');

                Debug.Assert(lineEnd != -1);
                string firstLine = Header.Substring(0, lineEnd);

                Debug.Assert(firstLine.StartsWith("==="));
                Debug.Assert(firstLine.EndsWith("==="));
                return firstLine.Substring(3, firstLine.Length - 6);
            }
        }

        public string Text
        {
            get
            {
                return
                    Header +
                    H3Template +
                    BoldLine +
                    Pronunciation +
                    string.Join("", Definitions) +
                    CommonInfo +
                    TranslationSection +
                    Footer;
            }
            set
            {
                // Split into two parts: before definitions, and def+after.
                var pos = WikitextUtils.VanillaIndexOf("\n#", value);
                if (pos == -1)
                {
                    BeforeDefinitions = value;
                    DefinitionsAndAfter = "";
                } else
                {
                    BeforeDefinitions = value.Substring(0, pos + 1);
                    DefinitionsAndAfter = value.Substring(pos + 1);
                }
            }
        }

        protected string BeforeDefinitions
        {
            set
            {
                // Header, H3Template, BoldLine, Pronunciation.
                var text = value;

                // Find pronunciation.
                var pos = WikitextUtils.VanillaIndexOf("\n*", text);
                if (pos == -1)
                    Pronunciation = "";
                else
                {
                    Pronunciation = text.Substring(pos + 1);
                    text = text.Substring(0, pos + 1);
                }

                // Find bold line.
                pos = text.LastIndexOf("\n'''");
                // E.g. Chinese doesn't use bold.
                if (pos == -1 && PageTitle != null)
                {
                    var lineStart = "\n" + PageTitle;
                    pos = text.LastIndexOf(lineStart);
                    if (pos != -1)
                    {
                        // The next char must be a normal char for bold lines.
                        var next = text [pos + lineStart.Length];
                        if (!" \n'{".Contains(next.ToString()))
                            // Search failed.
                            pos = -1;
                    }
                }
                if (pos == -1)
                    BoldLine = "";
                else
                {
                    BoldLine = text.Substring(pos + 1);
                    text = text.Substring(0, pos + 1);
                }

                // Find H3 template.
                pos = text.IndexOf("\n{{");

                if (pos == -1)
                    H3Template = "";
                else
                {
                    H3Template = text.Substring(pos + 1);
                    text = text.Substring(0, pos + 1);
                }

                // The rest is the header.
                Header = text;
            }
        }

        protected string DefinitionsAndAfter
        {
            set
            {
                // Definitions, common info, translation section, footer.
                var text = value;

                // Find definitions.
                var pos = 0;
                Definitions = new List<string>();

                do
                {
                    var endPos = WikitextUtils.VanillaIndexOf("\n", text, pos);
                    var line = text.Substring(pos, endPos - pos + 1);
                    if (!line.StartsWith("#:"))
                        Definitions.Add(line);
                    else
                        Definitions [Definitions.Count - 1] += line;

                    pos = endPos + 1;
                } while (pos < text.Length && text[pos] == '#');

                if (pos < text.Length)
                {
                    CommonInfo = "";
                    TranslationSection = "";
                    Footer = "";
                    return;
                }
                text = text.Substring(pos);

                // Find common info and translation section start.
                pos = text.IndexOf("\n====Översättningar====\n");
                if (pos == -1)
                {
                    CommonInfo = text;
                    TranslationSection = "";
                    Footer = "";
                    return;
                }
                CommonInfo = text.Substring(0, pos + 1);
                text = text.Substring(pos + 1);

                // Find footer.
                pos = text.IndexOf("\n====");
                if (pos == -1)
                {
                    TranslationSection = text;
                    Footer = "";
                } else
                {
                    TranslationSection = text.Substring(0, pos + 1);
                    Footer = text.Substring(pos + 1);
                }
            }
        }
    }
}
