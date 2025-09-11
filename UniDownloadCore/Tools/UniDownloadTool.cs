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
            return UnityEngine.Time.frameCount;
        }
    }
}