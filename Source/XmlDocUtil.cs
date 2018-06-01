using System;
using System.Xml;

namespace ModSyncRW
{
    static class XmlDocUtil
    {
        internal static void AddElement(XmlDocument xml, XmlElement parent, string tag, string value)
        {
            XmlElement el = xml.CreateElement(tag);
            el.InnerText = value;
            parent.AppendChild(el);
        }
    }
}
