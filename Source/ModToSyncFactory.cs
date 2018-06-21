using ModSyncRW.Hosts;
using System;
using System.IO;
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

            string f = mod.RootDir + "/About/ModSync.xml";
            if (File.Exists(f))
            {
                try
                {
                    XmlDocument xml = new XmlDocument();
                    xml.Load(f);

                    if (ReadModSync(xml, mod.Name, out info, out host))
                    {
                        modToSync = new ModToSync(mod, info);
                        modToSync.Host = host;
                        return true;
                    }
                }
                catch { }
            }

            f = mod.RootDir + "/About/Version.xml";
            if (File.Exists(f))
            {
                try
                {
                    XmlDocument xml = new XmlDocument();
                    xml.Load(f);

                    if (ReadVersion(xml, mod.Name, out info, out host))
                    {
                        modToSync = new ModToSync(mod, info);
                        modToSync.Host = host;
                        return true;
                    }
                }
                catch { }
            }
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
                if (TryGetNode(xml, "ModSyncNinjaData", out parentNode))
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
            IHost host = null;
            XmlAttribute nameAttr = parentNode.Attributes["name"];
            if (nameAttr != null)
            {
                if (HostFactory.TryCreateHost(nameAttr.Value, out host))
                {
                    host.LoadFromXml(parentNode);
                }
            }
            return host;
        }

        private static bool TryGetNode(XmlDocument xml, string nodeName, out XmlNode node)
        {
            foreach (XmlNode n in xml.ChildNodes)
            {
                if (n.Name.Equals(nodeName))
                {
                    node = n;
                    return true;
                }
            }
            node = null;
            return false;
        }



        public static bool ReadVersion(XmlDocument xml, string modName, out ModSyncInfo info, out IHost host)
        {
#if MOD_TO_SYNC_FACTORY_VERSION
            Log.Warning("Begin ModToSyncFacotry.ReadModSync " + modName);
#endif
            info = null;
            host = null;
            string name = modName;
            string version = null;
            string ownerProject = null;
            try
            {
                XmlNode parentNode;
                if (TryGetNode(xml, "VersionData", out parentNode))
                {
#if TRACE && MOD_TO_SYNC_FACTORY_VERSION
                    Log.Message("    ModSync Tags:");
#endif
                    foreach (XmlNode n in parentNode.ChildNodes)
                    {
#if TRACE && MOD_TO_SYNC_FACTORY_VERSION
                        Log.Message("        Tag: " + n.Name + "   InnerText: " + n.InnerText);
#endif
                        switch (n.Name)
                        {
                            case "overrideVersion":
#if TRACE && MOD_TO_SYNC_FACTORY_VERSION
                        Log.Message("            overrideVersion found");
#endif
                                version = n.InnerText;
                                break;
                            case "gitHubRepository":
#if TRACE && MOD_TO_SYNC_FACTORY_VERSION
                        Log.Message("            gitHubRepository found");
#endif
                                ownerProject = n.InnerText;
                                break;
                            default:
                                Log.Warning("Unknown field [" + n.Name + "] for mod [" + modName + "]");
                                break;
                        }
                    }
                }
                info = new ModSyncInfo("", name, version, false);
                host = new HugsLibVersionHost()
                {
                    OwnerProject = ownerProject
                };
#if MOD_TO_SYNC_FACTORY_VERSION
                Log.Warning("End ModToSyncFacotry.ReadModSync true\nInfo:" + info.ToString());
#endif
                return true;
            }
            catch (Exception e)
            {
                Log.Error("Failed to create ModSyncInfo for mod [" + modName + "]. " + e.GetType().Name + " " + e.Message);
            }
#if MOD_TO_SYNC_FACTORY_VERSION
            Log.Warning("End ModToSyncFacotry.ReadModSync false");
#endif
            return false;
        }
    }
}
