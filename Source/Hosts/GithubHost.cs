using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using Verse;

namespace ModSyncRW.Hosts
{
    class GithubHost : IHost
    {
        private const string ABOUT_DIR = "About";
        private const string MASTER_BRANCH = "master";

        public enum DownloadLocationEnum { ModSyncMainPage, ModSyncReleases }

        public string Owner;
        public string Project;
        public string AboutDir = ABOUT_DIR;
        public DownloadLocationEnum DownloadLocation = DownloadLocationEnum.ModSyncMainPage;
        public string Branch = MASTER_BRANCH;
        
        public GithubHost() { }

        public HostEnum Type => HostEnum.Github;

        public string AboutXmlUri =>
            "https://raw.githubusercontent.com/" + this.Owner + "/" + this.Project + "/" + this.Branch + "/" + AboutDir + "/About.xml";

        public string ModSyncXmlUri => 
            "https://raw.githubusercontent.com/" + this.Owner + "/" + this.Project + "/" + this.Branch + "/" + AboutDir + "/ModSync.xml";

        public string DownloadPageUri
        {
            get
            {
                string url = "https://github.com/" + this.Owner + "/" + this.Project + "/";
                if (this.DownloadLocation == DownloadLocationEnum.ModSyncReleases)
                {
                    url += "releases/";
                }
                return url;
            }
        }

        public bool Validate()
        {
            return true;
        }

        public bool IsFormFilled => !string.IsNullOrEmpty(this.Owner) && !string.IsNullOrEmpty(this.Project) && !string.IsNullOrEmpty(this.Branch);

        public float DrawHost(float xMin, float y, float width)
        {
            Widgets.Label(new Rect(xMin, y, 100, 32), "ModSync.Owner".Translate());
            this.Owner = Widgets.TextField(new Rect(xMin + 110, y, 200, 32), this.Owner).Trim();
            y += 40;

            Widgets.Label(new Rect(xMin, y, 100, 32), "ModSync.Project".Translate());
            this.Project = Widgets.TextField(new Rect(xMin + 110, y, 200, 32), this.Project).Trim();
            y += 40;

            Widgets.Label(new Rect(xMin, y, 100, 32), "ModSync.AboutDir".Translate());
            this.AboutDir = Widgets.TextField(new Rect(xMin + 110, y, 200, 32), this.AboutDir).Trim();
            y += 40;

            Widgets.Label(new Rect(xMin, y, 100, 32), "ModSync.DownloadPage".Translate());
            if (Widgets.ButtonText(new Rect(xMin + 110, y, 100, 32), this.DownloadLocation.ToString().Translate()))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                foreach (DownloadLocationEnum dl in Enum.GetValues(typeof(DownloadLocationEnum)))
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
            this.Branch = Widgets.TextField(new Rect(xMin + 110, y, 200, 32), this.Branch).Trim();
            y += 40;
            return y;
        }

        public void LoadFromXml(XmlNode parentNode)
        {
            this.AboutDir = ABOUT_DIR;
            this.Branch = MASTER_BRANCH;
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
                    case "AboutDir":
                        this.AboutDir = el.InnerText;
                        break;
                    case "DownloadFrom":
                        try
                        {
                            this.DownloadLocation = (DownloadLocationEnum)Enum.Parse(typeof(DownloadLocationEnum), el.InnerText);
                        }
                        catch
                        {
                            this.DownloadLocation = DownloadLocationEnum.ModSyncMainPage;
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

        public void WriteToXml(XmlDocument xml, XmlElement parent)
        {
            XmlDocUtil.AddElement(xml, parent, "Owner", this.Owner);
            XmlDocUtil.AddElement(xml, parent, "Project", this.Project);
            XmlDocUtil.AddElement(xml, parent, "AboutDir", this.AboutDir);
            XmlDocUtil.AddElement(xml, parent, "DownloadFrom", this.DownloadLocation.ToString());
            XmlDocUtil.AddElement(xml, parent, "Branch", this.Branch);
        }
    }
}