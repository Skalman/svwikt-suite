using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace SvwiktSuite
{
    public class Page
    {
        public Page(JToken json)
            : this(
                title: (string)json ["title"],
                text: (string)json ["revisions"] [0] ["*"] + "\n",
                timestamp: (string)json ["revisions"] [0] ["timestamp"])
        {
        }

        public Page(string title, string text, string timestamp = "")
        {
            Text = text;
            OriginalText = text;
            Title = title;
            Timestamp = timestamp;
            Summary = new SummaryBuilder();
        }

        public string Text { get; set; }

        public string OriginalText { get; private set; }

        public string Title { get; private set; }

        public string Timestamp { get; private set; }

        public bool Changed
        {
            get
            {
                return Text != OriginalText;
            }
        }

        public SummaryBuilder Summary { get; private set; }

        public override string ToString()
        {
            return Title;
        }

        public class SummaryBuilder
        {

            protected IList<string> Parts, MinorParts;

            public SummaryBuilder()
            {
                Parts = new List<string>();
                MinorParts = new List<string>();
            }

            public void Add(string summary)
            {
                Parts.Add(summary);
            }

            public void AddMinor(string summary)
            {
                MinorParts.Add(summary);
            }

            public string Text
            {
                get
                {
                    var l = new List<string>(Parts);
                    l.AddRange(MinorParts);
                    return string.Join("; ", l.Distinct());
                }
                set
                {
                    MinorParts.Clear();
                    Parts.Clear();
                    if (value != null)
                        Parts.Add(value);
                }
            }

            public override string ToString()
            {
                return Text;
            }
        }
    }
}

