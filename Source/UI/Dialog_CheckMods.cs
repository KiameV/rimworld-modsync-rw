using ModSyncRW.Hosts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;
using Verse;

namespace ModSyncRW.UI
{
    public class Dialog_CheckMods : Window
    {
        public override Vector2 InitialSize => new Vector2(725f, 65f + Mathf.Max(200f, 700f));

        private InternetConnectivity Status = InternetConnectivity.Unchecked;

        private List<ModToSync> modsToSync;
        private static Vector2 scrollPosition = Vector2.zero;
        private Spinner spinner = new Spinner();
        private string _userRequestStr = string.Empty;
        private string _errorCode = String.Empty;

        public Dialog_CheckMods()
        {
#if DIALOG_CHECK_MODS
            Log.Warning("Begin Dialog_CheckMods()");
#endif
            try
            {
                this.Status = InternetConnectivity.Unchecked;
                closeOnClickedOutside = false;

                this.FindModsToSync();

#if TRACE && DIALOG_CHECK_MODS
                Log.Message("    ModsToSync Count: " + ((modsToSync == null) ? "<null>" : modsToSync.Count.ToString()));
#endif
                if (this.modsToSync != null && this.modsToSync.Count > 0)
                {
#if TRACE && DIALOG_CHECK_MODS
                    Log.Message("    Check Internet Connection");
#endif
                    RestUtil.CheckForInternetConnectionAsync((bool result) =>
                    {
#if TRACE && DIALOG_CHECK_MODS
                        Log.Warning("Callback: " + result);
#endif
                        this.Status = (result) ? InternetConnectivity.Online : InternetConnectivity.Offline;
                        if (this.Status == InternetConnectivity.Online)
                        {
                            this.Resync();
                        }
                    });
                }
            }
#if TRACE && DIALOG_CHECK_MODS
            catch (Exception e)
            {
                Log.Error("Exception: " + e.GetType().Name + " " + e.Message);
            }
#else
            catch { }
#endif
#if DIALOG_CHECK_MODS
            Log.Warning("End Dialog_CheckMods()");
#endif
        }

        private void FindModsToSync()
        {
#if DIALOG_CHECK_MODS
            Log.Warning("Begin Dialog_CheckMods.FindModsToSync");
#endif
            this.modsToSync = new List<ModToSync>();
#if TRACE && DIALOG_CHECK_MODS
            Log.Message("    Mods:");
#endif
            foreach (ModMetaData modMetaData in ModsConfig.ActiveModsInLoadOrder)
            {
#if TRACE && DIALOG_CHECK_MODS
                Log.Message("        " + modMetaData.Name + "   OnSteamWorkshop: " + modMetaData.OnSteamWorkshop + "  IsCoreMod: " + modMetaData.IsCoreMod);
#endif
                if (modMetaData.OnSteamWorkshop == false && modMetaData.IsCoreMod == false)
                {
#if TRACE && DIALOG_CHECK_MODS
                    Log.Message("            Add to ModsToSync");
#endif
                    ModToSync mod;
                    if (ModToSyncFactory.TryCreateModToSync(modMetaData, out mod))
                    {
                        if (mod.Host != null)
                        {
                            this.modsToSync.Add(mod);
                        }
                    }
                }
            }
#if DIALOG_CHECK_MODS
            Log.Warning("End Dialog_CheckMods.FindModsToSync");
#endif
        }

        private void Resync()
        {
#if DIALOG_CHECK_MODS
            Log.Warning("Begin Dialog_CheckMods.Resync");
#endif
#if TRACE && DIALOG_CHECK_MODS
            Log.Message("    Mods:");
#endif
            foreach (ModToSync mod in this.modsToSync)
            {
#if TRACE && DIALOG_CHECK_MODS
                Log.Message("        " + mod.Mod.Name);
#endif
                if (mod.Host != null)
                {
                    RestUtil.GetModSyncXml(mod.Host.ModSyncXmlUri, (XmlDocument xml) =>
                    {
#if TRACE && DIALOG_CHECK_MODS
                        Log.Message("ModCheck Callback: " + mod.Mod.Name);
#endif
                        mod.RequestDone = true;
                        if (xml != null)
                        {
                            try
                            {
                                ModSyncInfo info;
                                IHost host;
                                if (ModToSyncFactory.ReadModSync(xml, mod.Mod.Name, out info, out host))
                                {
                                    mod.RemoteInfo = info;
                                }
                            }
                            catch { }
                        }
                    });
                }
                else
                {
                    mod.RequestDone = true;
                }
            }
#if DIALOG_CHECK_MODS
            Log.Warning("End Dialog_CheckMods.Resync");
#endif
        }

