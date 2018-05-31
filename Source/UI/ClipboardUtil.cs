using UnityEngine;

namespace ModSyncRW.UI
{
    static class ClipboardUtil
    {
        public static string ClipBoard
        {
            get
            {
                return GUIUtility.systemCopyBuffer;
            }
            set
            {
                GUIUtility.systemCopyBuffer = value;
            }
        }
    }
}
