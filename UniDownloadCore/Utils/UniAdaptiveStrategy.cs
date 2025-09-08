using System;

namespace UniDownload
{
    /// <summary>
    /// 自适应下载策略
    /// </summary>
    internal class UniAdaptiveStrategy
    {
        /// <summary>
        /// 下载配置参数
        /// </summary>
        public struct DownloadConfig
        {
            public int ChunkSize;           // 分片大小 (bytes)
            public int MaxConcurrent;       // 最大并发数
            public int ConnectionTimeout;   // 连接超时 (ms)
            public int RetryCount;          // 重试次数
            public int SpeedLimitBps;       // 速度限制 (bytes/sec)
            public int BufferSize;          // 缓冲区大小 (bytes)
        }

        /// <summary>
        /// 根据设备性能和网络状况计算最佳下载配置
        /// </summary>
        /// <param name="networkSpeed">网络速度</param>
        /// <param name="networkType">网络类型</param>
        /// <param name="devicePerformance">设备性能</param>
        /// <param name="currentDownloadSpeed">当前实际下载速度</param>
        /// <returns>优化的下载配置</returns>
        public static DownloadConfig CalculateOptimalConfig(
            NetworkSpeed networkSpeed, 
            NetworkType networkType, 
            DevicePerformance devicePerformance,
            double currentDownloadSpeed = 0)
        {
            var config = GetBaseConfig(networkSpeed);
            
            // 根据网络类型调整
            AdjustForNetworkType(ref config, networkType);
            
            // 根据设备性能调整
            AdjustForDevicePerformance(ref config, devicePerformance);
            
            // 根据实际下载速度微调
            AdjustForRealSpeed(ref config, currentDownloadSpeed);
            
            return config;
        }

        private static DownloadConfig GetBaseConfig(NetworkSpeed networkSpeed)
        {
            return networkSpeed switch
            {
                NetworkSpeed.Slow => new DownloadConfig
                {
                    ChunkSize = 256 * 1024,        // 256KB 小分片
                    MaxConcurrent = 1,             // 单线程下载
                    ConnectionTimeout = 45000,     // 45秒超时
                    RetryCount = 5,                // 多次重试
                    SpeedLimitBps = 50 * 1024,     // 限制50KB/s，避免占满带宽
                    BufferSize = 8 * 1024          // 8KB缓冲区
                },
                
                NetworkSpeed.Medium => new DownloadConfig
                {
                    ChunkSize = 1024 * 1024,       // 1MB
                    MaxConcurrent = 2,             
                    ConnectionTimeout = 30000,     // 30秒
                    RetryCount = 3,
                    SpeedLimitBps = 200 * 1024,    // 限制200KB/s
                    BufferSize = 16 * 1024         // 16KB
                },
                
                NetworkSpeed.Fast => new DownloadConfig
                {
                    ChunkSize = 4 * 1024 * 1024,   // 4MB 大分片
                    MaxConcurrent = 4,
                    ConnectionTimeout = 20000,     // 20秒
                    RetryCount = 2,
                    SpeedLimitBps = 0,             // 不限速
                    BufferSize = 64 * 1024         // 64KB
                },
                
                _ => new DownloadConfig
                {
                    ChunkSize = 1024 * 1024,
                    MaxConcurrent = 2,
                    ConnectionTimeout = 30000,
                    RetryCount = 3,
                    SpeedLimitBps = 0,
                    BufferSize = 16 * 1024
                }
            };
        }

