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

        public bool IsInSync => this.RemoteInfo == null || this.LocalInfo.Version.Equals(this.RemoteInfo.Version);

        public string TruncatedName => this.Mod.Name.Truncate(NAME_TRUNCATE_LENGTH);
    }
}
