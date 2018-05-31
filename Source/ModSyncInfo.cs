namespace ModSyncRW
{
    class ModSyncInfo
    {
        public readonly string Id;
        public string ModName;
        public string Version;
        public bool IsSaveBreaking;

        public ModSyncInfo(string modName, string version)
        {
            this.Id = System.Guid.NewGuid().ToString();
            this.ModName = modName;
            this.Version = version;
            this.IsSaveBreaking = false;
        }

        public ModSyncInfo (string id, string modName, string version, bool isSaveBreaking)
        {
            this.Id = id;
            this.ModName = modName;
            this.Version = version;
            this.IsSaveBreaking = isSaveBreaking;
        }

        public override string ToString()
        {
            return
                "ModSyncInfo:" +
                "\n    Id: " + Id +
                "\n    ModName: " + ModName +
                "\n    Version: " + Version +
                "\n    IsSaveBreaking: " + IsSaveBreaking;
        }
    }
}
