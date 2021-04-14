using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using Verse;

namespace ModSyncRW.Hosts
{
    class ManifestHost : IHost
    {
        private string downloadPageUri;
        private string aboutXmlUri;
        private string manifestXmlUri;

        public ManifestHost(string aboutXmlUri, string manifestXmlUri, string downloadPageUri)
        {
            this.aboutXmlUri = aboutXmlUri;
            this.manifestXmlUri = manifestXmlUri;
            this.downloadPageUri = downloadPageUri;
        }

        public HostEnum Type => HostEnum.Direct;

        public string DownloadPageUri => downloadPageUri;

        public string AboutXmlUri => aboutXmlUri;

        public string ModSyncXmlUri => manifestXmlUri;

        public void LoadFromXml(XmlNode parentNode)
        {
            foreach (XmlElement el in parentNode.ChildNodes)
            {
                switch (el.Name)
                {
                    case "manifestUri":
                        manifestXmlUri = el.InnerText;
                        int i = manifestXmlUri.LastIndexOf("/");
                        if (i != -1)
                            aboutXmlUri = manifestXmlUri.Substring(0, i) + "/About.xml";
                        break;
                    case "downloadUri":
                        downloadPageUri = el.InnerText;
                        break;
                }
            }
        }
    }
}
