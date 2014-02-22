using System;
using System.Collections.Generic;

namespace SvwiktSuite
{
    public class WikitextUtils
    {
        private WikitextUtils()
        {
        }

        /// <summary>
        /// Like IndexOf, but ignores wikitext features.
        /// Inspired by https://en.wiktionary.org/wiki/User:Conrad.Irwin/editor.js
        /// </summary>
        public static int VanillaIndexOf(string search, string text, int pos = 0)
        {
            var cpos = 0;
            var tpos = 0;
            var wpos = 0;
            var spos = 0;
            do
            {
                cpos = text.IndexOf("<!--", pos);
                tpos = text.IndexOf("{{", pos);
                wpos = text.IndexOf("<nowiki>", pos);
                spos = text.IndexOf(search, pos);
 
                pos = Math.Min(
                Math.Min(
                    cpos == -1 ? int.MaxValue : cpos, 
                    tpos == -1 ? int.MaxValue : tpos
                ), 
                Math.Min(
                    wpos == -1 ? int.MaxValue : wpos,
                    spos == -1 ? int.MaxValue : spos
                )
                );
 
                if (pos == spos)
                    return pos == int.MaxValue ? -1 : pos;
                else if (pos == cpos)
                    pos = text.IndexOf("-->", pos) + 3;
                else if (pos == wpos)
                    pos = text.IndexOf("</nowiki>", pos) + 9;
                else if (pos == tpos)
                    pos = VanillaIndexOf("}}", text, pos + 2);

            } while (pos < int.MaxValue);
            return -1;
        }
    }
}

