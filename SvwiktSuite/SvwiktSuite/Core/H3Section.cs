using System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace SvwiktSuite
{
    public class H3Section
    {
        protected string _text,
            _header, _h3Template, _boldLine, _pronunciation,
            _commonInfo, _translationSection, _footer;
        protected IList<string> _definitions;

        public H3Section(string pageTitle, string text)
        {
            PageTitle = pageTitle;
            _text = text;
        }

        /// <summary>
        /// Page title of the page that this section belongs to. May be null.
        /// </summary>
        public string PageTitle { get; set; }

        /// <summary>
        /// The beginning of the section, starting with ===Header===
        /// until the H3 template.
        /// </summary>
        public string Header
        {
            get
            {
                Parse();
                return _header;
            }
            set
            {
                Parse();
                _header = value;
            }
        }

        /// <summary>
        /// Any H3 templates and wikitext until the bold line.
        /// </summary>
        public string H3Template
        {
            get
            {
                Parse();
                return _h3Template;
            }
            set
            {
                Parse();
                _h3Template = value;
            }
        }

        /// <summary>
        /// The bold line, until the pronunciation guide or definitions begin.
        /// </summary>
        public string BoldLine
        {
            get
            {
                Parse();
                return _boldLine;
            }
            set
            {
                Parse();
                _boldLine = value;
            }
        }

        /// <summary>
        /// The pronunciation guide, if any.
        /// </summary>
        public string Pronunciation
        {
            get
            {
                Parse();
                return _pronunciation;
            }
            set
            {
                Parse();
                _pronunciation = value;
            }
        }

        /// <summary>
        /// List of definitions.
        /// </summary>
        public IList<string> Definitions
        {
            get
            {
                Parse();
                return _definitions;
            }
            set
            {
                Parse();
                _definitions = value;
            }
        }

        /// <summary>
        /// Common info after the definitions and before translations.
        /// </summary>
        public string CommonInfo
        {
            get
            {
                Parse();
                return _commonInfo;
            }
            set
            {
                Parse();
                _commonInfo = value;
            }
        }

        /// <summary>
        /// The translation section
        /// </summary>
        public string TranslationSection
        {
            get
            {
                Parse();
                return _translationSection;
            }
            set
            {
                Parse();
                _translationSection = value;
            }
        }

        /// <summary>
        /// Text after the translation section.
        /// </summary>
        public string Footer
        {
            get
            {
                Parse();
                return _footer;
            }
            set
            {
                Parse();
                _footer = value;
            }
        }

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
                if (_text != null)
                    return _text;
                else
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
                if (value == null)
                    throw new ArgumentNullException();

                // Unparse.
                if (_text == null)
                {
                    _header =
                        _h3Template =
                        _boldLine =
                        _pronunciation =
                        _commonInfo =
                        _translationSection =
                        _footer = null;
                    _definitions = null;
                }

                _text = value;
            }
        }

        protected void Parse()
        {
            if (_text != null)
            {
                var tmp = _text;
                _text = null;
                // Split into two parts: before definitions, and def+after.
                var pos = WikitextUtils.VanillaIndexOf("\n#", tmp);
                if (pos == -1)
                {
                    BeforeDefinitions = tmp;
                    DefinitionsAndAfter = "";
                } else
                {
                    BeforeDefinitions = tmp.Substring(0, pos + 1);
                    DefinitionsAndAfter = tmp.Substring(pos + 1);
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
                    _pronunciation = "";
                else
                {
                    _pronunciation = text.Substring(pos + 1);
                    text = text.Substring(0, pos + 1);
                }

                // Find bold line.
                pos = text.LastIndexOf("'''");
                if (pos != -1)
                    pos = text.LastIndexOf('\n', pos);

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
                    _boldLine = "";
                else
                {
                    _boldLine = text.Substring(pos + 1);
                    text = text.Substring(0, pos + 1);
                }

                // Find H3 template.
                pos = text.IndexOf("\n{{");

                if (pos == -1)
                    _h3Template = "";
                else
                {
                    _h3Template = text.Substring(pos + 1);
                    text = text.Substring(0, pos + 1);
                }

                // The rest is the header.
                _header = text;
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
                _definitions = new List<string>();

                do
                {
                    var endPos = WikitextUtils.VanillaIndexOf("\n", text, pos);
                    var line = text.Substring(pos, endPos - pos + 1);
                    if (!line.StartsWith("#:"))
                        _definitions.Add(line);
                    else
                        _definitions [Definitions.Count - 1] += line;

                    pos = endPos + 1;
                } while (pos < text.Length && text[pos] == '#');

                if (pos < text.Length)
                {
                    _commonInfo = "";
                    _translationSection = "";
                    _footer = "";
                    return;
                }
                text = text.Substring(pos);

                // Find common info and translation section start.
                pos = text.IndexOf("\n====Översättningar====\n");
                if (pos == -1)
                {
                    _commonInfo = text;
                    _translationSection = "";
                    _footer = "";
                    return;
                }
                _commonInfo = text.Substring(0, pos + 1);
                text = text.Substring(pos + 1);

                // Find footer.
                pos = text.IndexOf("\n====");
                if (pos == -1)
                {
                    _translationSection = text;
                    _footer = "";
                } else
                {
                    _translationSection = text.Substring(0, pos + 1);
                    _footer = text.Substring(pos + 1);
                }
            }
        }
    }
}
