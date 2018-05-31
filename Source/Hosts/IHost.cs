using System.Xml;

namespace ModSyncRW.Hosts
{
    interface IHost
    {
        string DownloadPageUri { get; }
        string ModSyncXmlUri { get; }
        void WriteHostInfo(XmlDocument xml, XmlNode parentNode);
    }
}
