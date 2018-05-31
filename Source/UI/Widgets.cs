using UnityEngine;
using Verse;

namespace ModSyncRW.UI
{
    class Spinner
    {
        // 60 = 1 second. 120 = 2 seconds. Etc
        const int MAX = 360;
        private int frame = 0;

        public string GetSpinnerDots()
        {
            // ◰ ◳ ◲ ◱
            if (this.frame < 120)
                return @".  ";
            if (this.frame < 240)
                return @".. ";
            return @"...";
        }

        public void OnDoWindowContents()
        {
            ++this.frame;
            if (this.frame > MAX)
                this.frame = 0;
        }
    }

    public enum CurrentSyncState
    {
        Unchecked,
        ClientOffline,
        Done,
        RequestStarted
    }

    public enum InternetConnectivity
    {
        Unchecked,
        Offline,
        Online
    }

    [StaticConstructorOnStartup]
    public static class NetworkIndicator
    {
        public static Texture2D Working;
        public static Texture2D Synced;
        public static Texture2D Error;
        public static Texture2D Offline;

        static NetworkIndicator()
        {
            Working = ContentFinder<Texture2D>.Get("UI/Indicators/networkIndicator_working");
            Synced = ContentFinder<Texture2D>.Get("UI/Indicators/networkIndicator_requestcompleted");
            Error = ContentFinder<Texture2D>.Get("UI/Indicators/networkIndicator_error");
            Offline = ContentFinder<Texture2D>.Get("UI/Indicators/networkIndicator_offline");
        }
    }
}
