using System;
using System.Xml;

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
        
        public void WriteHostInfo(XmlDocument xml, XmlNode parent)
        {
            XmlDocUtil.AddNodeToElement(xml, parent, "Owner", this.Owner);
            XmlDocUtil.AddNodeToElement(xml, parent, "Project", this.Project);
            XmlDocUtil.AddNodeToElement(xml, parent, "DownloadFrom", this.DownloadLocation.ToString());
            XmlDocUtil.AddNodeToElement(xml, parent, "Branch", this.Branch);
        }
    }
}