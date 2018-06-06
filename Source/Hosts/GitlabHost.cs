using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using Verse;

namespace ModSyncRW.Hosts
{
    class GitlabHost : IHost
    {
        public enum DownloadLocationEnum { ModSyncMainPage, ModSyncTags }

        public string Owner;
        public string Project;
        public DownloadLocationEnum DownloadLocation = DownloadLocationEnum.ModSyncMainPage;
        public string Branch = "master";
        
        public GitlabHost() { }

        public HostEnum Type => HostEnum.Gitlab;

        public string AboutXmlUri =>
            "https://gitlab.com/" + this.Owner + "/" + this.Project + "/raw/" + this.Branch + "/About/About.xml";

        public string ModSyncXmlUri =>
            "https://gitlab.com/" + this.Owner + "/" + this.Project + "/raw/" + this.Branch + "/About/ModSync.xml";

        public string DownloadPageUri
        {
            get
            {
                string url = "https://gitlab.com/" + this.Owner + "/" + this.Project + "/";
                if (this.DownloadLocation == DownloadLocationEnum.ModSyncTags)
                {
                    url += "tags/";
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
            XmlDocUtil.AddElement(xml, parent, "DownloadFrom", this.DownloadLocation.ToString());
            XmlDocUtil.AddElement(xml, parent, "Branch", this.Branch);
        }
    }
}