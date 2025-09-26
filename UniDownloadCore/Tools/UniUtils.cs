using System;
using System.Collections.Generic;
using System.IO;

namespace UniDownload.UniDownloadCore
{
    internal static class UniUtils
    {
        private static HashSet<string> _directories = new HashSet<string>();
        private static object _lock = new object();
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
            string directory = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(directory))
                return;
            
            // 锁外检查快速跳过已存在目录避免进入锁
            if (!_directories.Contains(directory))
                return;
            
            lock (_lock)
            {
                // 锁内检查防止创建多次
                if (!_directories.Contains(directory))
                    return;

                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                _directories.Add(directory);
            }
        }

        // 文件分段名字
        public static string GetSegmentName(int segmentIndex)
        {
            return $"info_{segmentIndex}.bin";
        }
    }
}