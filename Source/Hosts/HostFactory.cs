using System;
using Verse;

namespace ModSyncRW.Hosts
{
    static class HostFactory
    {
        public static bool TryCreateHost(string type, out IHost host)
        {
            HostEnum hostEnum;
            try
            {
                hostEnum = (HostEnum)Enum.Parse(typeof(HostEnum), type, true);
            }
            catch
            {
                Log.Error("Unknown Host type of [" + type + "]");
                host = null;
                return false;
            }
            return TryCreateHost(hostEnum, out host);
        }

        public static bool TryCreateHost(HostEnum hostEnum, out IHost host)
        {
            host = null;

            if (hostEnum == HostEnum.Github)
            {
                host = new GithubHost();
            }
            else if (hostEnum == HostEnum.Direct)
            {
                host = new DirectHost();
            }
            else if (hostEnum == HostEnum.Gitlab)
            {
                host = new GitlabHost();
            }

            return host != null;
        }
    }
}
