using System;
using Newtonsoft.Json.Linq;

namespace SvwiktSuite
{
    public class Page
    {
        protected JToken Json;
        protected string _text, _title, _timestamp;

        public Page(JToken json)
        {
            Json = json;
            _text = null;
            _title = null;
            _timestamp = null;
        }

        public Page(string title, string text, string timestamp = "")
        {
            _text = text;
            _title = title;
            _timestamp = timestamp;
        }

        public string Text
        {
            get
            {
                if (_text == null)
                    _text = (string)Json ["revisions"] [0] ["*"] + "\n";
                return _text;
            }
            set
            {
                _text = value;
            }
        }

        public string Title
        {
            get
            {
                if (_title == null)
                    _title = (string)Json ["title"];
                return _title;
            }
        }

        public string Timestamp
        {
            get
            {
                if (_timestamp == null)
                    _timestamp = (string)Json ["revisions"] [0] ["timestamp"];
                return _timestamp;
            }
        }

        public override string ToString()
        {
            return Title;
        }
    }
}

