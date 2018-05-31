using System.Xml;

namespace ModSyncRW
{
    static class XmlDocUtil
    {
        public static void AddNodeToElement(XmlDocument doc, XmlNode parent, string tag, string value)
        {
            XmlNode n = doc.CreateNode(XmlNodeType.Element, tag, "");
            n.InnerText = value;
            parent.AppendChild(n);
        }
    }
}
