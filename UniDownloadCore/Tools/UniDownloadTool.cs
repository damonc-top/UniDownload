using System;

namespace UniDownload.UniDownloadCore
{
    internal static class UniDownloadTool
    {
        public static int GetRequestLifeTime()
        {
            return UniServiceContainer.Get<UniDownloadSetting>().RequestLifeTime;
        }
        
        // 获取热点时间戳
        public static int GetTime()
        {
#if UNITY_STANDALONE || UNITY_EDITOR
            return UnityEngine.Time.frameCount;
#else
            // 在非Unity环境下，使用系统时间戳
            return Environment.TickCount;
#endif
        }
    }
}