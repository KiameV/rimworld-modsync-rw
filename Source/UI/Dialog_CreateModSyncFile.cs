using ModSyncRW.Hosts;
using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using Verse;

namespace ModSyncRW.UI
{
    internal class Dialog_CreateModSyncFile : Window
    {
        public override Vector2 InitialSize => new Vector2(500, 660);

        private readonly ModToSync Mod;
        private readonly string AssemblyVersion;

        private readonly List<string> Hosts = new List<string>();
        private string selectedHost = "";

        private bool isHostValid = false;

        public Dialog_CreateModSyncFile(ModToSync mod, string assemblyVersion)
        {
            this.Mod = mod;
            this.AssemblyVersion = assemblyVersion;

            foreach (HostEnum h in Enum.GetValues(typeof(HostEnum)))
            {
                this.Hosts.Add(h.ToString());
            }

            if (mod.Host != null)
            {
                this.selectedHost = mod.Host.Type.ToString();
                this.isHostValid = true;
            }

            closeOnClickedOutside = false;
        }

        public override void DoWindowContents(Rect rect)
        {
            const float LEFT = 25;
            const float BUTTON_LENGTH = 65;
            float lineLength = (int)rect.xMax - LEFT * 2;
            float buttonBuffer = (int)(((int)(lineLength * 0.5f) - BUTTON_LENGTH) * 0.5f);
            float y = 0;

            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(LEFT, y, lineLength, 64), "ModSync.UpdateOnModSyncNinja".Translate());
            Text.Font = GameFont.Small;
            y += 52;

            // Label saying "Update Mod <mod name>"
            Widgets.Label(new Rect(LEFT, y, lineLength, 64), this.Mod.LocalInfo.ModName);
            y += 42;

            // Label saying "Previous version: <version>
            Widgets.Label(new Rect(LEFT, y, lineLength, 64), "ModSync.PreviousVersion".Translate().Replace("{previous_version}", this.Mod.LocalInfo.Version));
            y += 40;

            // User input for new version
            Widgets.Label(new Rect(LEFT, y, 80, 32), "ModSync.NewVersion".Translate());
            this.Mod.LocalInfo.Version = Widgets.TextField(new Rect(LEFT + 100, y, lineLength - 100, 32), this.Mod.LocalInfo.Version).Trim();
            y += 42;

            // Quick copy version buttons
            Widgets.Label(new Rect(LEFT, y, 80, 32), "ModSync.QuickCopy".Translate());
            string v = this.GetNextVersion(this.Mod.LocalInfo.Version);
            if (Widgets.ButtonText(new Rect(LEFT + 100, y, 80, 32), v))
            {
                this.Mod.LocalInfo.Version = v;
            }
            if (!String.IsNullOrEmpty(this.AssemblyVersion))
            {
                if (Widgets.ButtonText(new Rect(LEFT + 200, y, 80, 32), this.AssemblyVersion))
                {
                    this.Mod.LocalInfo.Version = this.AssemblyVersion;
                }
            }
            y += 42;

            // Is Save Breaking
            Widgets.Label(new Rect(LEFT, y, 120, 32), "ModSync.IsSaveBreaking".Translate());
            Widgets.Checkbox(new Vector2(LEFT + 140, y + 4), ref this.Mod.LocalInfo.IsSaveBreaking);
            y += 40;

            // Host Selection
            Widgets.Label(new Rect(LEFT, y, 80, 32), "ModSync.Host".Translate());
            if (Widgets.ButtonText(new Rect(LEFT + 100, y, 150, 32), this.selectedHost))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                foreach (string host in this.Hosts)
                {
                    options.Add(new FloatMenuOption(host, delegate
                    {
                        this.selectedHost = host;
                        this.Mod.Host = new GithubHost();
                    }));
                }
                Find.WindowStack.Add(new FloatMenu(options));
            }
            y += 40;
            if (this.Mod.Host != null)
            {
                y = this.Mod.Host.DrawHost(LEFT + 20, y, lineLength);
                if (Widgets.ButtonText(new Rect(LEFT + 20, y, 100, 32), "ModSync.Validate".Translate()))
                {
                    // Going to allow user to always create ModSync file even if validation fails
                    this.isHostValid = true;
                    RestUtil.GetAboutXml(this.Mod.Host.AboutXmlUri, delegate(bool found)
                    {
                        if (found)
                        {
                            ;// this.isHostValid = true;
                        }
                        else
                        {
                            Log.Warning(this.Mod.Host.AboutXmlUri);
                            Log.Error("ModSync.UnableToAboutSyncFile".Translate());
                        }
                    });
                }
            }
            y += 40;

            // Submit button
            bool canSubmit = !String.IsNullOrEmpty(this.Mod.LocalInfo.Version) && this.Mod.Host != null && this.isHostValid;
            if (Widgets.ButtonText(new Rect(LEFT + buttonBuffer, y, 65, 32), "Confirm".Translate(), canSubmit, false, canSubmit)) // Using Confirm as it's translated already in the base game
            {
                XmlDocument xml = new XmlDocument();
                xml.AppendChild(xml.CreateXmlDeclaration("1.0", "utf-8", null));

                XmlElement root = xml.CreateElement("ModSyncNinjaData");
                xml.AppendChild(root);

                XmlElement el = xml.CreateElement("ID");
                el.InnerText = this.Mod.LocalInfo.Id;
                root.AppendChild(el);
                
                el = xml.CreateElement("ModName");
                el.InnerText = this.Mod.LocalInfo.ModName;
                root.AppendChild(el);

                el = xml.CreateElement("Version");
                el.InnerText = this.Mod.LocalInfo.Version;
                root.AppendChild(el);

                el = xml.CreateElement("SaveBreaking");
                el.InnerText = this.Mod.LocalInfo.IsSaveBreaking.ToString();
                root.AppendChild(el);

                this.WriteToXml(xml, root);
                
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.NewLineChars = "\n";
                settings.Indent = true;
                using (XmlWriter writer = XmlWriter.Create(this.Mod.Mod.RootDir + "/About/ModSync.xml", settings))
                {
                    xml.WriteTo(writer);
                }

                base.Close();
            }
            // Cancel Button
            if (Widgets.ButtonText(new Rect((int)(rect.width * 0.5f) + buttonBuffer, y, 65, 32), "CancelButton".Translate())) // Using CancelButton as it's translated already in the base game
            {
                base.Close();
            }
        }

        private void WriteToXml(XmlDocument xml, XmlElement parent)
        {
            if (this.Mod.Host != null)
            {
                XmlElement hostEl = xml.CreateElement("Host");
                XmlAttribute at = xml.CreateAttribute("name");
                at.Value = this.selectedHost;
                hostEl.Attributes.Append(at);
                parent.AppendChild(hostEl);

                this.Mod.Host.WriteToXml(xml, hostEl);
            }
        }

        private string GetNextVersion(string version)
        {
            if (String.IsNullOrEmpty(version))
                return "0";
            char c = version[version.Length - 1];
            if (c >= '0' && c <= '9')
            {
                List<char> chars = new List<char>();
                int end = version.Length - 1;
                while (end >= 0 && version[end] >= '0' && version[end] <= '9')
                {
                    chars.Add(version[end]);
                    --end;
                }

                int num = 0;
                for (int i = chars.Count - 1; i >= 0; --i)
                {
                    if (i != chars.Count - 1)
                        num *= 10;
                    num += (int)char.GetNumericValue(chars[i]);
                }
                ++num;
                if (end == -1)
                {
                    return num.ToString();
                }
                return version.Substring(0, end + 1) + num.ToString();
            }
            return version;
        }
    }
}
 