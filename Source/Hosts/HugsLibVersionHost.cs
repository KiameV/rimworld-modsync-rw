using System.Xml;

namespace ModSyncRW.Hosts
{
    class HugsLibVersionHost : IHost
    {
        public string OwnerProject;
        public string Branch = "master";
        
        public HugsLibVersionHost() { }

        public HostEnum Type => HostEnum.Github;

        public string AboutXmlUri =>
            "https://raw.githubusercontent.com/" + this.OwnerProject + "/" + this.Branch + "/Mods/HugsLib/About/About.xml";

        public string ModSyncXmlUri => 
            "https://raw.githubusercontent.com/" + this.OwnerProject + "/" + this.Branch + "/Mods/HugsLib/About/Version.xml";

        public string DownloadPageUri
        {
            get
            {
                return "https://github.com/" + this.OwnerProject + "/releases/";
            }
        }

        public bool Validate()
        {
            return true;
        }

        public bool IsFormFilled => true;

        public float DrawHost(float xMin, float y, float width)
        {
            return y;
        }

        public void LoadFromXml(XmlNode parentNode)
        {

        }

        public void WriteToXml(XmlDocument xml, XmlElement parent)
        {

        }
    }
}