        private static void AdjustForNetworkType(ref DownloadConfig config, NetworkType networkType)
        {
            switch (networkType)
            {
                case NetworkType.Mobile:
                    // 移动网络：更保守的策略
                    config.MaxConcurrent = Math.Max(1, config.MaxConcurrent - 1);
                    config.ConnectionTimeout += 10000; // 增加10秒超时
                    config.RetryCount += 1;
                    
                    // 如果没有限速，添加合理的限速避免流量消耗过快
                    if (config.SpeedLimitBps == 0)
                    {
                        config.SpeedLimitBps = 500 * 1024; // 500KB/s
                    }
                    break;
                    
                case NetworkType.Wifi:
                    // WiFi：可以更激进
                    config.MaxConcurrent = Math.Min(6, config.MaxConcurrent + 1);
                    config.ConnectionTimeout = Math.Max(15000, config.ConnectionTimeout - 5000);
                    break;
                    
                case NetworkType.Other:
                    // 未知网络：保守策略
                    config.MaxConcurrent = Math.Max(1, config.MaxConcurrent - 1);
                    break;
            }
        }

        private static void AdjustForDevicePerformance(ref DownloadConfig config, DevicePerformance devicePerformance)
        {
            switch (devicePerformance)
            {
                case DevicePerformance.Low:
                    // 低性能设备：减少并发和缓冲区大小
                    config.MaxConcurrent = Math.Max(1, config.MaxConcurrent - 1);
                    config.BufferSize = Math.Max(4 * 1024, config.BufferSize / 2);
                    config.ChunkSize = Math.Max(128 * 1024, config.ChunkSize / 2);
                    break;
                    
                case DevicePerformance.High:
                    // 高性能设备：可以增加并发和缓冲区
                    config.MaxConcurrent = Math.Min(8, config.MaxConcurrent + 1);
                    config.BufferSize = Math.Min(128 * 1024, config.BufferSize * 2);
                    break;
                    
                case DevicePerformance.Medium:
                    // 中等性能：保持默认配置
                    break;
            }
        }

        private static void AdjustForRealSpeed(ref DownloadConfig config, double currentDownloadSpeed)
        {
            if (currentDownloadSpeed <= 0) return;

            // 根据实际下载速度动态调整分片大小
            if (currentDownloadSpeed < 50 * 1024) // < 50KB/s，网络很慢
            {
                config.ChunkSize = Math.Max(128 * 1024, config.ChunkSize / 2);
                config.MaxConcurrent = 1;
            }
            else if (currentDownloadSpeed > 2 * 1024 * 1024) // > 2MB/s，网络很快
            {
                config.ChunkSize = Math.Min(8 * 1024 * 1024, config.ChunkSize * 2);
                config.MaxConcurrent = Math.Min(6, config.MaxConcurrent + 1);
            }
        }

        /// <summary>
        /// 计算推荐的速度限制
        /// </summary>
        /// <param name="networkType">网络类型</param>
        /// <param name="networkSpeed">网络速度</param>
        /// <returns>推荐的速度限制 (bytes/sec)，0表示不限制</returns>
        public static int CalculateRecommendedSpeedLimit(NetworkType networkType, NetworkSpeed networkSpeed)
        {
            // 只对移动网络进行限速，保护用户流量
            if (networkType != NetworkType.Mobile)
            {
                return 0; // WiFi和其他网络不限速
            }

            return networkSpeed switch
            {
                NetworkSpeed.Slow => 30 * 1024,    // 30KB/s
                NetworkSpeed.Medium => 200 * 1024,  // 200KB/s
                NetworkSpeed.Fast => 1024 * 1024,   // 1MB/s
                _ => 100 * 1024                     // 默认100KB/s
            };
        }

        /// <summary>
        /// 判断是否应该暂停下载任务
        /// </summary>
        /// <param name="networkSpeed">当前网络速度</param>
        /// <param name="devicePerformance">设备性能</param>
        /// <param name="batteryLevel">电池电量 (0-100)</param>
        /// <returns>是否应该暂停</returns>
        public static bool ShouldPauseDownload(NetworkSpeed networkSpeed, DevicePerformance devicePerformance, int batteryLevel = 100)
        {
            // 电量过低时暂停下载
            if (batteryLevel < 10)
            {
                return true;
            }

            // 低性能设备 + 慢速网络时暂停
            if (devicePerformance == DevicePerformance.Low && networkSpeed == NetworkSpeed.Slow)
            {
                return true;
            }

            return false;
        }
    }
}
