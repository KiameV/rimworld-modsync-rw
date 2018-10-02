using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using Verse;

namespace ModSyncRW.Hosts
{
    class BitbucketHost : IHost
    {
        private const string ABOUT_DIR = "About";
        private const string MASTER_BRANCH = "master";

        public string Owner;
        public string Project;
        public string AboutDir = ABOUT_DIR;
        public string Branch = MASTER_BRANCH;
        
        public BitbucketHost() { }

        public HostEnum Type => HostEnum.Bitbucket;

        public string AboutXmlUri =>
            "https://bitbucket.org/" + this.Owner + "/" + this.Project + "/raw/" + this.Branch + "/" + AboutDir + "/About.xml";

        public string ModSyncXmlUri =>
            "https://bitbucket.org/" + this.Owner + "/" + this.Project + "/raw/" + this.Branch + "/" + AboutDir + "/ModSync.xml";

        public string DownloadPageUri
        {
            get
            {
                string url = "https://bitbucket.org/" + this.Owner + "/" + this.Project + "/get/" + this.Branch + ".zip";
                Log.Warning(url);
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
            XmlDocUtil.AddElement(xml, parent, "Branch", this.Branch);
        }
    }
}
