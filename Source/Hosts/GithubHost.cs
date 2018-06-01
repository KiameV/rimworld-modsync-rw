using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using Verse;

namespace ModSyncRW.Hosts
{
    public enum DownloadLocation { ModSyncMainPage, ModSyncReleases }

    class GithubHost : IHost
    {
        public string Owner;
        public string Project;
        public DownloadLocation DownloadLocation = DownloadLocation.ModSyncMainPage;
        public string Branch = "master";
        
        public GithubHost() { }

        public GithubHost(XmlNode parentNode)
        {
            foreach (XmlElement el in parentNode.ChildNodes)
            {
                switch (el.Name)
                {
                    case "Owner":
                        this.Owner = el.InnerText;
                        break;
                    case "Project":
                        this.Project = el.InnerText;
                        break;
                    case "DownloadFrom":
                        try
                        {
                            this.DownloadLocation = (DownloadLocation)Enum.Parse(typeof(DownloadLocation), el.InnerText);
                        }
                        catch
                        {
                            this.DownloadLocation = DownloadLocation.ModSyncMainPage;
                        }
                        break;
                    case "Branch":
                        this.Branch = el.InnerText;
                        break;
                }
            }
            if (String.IsNullOrEmpty(this.Branch))
            {
                this.Branch = null;
            }
        }

        public HostEnum Type => HostEnum.Github;

        public string ModSyncXmlUri => 
            "https://raw.githubusercontent.com/" + this.Owner + "/" + this.Project + "/" + this.Branch + "/About/ModSync.xml";

        public string DownloadPageUri
        {
            get
            {
                string url = "https://github.com/" + this.Owner + "/" + this.Project + "/";
                if (this.DownloadLocation == DownloadLocation.ModSyncReleases)
                {
                    url += "releases/";
                }
                return url;
            }
        }

        public float DrawHost(float xMin, float y, float width)
        {
            Widgets.Label(new Rect(xMin, y, 100, 32), "ModSync.Owner".Translate());
            this.Owner = Widgets.TextField(new Rect(xMin + 110, y, 200, 32), this.Owner);
            y += 40;

            Widgets.Label(new Rect(xMin, y, 100, 32), "ModSync.Project".Translate());
            this.Project = Widgets.TextField(new Rect(xMin + 110, y, 200, 32), this.Project);
            y += 40;

            Widgets.Label(new Rect(xMin, y, 100, 32), "ModSync.DownloadPage".Translate());
            if (Widgets.ButtonText(new Rect(xMin + 110, y, 100, 32), this.DownloadLocation.ToString().Translate()))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                foreach (DownloadLocation dl in Enum.GetValues(typeof(DownloadLocation)))
                {
                    options.Add(new FloatMenuOption(dl.ToString().Translate(), delegate ()
                    {
                        this.DownloadLocation = dl;
                    }));
                }
                Find.WindowStack.Add(new FloatMenu(options));
            }
            y += 40;

            Widgets.Label(new Rect(xMin, y, 100, 32), "ModSync.Branch".Translate());
            this.Branch = Widgets.TextField(new Rect(xMin + 110, y, 200, 32), this.Branch);
            y += 40;
            return y;
        }

        public void WriteToXml(XmlDocument xml, XmlElement parent)
        {
            XmlDocUtil.AddElement(xml, parent, "Owner", this.Owner);
            XmlDocUtil.AddElement(xml, parent, "Project", this.Project);
            XmlDocUtil.AddElement(xml, parent, "DownloadFrom", this.DownloadLocation.ToString());
            XmlDocUtil.AddElement(xml, parent, "Branch", this.Branch);
        }
    }
}