using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SvwiktSuite
{
    public class FormattingEditor : IBatchEditor
    {
        public FormattingEditor()
        {
        }

        public void EditBatch(IList<Page> batch)
        {
            foreach (var page in batch)
                FormatPage(page);
        }

        private static void FormatPage(Page page)
        {
            var wikitext = page.Text;
            var summary = page.Summary;

            // Added on many pages by User:Pametzma
            if (wikitext.IndexOf("----") != -1)
            {
                var newWikitext = Regex.Replace(wikitext, @"\n+(\{\{nollpos\}\}\n|\-{4,}\n)+\n*", "\n\n");
                if (wikitext != newWikitext)
                {
                    summary.AddMinor("ta bort onödig {{nollpos}} och ----");
                    wikitext = newWikitext;
                }
            }

            // Incorrect translation heading
            if (wikitext.IndexOf("\n=====Översättningar=====\n") != -1)
            {
                summary.Add("översättningsrubrik är H4");
                wikitext = wikitext.Replace("\n=====Översättningar=====\n", "\n====Översättningar====\n");
            }

            if (wikitext.IndexOf("\n====Översättning====\n") != -1)
            {
                summary.Add("'Översättning' > 'Översättningar'");
                wikitext = wikitext.Replace("\n====Översättning====\n", "\n====Översättningar====\n");
            }

            if (wikitext.IndexOf("\n====Motsvarande namn på andra språk====\n") != -1)
            {
                summary.Add("'Motsvarande namn på andra språk' > 'Översättningar'");
                wikitext = wikitext.Replace("\n====Motsvarande namn på andra språk====\n", "\n====Översättningar====\n");
            }

            page.Text = wikitext;
        }
    }
}

