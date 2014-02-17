using System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

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
            Title = title;
            Timestamp = timestamp;
        }

        public string Text { get; set; }

        public string Title { get; private set; }

        public string Timestamp { get; private set; }

        public override string ToString()
        {
            return Title;
        }
    }
}

