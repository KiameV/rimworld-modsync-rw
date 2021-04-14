using System.Xml;
using UnityEngine;
using Verse;

namespace ModSyncRW.Hosts
{
    class DirectHost : IEditableHost
    {
        public string AboutUri;
        public string ModSyncUri;
        public string DownloadUri;

        public DirectHost() { }

        public HostEnum Type => HostEnum.Direct;
        public string AboutXmlUri => this.AboutUri;
        public string ModSyncXmlUri => this.ModSyncUri;
        public string DownloadPageUri => this.DownloadUri;
        public bool IsFormFilled => !string.IsNullOrEmpty(this.AboutUri) && !string.IsNullOrEmpty(this.DownloadUri);

        public bool Validate()
        {
            bool isValid = true;
            if (!this.AboutUri.EndsWith(".xml"))
            {
                Log.Error("About Uri must point to the page where the raw XML is hosted.");
                isValid = false;
            }

            if (this.AboutUri.IndexOf("about.xml") != -1)
            {
                this.ModSyncUri = this.AboutUri.Replace("about.xml", "ModSync.xml");
            }
            else if (this.AboutUri.IndexOf("About.xml") != -1)
            {
                this.ModSyncUri = this.AboutUri.Replace("About.xml", "ModSync.xml");
            }
            else
            {
                Log.Error("About Uri must point to the page where the raw About.xml is hosted.");
                isValid = false;
            }

            if (this.DownloadUri.EndsWith(".zip") || this.DownloadUri.EndsWith(".rar") || this.DownloadUri.EndsWith(".7z"))
            {
                Log.Error("Download Uri cannot be a compressed file. Please link to the page where the user can manually download the mod.");
                isValid = false;
            }
            return isValid;
        }

        public float DrawHost(float xMin, float y, float width)
        {
            Widgets.Label(new Rect(xMin, y, 100, 32), "ModSync.AboutUri".Translate());
            this.AboutUri = Widgets.TextField(new Rect(xMin + 110, y, 300, 32), this.AboutUri);
            y += 40;

            Widgets.Label(new Rect(xMin, y, 100, 32), "ModSync.DownloadUri".Translate());
            this.DownloadUri = Widgets.TextField(new Rect(xMin + 110, y, 300, 32), this.DownloadUri);
            y += 40;

            return y;
        }

        public void LoadFromXml(XmlNode parentNode)
        {
            foreach (XmlElement el in parentNode.ChildNodes)
            {
                switch (el.Name)
                {
                    case "AboutUri":
                        this.AboutUri = el.InnerText;
                        break;
                    case "ModSyncUri":
                        this.ModSyncUri = el.InnerText;
                        break;
                    case "DownloadUri":
                        this.DownloadUri = el.InnerText;
                        break;
                }
            }
        }

        public void WriteToXml(XmlDocument xml, XmlElement parent)
        {
            XmlDocUtil.AddElement(xml, parent, "AboutUri", this.AboutUri);
            XmlDocUtil.AddElement(xml, parent, "ModSyncUri", this.ModSyncUri);
            XmlDocUtil.AddElement(xml, parent, "DownloadUri", this.DownloadUri);
        }
    }
}