using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SvwiktSuite
{
    public class ParsedPage
    {
        protected string _title, _timestamp;

        public ParsedPage(string title, string text, string timestamp = "")
        {
            _title = title;
            _timestamp = timestamp;
            Text = text;
        }

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
                    @"^(==Källor==\n" +
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
                    select new H2Section(section)
                );
            }
        }
    }
}

