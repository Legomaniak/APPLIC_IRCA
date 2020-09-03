using AppJCE.Ini;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace EasyCC
{
    public class MyInitScrypt : ISlozkaPolozkaString
    {
        public string Jmeno { get; set; }
        public int Cislo { get; set; }
        public string Kategorie { get; set; }
        public string Text { get; set; }
        public void Load(string source)
        {
            string[] s = source.Split('\n');
            string text = "";
            if (s?.Length > 0)
            {
                try
                {
                    int i = 0;
                    Jmeno = s[i++];
                    Cislo = int.Parse(s[i++]);
                    Kategorie = s[i++];
                    for (; i < s.Length; i++) if (!string.IsNullOrEmpty(s[i])) text += s[i] + '\n';
                    Text = text;
                }
                catch
                {
                    Cislo = 0;
                    Kategorie = "";
                    text = "";
                    for (int i = 1; i < s.Length; i++) if (!string.IsNullOrEmpty(s[i])) text += s[i] + '\n';
                    Text = text;
                }
            }
        }
        public void Save(ref string source)
        {
            if (source != null)
            {
                source += Jmeno + "\n";
                source += Cislo + "\n";
                source += Kategorie + "\n";
                source += Text;
            }
        }
        public void LoadXML(string cesta)
        {
            var XR = XmlReader.Create(cesta);
            while (XR.Read())
            {
                switch (XR.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (XR.Name)
                        {
                            case "Jmeno":
                                XR.Read();
                                Jmeno = XR.Value;
                                XR.Read();
                                break;
                            case "Cislo":
                                XR.Read();
                                Cislo = int.Parse(XR.Value);
                                XR.Read();
                                break;
                            case "Kategorie":
                                XR.Read();
                                Kategorie = XR.Value;
                                XR.Read();
                                break;
                            case "Text":
                                XR.Read();
                                Text = XR.Value;
                                XR.Read();
                                break;
                        }
                        break;
                }
            }
        }
        public void SaveXML(string cesta)
        {
            var XW = XmlWriter.Create(cesta);
            XW.WriteElementString("Jmeno", Jmeno);
            XW.WriteElementString("Cislo", Cislo.ToString());
            XW.WriteElementString("Kategorie", Kategorie);
            XW.WriteElementString("Text", Text);
        }

    }
}