        protected Rect GetMainRect(Rect rect, float extraTopSpace = 0, bool ignoreTitle = false)
        {
            float y = 0.0f;
            if (!ignoreTitle)
                y = 45f + extraTopSpace;
            return new Rect(0.0f, y, rect.width, (float)((double)rect.height - 38.0 - (double)y - 17.0));
        }

        public override void DoWindowContents(Rect rect)
        {
            spinner.OnDoWindowContents();
            Rect outRect = new Rect(0.0f, 0.0f, rect.width + 30f, rect.height);

            float pushY = 70f;
            // sync mods title
            DoTopBar(outRect, pushY);
            // place labels ontop of table
            PlaceTableLabels(outRect, pushY);

            Rect rect4 = new Rect(0f, 40f + pushY, outRect.width - 30f, outRect.height - 110f - 60f - pushY);
            Rect safeUpdatesRect = new Rect(15f + 28f, rect4.height + rect4.y + 15f, rect4.width - 100f, 80f);
            GUI.DrawTexture(new Rect(15f, rect4.height + rect4.y + 15f, 24f, 24f), Widgets.CheckboxOnTex);
            Widgets.Label(safeUpdatesRect, " - " + "ModSync.ModNotBreakingSave".Translate());
            Widgets.DrawMenuSection(rect4);

            // long scroller
            //float height = (float)(ModLister.AllInstalledMods.Count<ModMetaData>() * 34 *60+ 300);
            // real scroller

            float height = (float)(ModLister.AllInstalledMods.Count<ModMetaData>() * 34 + 300);
            DrawModsToSync(rect4, height, this.modsToSync);

            AddCloseBtn(rect);
            //AddBrandLogo(rect);

            // reset UI settings
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
        }

