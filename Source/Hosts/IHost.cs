using System.Xml;

namespace ModSyncRW.Hosts
{
    interface IHost
    {
        /// <summary>
        /// The type of Host
        /// </summary>
        HostEnum Type { get; }
        /// <summary>
        /// The uri to the page from where the mod can be downloaded
        /// </summary>
        string DownloadPageUri { get; }
        /// <summary>
        /// The uri to the About.xml
        /// Example: https://github.com/KiameV/rimworld-modsync-rw/blob/master/About/About.xml
        /// </summary>
        string AboutXmlUri { get; }
        /// <summary>
        /// The uri to the ModSync.xml
        /// Example: https://github.com/KiameV/rimworld-modsync-rw/blob/master/About/ModSync.xml
        /// </summary>
        string ModSyncXmlUri { get; }
        /// <summary>
        /// Draw the entry fields the user will need to fill out for the ModSync lookup to work
        /// </summary>
        /// <param name="xMin">Left-most x position any field should be drawn from</param>
        /// <param name="y">The initial y position</param>
        /// <param name="width">Width of the box that can be drawn in</param>
        /// <returns>The next y from which to add the next element</returns>
        float DrawHost(float xMin, float y, float width);
        /// <summary>
        /// Load all data from the XmlNode to properly populate this Host instance
        /// </summary>
        /// <param name="parent">The child elements will be the fields to load from</param>
        void LoadFromXml(XmlNode parent);
        /// <summary>
        /// Write the required fields into the XmlDocument
        /// </summary>
        /// <param name="xml">The XmlDocument to write the fields to (used to create XmlElements)</param>
        /// <param name="parent">The parent XmlElement to add fields to as children</param>
        void WriteToXml(XmlDocument xml, XmlElement parent);
    }
}
