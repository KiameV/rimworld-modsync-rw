using System.Xml;

namespace ModSyncRW.Hosts
{
    interface IHost
    {
        HostEnum Type { get; }
        string DownloadPageUri { get; }
        string AboutXmlUri { get; }
        string ModSyncXmlUri { get; }
        float DrawHost(float v, float y, float lineLength);
        void WriteToXml(XmlDocument xml, XmlElement parent);
    }
}
