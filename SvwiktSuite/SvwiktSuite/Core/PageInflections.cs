using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SvwiktSuite
{
    public class PageInflections
    {
        public string Title { get; private set; }

        public string Touched { get; private set; }

        public IList<Template> Templates { get; private set; }

        public PageInflections(
            string title,
            string touched,
            IEnumerable<Template> templates = null)
        {
            Title = title;
            Touched = touched;

            if (templates == null)
                Templates = new List<Template>();
            else
                Templates = new List<Template>(templates);
        }

        override public string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public void MakeUnique(bool ignoreType = false)
        {
            var templates = new Dictionary<string, Template>();
            foreach (var t in Templates)
            {
                if (!templates.ContainsKey(t.Name))
                    templates [t.Name] = t;
                else
                    templates [t.Name].MergeWith(t);
            }
            Templates = new List<Template>(templates.Values);

            foreach (var t in Templates)
                t.MakeUnique(ignoreType);
        }

        public class Template
        {
            public string Name { get; private set; }

            public IList<Inflection> Inflections { get; private set; }

            public Template(
                string name,
                IEnumerable<Inflection> inflections = null)
            {
                Name = name;

                if (inflections == null)
                    Inflections = new List<Inflection>();
                else
                    Inflections = new List<Inflection>(inflections);
            }

            public void MakeUnique(bool ignoreType = false)
            {
                var inflections = new Dictionary<string, Inflection>();

                if (!ignoreType)
                {
                    foreach (var i in Inflections)
                        inflections [i.Type + " " + i.Value] = i;
                } else
                {
                    foreach (var i in Inflections)
                        inflections [i.Value] = i;
                }

                Inflections = new List<Inflection>(inflections.Values);
            }

            public void MergeWith(Template t)
            {
                foreach (var i in t.Inflections)
                    Inflections.Add(i);
            }

        }

        public class Inflection
        {
            public string Value { get; private set; }

            public string Type { get; private set; }

            public Inflection(string value, string type)
            {
                Value = value;
                Type = type;
            }
        }

    }
}

