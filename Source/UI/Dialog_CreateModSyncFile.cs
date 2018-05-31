using ModSyncRW.Hosts;
using System;
using System.Collections.Generic;
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

        public Dialog_CreateModSyncFile(ModToSync mod, string assemblyVersion)
        {
            this.Mod = mod;
            this.AssemblyVersion = assemblyVersion;

            if (mod.Host != null)
            {
                if (mod.Host is GithubHost)
                    this.selectedHost = "Github";
            }

            this.Hosts.Add("Github");

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
            Widgets.Label(new Rect(LEFT, y, lineLength, 64), this.Mod.Mod.Name);
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
            Widgets.CheckboxLabeled(new Rect(LEFT, y, lineLength - 100, 32), "ModSync.IsSaveBreaking".Translate(), ref this.Mod.LocalInfo.IsSaveBreaking);
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
            y = this.DrawHost(LEFT + 20, y, lineLength);
            y += 40;

            // Submit button
            bool canSubmit = !String.IsNullOrEmpty(this.Mod.LocalInfo.Version) && this.Mod.Host != null;
            if (Widgets.ButtonText(new Rect(LEFT + buttonBuffer, y, 65, 32), "Confirm".Translate(), canSubmit, false, canSubmit)) // Using Confirm as it's translated already in the base game
            {
                base.Close();
            }
            // Cancel Button
            if (Widgets.ButtonText(new Rect((int)(rect.width * 0.5f) + buttonBuffer, y, 65, 32), "CancelButton".Translate())) // Using CancelButton as it's translated already in the base game
            {
                base.Close();
            }
        }

        private float DrawHost(float xMin, float y, float width)
        {
            if ("Github".Equals(this.selectedHost))
            {
                GithubHost host = this.Mod.Host as GithubHost;
                Widgets.Label(new Rect(xMin, y, 100, 32), "ModSync.Owner".Translate());
                host.Owner = Widgets.TextField(new Rect(xMin + 110, y, 200, 32), host.Owner);
                y += 40;

                Widgets.Label(new Rect(xMin, y, 100, 32), "ModSync.Project".Translate());
                host.Project = Widgets.TextField(new Rect(xMin + 110, y, 200, 32), host.Project);
                y += 40;

                Widgets.Label(new Rect(xMin, y, 100, 32), "ModSync.DownloadPage".Translate());
                if (Widgets.ButtonText(new Rect(xMin + 110, y, 100, 32), host.DownloadLocation.ToString().Translate()))
                {
                    List<FloatMenuOption> options = new List<FloatMenuOption>();
                    foreach(DownloadLocation dl in Enum.GetValues(typeof(DownloadLocation)))
                    {
                        options.Add(new FloatMenuOption(dl.ToString().Translate(), delegate ()
                        {
                            host.DownloadLocation = dl;
                        }));
                    }
                    Find.WindowStack.Add(new FloatMenu(options));
                }
                y += 40;

                Widgets.Label(new Rect(xMin, y, 100, 32), "ModSync.Branch".Translate());
                host.Branch = Widgets.TextField(new Rect(xMin + 110, y, 200, 32), host.Branch);
                y += 40;
            }
            return y;
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
 