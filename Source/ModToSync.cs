using ModSyncRW.Hosts;
using Verse;

namespace ModSyncRW
{
    class ModToSync
    {
        public const float NAME_TRUNCATE_LENGTH = 220;

        public readonly ModMetaData Mod;
        public readonly ModSyncInfo LocalInfo;
        public ModSyncInfo RemoteInfo = null;
        public IHost Host = null;

        public bool RequestDone = false;

        public ModToSync(ModMetaData mod, ModSyncInfo localInfo)
        {
            this.Mod = mod;
            this.LocalInfo = localInfo;
        }

        public bool IsInSync => this.RemoteInfo == null || this.GetVersionAsInt(this.LocalInfo.Version) >= this.GetVersionAsInt(this.RemoteInfo.Version);

        public string TruncatedName => this.Mod.Name.Truncate(NAME_TRUNCATE_LENGTH);

        private int GetVersionAsInt(string version)
        {
            if (System.String.IsNullOrEmpty(version))
                return 0;
            int result = 0;
            for (int i = 0; i < version.Length; ++i)
            {
                char c = version[i];
                if (c >= '0' && c <= '9')
                {
                    result = result * 10 + ((int)c - 48);
                }
                else if (c >= 'a' && c <= 'z')
                {
                    c = char.ToUpper(c);
                    result = result * 100 + ((int)c - 65) + 20;
                }
                else if (c >= 'A' && c <= 'Z')
                {
                    result = result * 100 + ((int)c - 65) + 50;
                }
            }
            return result;
        }
    }
}
