using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace UniDownload
{
    /// <summary>
    /// 性能和网络监测器
    /// </summary>
    internal class UniPerformanceMonitor
    {
        private NetworkSpeed _currentNetworkSpeed = NetworkSpeed.Medium;
        private NetworkType _currentNetworkType = NetworkType.Wifi;
        private DevicePerformance _currentDevicePerformance = DevicePerformance.Medium;
        
        private Timer _monitorTimer;
        private readonly object _lockObject = new object();
        
        // 网络测试相关
        private long _lastBytesReceived = 0;
        private DateTime _lastMeasureTime = DateTime.Now;
        private double _currentDownloadSpeed = 0; // bytes/sec
        
        // 设备性能相关
        private long _availableMemory = 0;
        private double _cpuUsage = 0;
        
        public event Action<NetworkSpeed, NetworkType, DevicePerformance> OnPerformanceChanged;

        public UniPerformanceMonitor()
        {
            // 每5秒检测一次性能
            _monitorTimer = new Timer(MonitorPerformance, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
            InitialDetection();
        }

        /// <summary>
        /// 获取当前网络速度
        /// </summary>
        public NetworkSpeed CurrentNetworkSpeed
        {
            get { lock (_lockObject) return _currentNetworkSpeed; }
        }

        /// <summary>
        /// 获取当前网络类型
        /// </summary>
        public NetworkType CurrentNetworkType
        {
            get { lock (_lockObject) return _currentNetworkType; }
        }

        /// <summary>
        /// 获取当前设备性能
        /// </summary>
        public DevicePerformance CurrentDevicePerformance
        {
            get { lock (_lockObject) return _currentDevicePerformance; }
        }

        /// <summary>
        /// 获取当前下载速度 (bytes/sec)
        /// </summary>
        public double CurrentDownloadSpeed
        {
            get { lock (_lockObject) return _currentDownloadSpeed; }
        }

        /// <summary>
        /// 更新下载统计数据
        /// </summary>
        /// <param name="bytesReceived">接收到的字节数</param>
        public void UpdateDownloadStats(long bytesReceived)
        {
            lock (_lockObject)
            {
                var now = DateTime.Now;
                var timeDiff = (now - _lastMeasureTime).TotalSeconds;
                
                if (timeDiff >= 1.0) // 每秒更新一次速度
                {
                    var bytesDiff = bytesReceived - _lastBytesReceived;
                    _currentDownloadSpeed = bytesDiff / timeDiff;
                    
                    _lastBytesReceived = bytesReceived;
                    _lastMeasureTime = now;
                    
                    // 根据下载速度更新网络速度评估
                    UpdateNetworkSpeedFromStats();
                }
            }
        }

        private void InitialDetection()
        {
            Task.Run(() =>
            {
                DetectNetworkType();
                DetectDevicePerformance();
                DetectInitialNetworkSpeed();
            });
        }

        private void MonitorPerformance(object state)
        {
            try
            {
                DetectDevicePerformance();
                DetectNetworkType();
                
                // 触发性能变化事件
                OnPerformanceChanged?.Invoke(_currentNetworkSpeed, _currentNetworkType, _currentDevicePerformance);
            }
            catch (Exception ex)
            {
                UniLogger.Error($"Performance monitoring error: {ex.Message}");
            }
        }

        private void DetectNetworkType()
        {
            lock (_lockObject)
            {
                // Unity/C# 网络类型检测
                // 这里需要根据具体平台实现
                #if UNITY_EDITOR
                _currentNetworkType = NetworkType.Wifi; // 编辑器默认wifi
                #elif UNITY_ANDROID
                // Android 网络检测逻辑
                _currentNetworkType = DetectAndroidNetworkType();
                #elif UNITY_IOS
                // iOS 网络检测逻辑
                _currentNetworkType = DetectIOSNetworkType();
                #else
                _currentNetworkType = NetworkType.Other;
                #endif
            }
        }

        private void DetectDevicePerformance()
        {
            lock (_lockObject)
            {
                try
                {
                    // 检测可用内存
                    var process = Process.GetCurrentProcess();
                    _availableMemory = GC.GetTotalMemory(false);
                    
                    // 简单的设备性能评估
                    // 可以根据具体需求调整阈值
                    if (_availableMemory < 512 * 1024 * 1024) // < 512MB
                    {
                        _currentDevicePerformance = DevicePerformance.Low;
                    }
                    else if (_availableMemory < 0) // < 2GB
                    {
                        _currentDevicePerformance = DevicePerformance.Medium;
                    }
                    else
                    {
                        _currentDevicePerformance = DevicePerformance.High;
                    }
                }
                catch
                {
                    _currentDevicePerformance = DevicePerformance.Medium; // 默认中等性能
                }
            }
        }

        private void DetectInitialNetworkSpeed()
        {
            // 初始网络速度检测，可以通过小文件下载测试
            // 这里先设置一个默认值，后续通过实际下载统计更新
            lock (_lockObject)
            {
                _currentNetworkSpeed = NetworkSpeed.Medium;
            }
        }

        private void UpdateNetworkSpeedFromStats()
        {
            if (_currentDownloadSpeed <= 0) return;

            NetworkSpeed newSpeed;
            
            // 根据实际下载速度分类网络速度
            if (_currentDownloadSpeed < 100 * 1024) // < 100KB/s
            {
                newSpeed = NetworkSpeed.Slow;
            }
            else if (_currentDownloadSpeed < 1024 * 1024) // < 1MB/s
            {
                newSpeed = NetworkSpeed.Medium;
            }
            else
            {
                newSpeed = NetworkSpeed.Fast;
            }

            if (newSpeed != _currentNetworkSpeed)
            {
                _currentNetworkSpeed = newSpeed;
                UniLogger.Debug($"Network speed updated to: {newSpeed} (Current: {_currentDownloadSpeed / 1024:F1} KB/s)");
            }
        }

        #if UNITY_ANDROID
        private NetworkType DetectAndroidNetworkType()
        {
            // Android 网络类型检测实现
            // 这里需要使用 Android 原生 API
            return NetworkType.Mobile; // 临时返回
        }
        #endif

        #if UNITY_IOS
        private NetworkType DetectIOSNetworkType()
        {
            // iOS 网络类型检测实现
            // 这里需要使用 iOS 原生 API
            return NetworkType.Wifi; // 临时返回
        }
        #endif

        public void Dispose()
        {
            _monitorTimer?.Dispose();
        }
    }
}
