using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Verse;

namespace ModSyncRW
{
    class FileUtil
    {
        private const string ASSEMBLIES_DIRECTORY = "Assemblies";

        private readonly static HashSet<string> DllsToExclude = new HashSet<string>();

        static FileUtil()
        {
            DllsToExclude.Add(@"$HugsLibChecker.dll");
            DllsToExclude.Add(@"0Harmony.dll");
            DllsToExclude.Add(@"SaveStorageSettingsUtil.dll");
            DllsToExclude.Add(@"ModSyncNinjaApiBridge.dll");
        }

        public static string GetVersionFromDll(ModMetaData mod)
        {
            string assemblyDirectory = mod.RootDir + "/" + ASSEMBLIES_DIRECTORY;
            if (!Directory.Exists(assemblyDirectory))
                return null;

            string foundDll = null;
            foreach(string dll in Directory.GetFiles(assemblyDirectory))
            {
                if (!DllsToExclude.Contains(Path.GetFileName(dll)))
                {
                    foundDll = dll;
                    break;
                }
            }

            if (!String.IsNullOrEmpty(foundDll))
            {
                FileVersionInfo info = FileVersionInfo.GetVersionInfo(@foundDll);
                return info.FileVersion;
            }

            return null;
        }
    }
}
