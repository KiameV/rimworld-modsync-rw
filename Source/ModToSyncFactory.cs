using ModSyncRW.Hosts;
using System;
using System.Xml;
using Verse;

namespace ModSyncRW
{
    static class ModToSyncFactory
    {
        public static bool TryCreateModToSync(ModMetaData mod, out ModToSync modToSync)
        {
            if (mod.OnSteamWorkshop || mod.IsCoreMod)
            {
                modToSync = null;
                return false;
            }
            ModSyncInfo info = null;
            IHost host = null;

            XmlDocument xml = new XmlDocument();
            try
            {
                xml.Load(mod.RootDir + "/About/ModSync.xml");

                if (ReadModSync(xml, mod.Name, out info, out host))
                {
                    modToSync = new ModToSync(mod, info);
                    modToSync.Host = host;
                    return true;
                }
            }
            catch { }
            modToSync = null;
            return false;
        }

        public static bool ReadModSync(XmlDocument xml, string modName, out ModSyncInfo info, out IHost host)
        {
#if MOD_TO_SYNC_FACTORY
            Log.Warning("Begin ModToSyncFacotry.ReadModSync " + modName);
#endif
            info = null;
            host = null;
            string id = null;
            string name = null;
            string version = null;
            bool isSaveBreaking = false;
            try
            {
                XmlNode parentNode;
                if (TryGetModSyncNode(xml, out parentNode))
                {
#if TRACE && MOD_TO_SYNC_FACTORY
                    Log.Message("    ModSync Tags:");
#endif
                    foreach (XmlNode n in parentNode.ChildNodes)
                    {
#if TRACE && MOD_TO_SYNC_FACTORY
                        Log.Message("        Tag: " + n.Name + "   InnerText: " + n.InnerText);
#endif
                        switch (n.Name)
                        {
                            case "ID":
                                id = n.InnerText;
                                break;
                            case "ModName":
                                name = n.InnerText;
                                break;
                            case "Version":
                                version = n.InnerText;
                                break;
                            case "SaveBreaking":
                                if (!bool.TryParse(n.InnerText, out isSaveBreaking))
                                {
                                    isSaveBreaking = false;
                                }
                                break;
                            case "Host":
                                host = CreateHost(modName, n);
                                break;
                            default:
                                Log.Warning("Unknown field [" + n.Name + "] for mod [" + modName + "]");
                                break;
                        }
                    }
                }
                info = new ModSyncInfo(id, name, version, isSaveBreaking);
#if MOD_TO_SYNC_FACTORY
                Log.Warning("End ModToSyncFacotry.ReadModSync true\nInfo:" + info.ToString());
#endif
                return true;
            }
            catch (Exception e)
            {
                Log.Error("Failed to create ModSyncInfo for mod [" + modName + "]. " + e.GetType().Name + " " + e.Message);
            }
#if MOD_TO_SYNC_FACTORY
            Log.Warning("End ModToSyncFacotry.ReadModSync false");
#endif
            return false;
        }

        private static IHost CreateHost(string modName, XmlNode parentNode)
        {
            XmlAttribute nameAttr = parentNode.Attributes["name"];
            if (nameAttr != null)
            {
                if (HostEnum.Github.ToString().Equals(nameAttr.Value))
                {
                    return new GithubHost(parentNode);
                }
                else
                {
                    Log.Warning("ModSyncRW: Unknown host [" + nameAttr.Value + "] for mod [" + modName + "]");
                }
            }
            return null;
        }

        private static bool TryGetModSyncNode(XmlDocument xml, out XmlNode node)
        {
            foreach (XmlNode n in xml.ChildNodes)
            {
                if (n.Name.Equals("ModSyncNinjaData"))
                {
                    node = n;
                    return true;
                }
            }
            node = null;
            return false;
        }
    }
}
