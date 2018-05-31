using System;
using System.Reflection;
using Harmony;
using UnityEngine;
using Verse;
using RimWorld;
using System.IO;
using ModSyncRW.UI;

namespace ModSyncRW
{
    [StaticConstructorOnStartup]
    class Main
    {
        static Main()
        {
            var harmony = HarmonyInstance.Create("com.modsyncninja.rimworld");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [HarmonyPatch(typeof(MainMenuDrawer), "DoMainMenuControls")]
    static class Patch_MainMenuDrawer_DoMainMenuControls
    {
        static void Postfix(Rect rect, bool anyMapFiles)
        {
            if (Current.ProgramState == ProgramState.Entry)
            {
                Text.Font = GameFont.Small;
                int y = (int)((rect.yMax + rect.yMin) * .49f);
                Rect r = new Rect(rect.xMax - 598, y + 2, 140, 45);
                if (Widgets.ButtonText(r, "ModSync".Translate(), true, false, true))
                {
                    Find.WindowStack.Add((Window)new Dialog_CheckMods());
                }
            }
        }
    }

    [HarmonyPatch(typeof(Page_ModsConfig), "DoWindowContents")]
    static class Patch_Page_ModsConfig_DoWindowContents
    {
        static void Postfix(Page_ModsConfig __instance, Rect rect)
        {
            ModMetaData selectedMod = __instance.selectedMod;
            if (Prefs.DevMode && selectedMod != null && !selectedMod.IsCoreMod && selectedMod.Source == ContentSource.LocalFolder)
            {
                Rect buttonRect = new Rect(580f, rect.height - 95f, 200f, 40f);

                ModToSync mod = null;
                if (File.Exists(selectedMod.RootDir + "/About/ModSync.xml"))
                {
                    // Draw the "Update ModSync" button
                    if (Widgets.ButtonText(buttonRect, "ModSync.UpdateModSyncFile".Translate()))
                    {
                        if (!ModToSyncFactory.TryCreateModToSync(selectedMod, out mod))
                        {
                            mod = null;
                            Log.Error("Could not open ModSync.xml file for [" + selectedMod.Name + "]");
                        }
                    }
                }
                else // No ModSync.xml for the mod
                {
                    if (Widgets.ButtonText(buttonRect, "ModSync.CreateModSyncFile".Translate()))
                    {
                        mod = new ModToSync(selectedMod, new ModSyncInfo(selectedMod.Name, "1.0.0.0"));
                    }
                }

                if (mod != null)
                {
                    string assemblyVersion = FileUtil.GetVersionFromDll(selectedMod);
                    Dialog_CreateModSyncFile dialog = new Dialog_CreateModSyncFile(mod, assemblyVersion);
                    Find.WindowStack.Add(dialog);
                }
            }
        }
    }

        /*private static void HandleUpdateOnModSync(ModMetaData selectedMod, string version)
        {
            isSubmitting = true;

            string modDirectoryName = selectedMod.RootDir.Name;
            string json = NetworkManager.GenerateServerRequestString(modDirectoryName, version);
            string request = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json));

            NetworkManager.CreateRequestHttpRequest(request, null, (string responseStr, bool success, string errorCode) =>
            {
                try
                {
                    if (success)
                    {
                        string remoteVerison = null;
                        // Get the remote mod's version from the repsonse
                        if (TryParseRemoteModDataVersion(responseStr, out remoteVerison))
                        {
                            string assemblyVersion = FileUtil.GetVersionFromDll(selectedMod);

                            UpdateModRequest r = new UpdateModRequest();

                            // Open a dialog box asking for the security token for the mod
                            Dialog_UpdateMod_Window dialog = new Dialog_UpdateMod_Window(selectedMod, version, remoteVerison, assemblyVersion);
                            Find.WindowStack.Add(dialog);
                        }
                        //else - A log message has already been written by TryParseRemoteMOdDataVersion
                    }
                    else
                    {
                        Log.Error("Failed to get the mod's data from the ModSync.ninja server.");
                    }
                }
                finally
                {
                    isSubmitting = false;
                }
            });
        }*/

        /*// <summary>
        /// Converts the given version string into an int.
        /// Examples:
        /// 0.18.1 will return into 181
        /// 0.1.8.B will return 1801
        /// 0.1.8.b will return 1801 as well
        /// For the last two [A-Z] and [a-z] are converted to 00-26
        /// In all cases periods are ignored
        /// </summary>
        /// <param name="version">The version string to convert into a number</param>
        /// <returns>A number representing the given version</returns>
        private static int GetVersionAsNumber(string version)
        {
            int i = 0;
            foreach (Char c in version.ToUpper().ToCharArray())
            {
                // for each iteration increase the magnitude by 10
                // This allows a string of "0.18" to become 18
                i *= 10;
                if (c >= 48 && c <= 57)
                {
                    // 0 - 9
                    i += (c - 48);
                }
                else if (c != '.')
                {
                    // This is a case where it's a special character (not a number or period)
                    if (c >= 65 && c <= 90)
                    {
                        // A - Z
                        // In this case the numbers count for two digits. Increase i's magnitude by 10 again
                        // and add the letter's value. A will be 0. Z will be 26
                        // Thus 0.18M would become 1811 (M being 11)
                        i *= 10;
                        i += (c - 48);
                    }
                    else
                    {
                        // This is a really unsupported character.
                        // I doubt this will ever happen. I'm just going to increase the magnitude of i by 100 and hope for the best
                        i *= 100;
                        i += c;
                    }
                }
                // else Skip '.'
            }
            return i;
        }*/

        /*// <summary>
        /// Helper method to get the version number from the response string from the MoSync.ninja server
        /// </summary>
        /// <param name="responseStr">Response from the server</param>
        /// <param name="remoteVersion">The argument which will contain the remote mod's version</param>
        /// <returns>True if the version could be parsed from the repsonse. False if the version could not be parsed from the response.</returns>
        private static bool TryParseRemoteModDataVersion(string responseStr, out string remoteVersion)
        {
            remoteVersion = null;

            string responseData;
            try
            {
                responseData = Encoding.UTF8.GetString(Convert.FromBase64String(responseStr));
            }
            catch
            {
                // Failed to parse BASE 64
                Log.Error("Failed to parse the returned data from the server.");
                return false;
            }

            try
            {
                string[] rows = Regex.Split(responseData.Trim('\"'), "{%}");
                if (rows.Length == 0)
                {
                    Log.Error("Unable to find mod in ModSync.ninja.");
                    return false;
                }
                else if (rows.Length == 1)
                {
                    try
                    {
                        string rowData = Encoding.UTF8.GetString(Convert.FromBase64String(rows[0]));
                        Dialog_ModSync_Window.ModDetailsResponse rowDataParsed = JsonUtility.FromJson<Dialog_ModSync_Window.ModDetailsResponse>(rowData);
                        remoteVersion = rowDataParsed.V;
                        return true;

                    }
                    catch
                    {
                        Log.Error("Unable to read version from the mod data retrieved from ModSync.ninja.");
                    }
                }
                else
                {
                    Log.Error("More than one mod found, no update will be performed.");
                }
            }
            catch
            {
                // failed to parse JSON
                Log.Error("An internal problem occurred while parsing the json retrieved from the server.");
            }
            return false;
        }
    }*/

    //[HarmonyPatch]
    //[HarmonyPatch(typeof(Page_ModsConfig))]
    //[HarmonyPatch("DoWindowContents")]
    //public static class DoWindowContents_Patch
    //{

    //    public static bool Prefix(this Page_ModsConfig __instance, ref Rect rect)
    //    {
    //        //Find.WindowStack.Add((Window) new Page_ModsConfig()))
    //Rect mainRect = (Rect)AccessTools.Method(typeof(Page_ModsConfig), "GetMainRect").Invoke(__instance, new object[] { rect, 0f, true });
    //GUI.BeginGroup(mainRect);
    //Text.Font = GameFont.Small;
    //float num = 0f;
    //Rect rect2 = new Rect(17f, num, 316f, 30f);
    //if (Widgets.ButtonText(rect2, "OpenSteamWorkshop".Translate(), true, false, true))
    //{
    //    __instance.PreOpen();
    //    return true;
    //}
    //GUI.EndGroup();
    //        //Page_ModsConfig_Controller.DoWindowContents(__instance, rect);
    //        return false;
    //    }
    //}

    /*[HarmonyPatch(typeof(WindowStack))]
    [HarmonyPatch("Add")]
    [HarmonyPatch(new Type[] { typeof(Window) })]
    class Patch
    {
        static bool Prefix(Window window)
        {
            if (window.ToString().Equals("RimWorld.Page_ModsConfig"))
            {

                if (Find.WindowStack.currentlyDrawnWindow != null &&
                    Find.WindowStack.currentlyDrawnWindow is Dialog_ModMenuSelection) return true;
                Rect mainRect = new Rect(0f, 0f, 500f, 500f);

                GUI.BeginGroup(mainRect);

                Find.WindowStack.Add((Window)new Dialog_ModMenuSelection());

                GUI.EndGroup();


                return false;


            }
            return true;
        }
    }*/
}
