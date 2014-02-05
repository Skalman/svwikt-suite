using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace SvwiktSuite
{
    public class H2Section
    {
        public H2Section(string pageTitle, string text)
        {
            PageTitle = pageTitle;
            Text = text;
        }

        public H2Section(string pageTitle, string header, IList<H3Section> h3s)
        {
            PageTitle = pageTitle;
            Header = header;
            H3Sections = h3s;
        }

        /// <summary>
        /// Page title of the page that this section belongs to. May be null.
        /// </summary>
        public string PageTitle { get; set; }

        public string Header { get; set; }

        public string HeaderName
        {
            get
            {
                int lineEnd = Header.IndexOf('\n');

                Debug.Assert(lineEnd != -1);
                string firstLine = Header.Substring(0, lineEnd);

                Debug.Assert(firstLine.StartsWith("=="));
                Debug.Assert(firstLine.EndsWith("=="));
                return firstLine.Substring(2, firstLine.Length - 4);
            }
        }

        public IList<H3Section> H3Sections { get; set; }

        public string Text
        {
            get
            {
                if (H3Sections == null)
                    return Header;

                return
                    Header +
                    string.Join("", H3Sections.Select(h => h.Text));
            }
            set
            {
                // Find H3 sections.
                string[] sections = Regex.Split(
                    value,
                    @"^(?====[^=\n][^\n]*===\n)",
                    RegexOptions.Multiline);

                Header = sections [0];
                H3Sections = new List<H3Section>(
                    from section in sections.Skip(1)
                    select new H3Section(PageTitle, section)
                );
            }
        }
    }

}

