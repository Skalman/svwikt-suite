using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SvwiktSuite
{
    public class ParsedPage
    {
        public ParsedPage(Page page)
            : this(page.Title, page.Text, page.Timestamp)
        {
        }

        public ParsedPage(string title, string text, string timestamp = "")
        {
            Title = title;
            Timestamp = timestamp;
            Text = text;
        }

        public string Title { get; set; }

        public string Timestamp { get; set; }

        public string Header { get; set; }

        public IList<H2Section> H2Sections { get; set; }

        public string Footer { get; set; }

        public string Text
        {
            get
            {
                if (H2Sections == null)
                    return Header + Footer;

                return
                    Header +
                    string.Join("", H2Sections.Select(h => h.Text)) +
                    Footer;
            }
            set
            {
                // Find footer.
                Match m = Regex.Match(
                    value,
                    @"^(==(Källor|Källa)==\n" +
                    @"|\{\{STANDARDSORTERING:" +
                    @"|\[\[[a-z]{2,6}(\-[a-z]{2,12})*:[^\]]+\]\]\n)",
                    RegexOptions.Multiline
                );
                Footer = value.Substring(m.Index);
                string headerAndBody = value.Substring(0, m.Index);

                // Find H2 sections.
                string[] sections = Regex.Split(
                    headerAndBody,
                    @"^(?===[^=\n][^\n]*==\n)",
                    RegexOptions.Multiline);

                Header = sections [0];
                H2Sections = new List<H2Section>(
                    from section in sections.Skip(1)
                    select new H2Section(Title, section)
                );
            }
        }
    }
}

