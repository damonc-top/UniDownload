
using System;

namespace UniDownload.UniDownloadCore
{
    internal class UniLogger
    {
        public static void Log(string message)
        {
#if UNITY_STANDALONE || UNITY_EDITOR
            UnityEngine.Debug.Log(message);
#else
            Console.WriteLine($"[LOG] {DateTime.Now:HH:mm:ss.fff} - {message}");
#endif
        }

        public static void Error(string message)
        {
#if UNITY_STANDALONE || UNITY_EDITOR
            UnityEngine.Debug.LogError(message);
#else
            Console.WriteLine($"[ERROR] {DateTime.Now:HH:mm:ss.fff} - {message}");
#endif
        }
    }
}