        private void DoTopBar(Rect rect, float height)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0, 0, rect.width / 2, height), "ModSync.UpdateMods".Translate().ToUpper());
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(0, 30f, rect.width / 2, height - 30f), "ModSync.ShowingActiveMods".Translate().ToUpper());
            //CurrSyncState = CurrentSyncState.ModSyncError;
            if (this.Status == InternetConnectivity.Offline)
            {
                PlaceConnectionStatusBar(new Rect(rect.width / 2f, 0, rect.width / 2f, height), "ModSync.YouAreOffline".Translate(), NetworkIndicator.Offline);
            }
            /*else if (this.Status == CurrentSyncState.RequestStarted)
            {
                PlaceConnectionStatusBar(new Rect(rect.width / 2f, 0, rect.width / 2f, height), "ModSync.PleaseWait".Translate() + " " + _spinner.GetSpinnerDots(), NetworkIndicator.Working);
            }
            else if (this.Status == CurrentSyncState.Done)
            {
                PlaceConnectionStatusBar(new Rect(rect.width / 2f, 0, rect.width / 2f, height), "ModSync.Synced".Translate(), NetworkIndicator.Synced);
            }*/
        }

        private void DrawModsToSync(Rect inrect, float height, IEnumerable<ModToSync> mods)
        {

            Rect rect = new Rect(0f, 0f, inrect.width - 16f, height);
            Widgets.BeginScrollView(inrect, ref scrollPosition, rect, true);
            rect = rect.ContractedBy(4f);
            Listing_Standard modListUi = new Listing_Standard();
            modListUi.ColumnWidth = rect.width;
            modListUi.Begin(rect);
            /*int reorderableGroup = ReorderableWidget.NewGroup(delegate(int from, int to)
            {
                ModsConfig.Reorder(from, to);
                //SoundDefOf.TickHigh.PlayOneShotOnCamera(null);
            });*/
            int row = 0;
            foreach (ModToSync mod in mods)
            {
                this.DrawRow(modListUi, mod, row, 0);
                ++row;
            }
            modListUi.End();
            Widgets.EndScrollView();
        }

        /*private void AddBrandLogo(Rect rect)
        {

            Texture2D brand = ContentFinder<Texture2D>.Get("UI/logo", true);
            if (Widgets.ButtonImage(new Rect(rect.xMax - 235f, rect.yMax - 80f, 225f, 64f), brand, Color.white, Color.white))
            {
                NetworkManager.OpenModSyncUrl();
            }
        }*/

        private void AddCloseBtn(Rect rect)
        {
            // close btn
            if (Widgets.ButtonText(new Rect((rect.xMax / 2) - (80 / 2f), rect.yMax - 65f, 80f, 50f), "CloseButton".Translate(), true, false, true))
            {
                Find.WindowStack.TryRemove(this.GetType(), true);
            }
        }

        const float LABEL_X = 0;
        const float LOCAL_VERSION_X = 250;
        const float REMOTE_VERSON_X = 400;
        const float GET_UPDATE_X = 550;
        private void PlaceTableLabels(Rect inRect, float distanceFromTop)
        {
            Widgets.Label(new Rect(LABEL_X, distanceFromTop, inRect.width, 40f), "ModSync.ModName".Translate());
            Widgets.Label(new Rect(LOCAL_VERSION_X, distanceFromTop, inRect.width, 40f), "ModSync.LocalVersion".Translate());
            Widgets.Label(new Rect(REMOTE_VERSON_X, distanceFromTop, inRect.width, 40f), "ModSync.AvailableVersion".Translate());
            Widgets.Label(new Rect(GET_UPDATE_X, distanceFromTop, inRect.width, 40f), "ModSync.GetUpdate".Translate());
        }

        private void PlaceConnectionStatusBar(Rect rect, string connectionMessage, Texture2D connectionIndicator)
        {
            float imgWidth = 40f;
            float imgHeight = 20f;
            float paddingFromImg = 2f;
            GUI.DrawTexture(new Rect(rect.xMax - imgWidth - paddingFromImg, rect.y + 5f, imgWidth, imgHeight), connectionIndicator);
            Text.Anchor = TextAnchor.UpperRight;
            DoBoldLabel(new Rect(rect.x, rect.y, rect.width - (imgWidth * 2) - (paddingFromImg * 2), rect.height), connectionMessage);
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private void DoBoldLabel(Rect rect, string label)
        {
            Widgets.Label(rect, label);
            Widgets.Label(rect, label);
            Widgets.Label(rect, label);
        }

        private void DrawRow(Listing_Standard listing, ModToSync mod, int index, int reorderableGroup)
        {
            Rect rowRect = listing.GetRect(26f);

            Text.Anchor = TextAnchor.UpperLeft;
            string text = null;
            if (String.IsNullOrEmpty(mod.LocalInfo.Version) && mod.Host != null)
            {
                text = "ModSync.UnknownVersion".Translate();
            }
            else if (mod.Host == null)
            {
                text = "ModSync.NotSupportedByModSync".Translate();
            }
            if (!string.IsNullOrEmpty(text))
            {
                text = text.Replace("{mod_name}", mod.Mod.Name);
                TooltipHandler.TipRegion(rowRect, new TipSignal(text, text.GetHashCode()));
            }

            float num = rowRect.width - 24f;
            if (mod.Mod.Active)
            {
                Rect position = new Rect(rowRect.xMax - 48f + 2f, rowRect.y, 24f, 24f);
                //GUI.DrawTexture(position, TexButton.DragHash);
                num -= 24f;
            }
            Text.Font = GameFont.Small;

            // Label
            Widgets.Label(new Rect(LABEL_X, rowRect.y, 250, 24), mod.TruncatedName);

            //GUI.color = Color.red;
            // no version on client
            string localVersion = "???";
            if (!String.IsNullOrEmpty(mod.LocalInfo.Version))
            {
                localVersion = mod.LocalInfo.Version;
            }
            if (mod.RequestDone && !mod.IsInSync)
            {
                GUI.color = Color.red;
            }
            Widgets.Label(new Rect(LOCAL_VERSION_X, rowRect.y, 150, 24), localVersion);
            GUI.color = Color.white;

            // remote version
            string remoteVersion = spinner.GetSpinnerDots();
            if (mod.RequestDone)
            {
                if (mod.RemoteInfo != null &&
                    !string.IsNullOrEmpty(mod.RemoteInfo.Version))
                {
                    remoteVersion = mod.RemoteInfo.Version;
                    
                    //GUI.DrawTexture(new Rect(rect.xMax - 24f, rect.y, 24f, 24f), (Texture)texture2D);
                    // draw download if needed
                    if (!mod.IsInSync)
                    {
                        // not a save breaking mod
                        GUI.DrawTexture(new Rect(rowRect.xMax - 140f - 28f, rowRect.y + 4, 24f, 24f), (Texture2D)((mod.RemoteInfo.IsSaveBreaking) ? Widgets.CheckboxOffTex : Widgets.CheckboxOnTex));
                        if (Widgets.ButtonText(new Rect(rowRect.xMax - 140f, rowRect.y, 140f, 24f), "ModSync.Update".Translate(), true, false, true))
                        {
                            Application.OpenURL(mod.Host.DownloadPageUri);
                        }
                    }
                }
                else
                {
                    remoteVersion = "ModSync.NAModSync".Translate();
                }
            }
            Widgets.Label(new Rect(REMOTE_VERSON_X, rowRect.y, 150, 24), remoteVersion);

            GUI.color = Color.white;
        }
    }
}