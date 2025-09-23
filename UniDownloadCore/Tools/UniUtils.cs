using System;
using System.IO;

namespace UniDownload.UniDownloadCore
{
    internal static class UniUtils
    {
        // 获取源站URL
        public static string GetBaseURL()
        {
            return UniServiceContainer.Get<UniDownloadSetting>().BaseURL;
        }
        
        // 获取正式保存根路径
        public static string GetBaseSavePath()
        {
            return UniServiceContainer.Get<UniDownloadSetting>().BaseSavePath;
        }

        // 获取临时保存根路径
        public static string GetTempSavePath()
        {
            return UniServiceContainer.Get<UniDownloadSetting>().TeamSavePath;
        }

        // 获取全局最大并发数
        public static int GetMaxParallel()
        {
            return UniServiceContainer.Get<UniDownloadSetting>().MaxParallel;
        }

        // 超时时间
        public static int GetTimeOut()
        {
            return UniServiceContainer.Get<UniDownloadSetting>().TimeOut;
        }
        
        // 获取request的可存活时间
        public static int GetRequestLifeTime()
        {
            return UniServiceContainer.Get<UniDownloadSetting>().RequestLifeTime;
        }

        // 获取文件名的MD5
        public static string GetFileNameMD5(string fileName)
        {
            return fileName;
        }

        // 获取文件内容MD5
        public static string GetFileContentMD5(string filePath)
        {
            return filePath;
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
        
        // 序列化到字节数组
        public static byte[] SerializeUniFileInfo(UniFileInfo info)
        {
            // TODO 复杂的自定义二进制序列化，这里用简化版JSON
            string value = UnityEngine.JsonUtility.ToJson(info);
            return System.Text.Encoding.UTF8.GetBytes(value);
        }
        
        // 从字节数组反序列化文件信息
        public static UniFileInfo DeserializeUniFileInfo(byte[] data)
        {
            // TODO 复杂的自定义二进制反序列化，这里用简化版JSON
            string json = System.Text.Encoding.UTF8.GetString(data);
            return UnityEngine.JsonUtility.FromJson<UniFileInfo>(json);
        }
        
        // 文件分段最大的并发数
        public static int GetSegmentParallel()
        {
            return UniServiceContainer.Get<UniDownloadSetting>().SegmentParallel;
        }

        // 文件分段读写流缓冲
        public static int GetSegmentBuffSize()
        {
            return UniServiceContainer.Get<UniDownloadSetting>().SegmentBuffSize;
        }

        // 确保目录必须存在
        public static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        // 文件分段名字
        public static string GetSegmentName(int segmentIndex)
        {
            return $"info_{segmentIndex}.bin";
        }
    }
}