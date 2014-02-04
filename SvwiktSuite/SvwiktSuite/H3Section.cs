using System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace SvwiktSuite
{
    public class H3Section
    {
        public H3Section(string text)
        {
            Text = text;
        }

        public string Text { get; set; }

        public string HeaderName
        {
            get
            {
                int lineEnd = Text.IndexOf('\n');

                Debug.Assert(lineEnd != -1);
                string firstLine = Text.Substring(0, lineEnd);

                Debug.Assert(firstLine.StartsWith("==="));
                Debug.Assert(firstLine.EndsWith("==="));
                return firstLine.Substring(3, firstLine.Length - 6);
            }
        }
    }


}

