
namespace UniDownload.UniDownloadCore
{
    internal class UniLogger
    {
        public static void Log(string message) => UnityEngine.Debug.Log(message);

        public static void Error(string message) => UnityEngine.Debug.LogError(message);
    }
}