
namespace UniDownload
{
    /*
        工具类
    */
    internal static class UniUtils
    {
        /// <summary>
        /// 获取最大并发数
        /// </summary>
        /// <returns></returns>
        public static int GetMaxParallel()
        {
            UniDownloadSetting setting = UniServiceContainer.Get<UniDownloadSetting>();
            return setting.MaxScheduleParallel;
        }

        /// <summary>
        /// 注册回调事件给主线程
        /// </summary>
        /// <param name="call">无参回调</param>
        public static void RegisterMainThreadEvent(System.Action call)
        {
            UniMainThread main = UniServiceContainer.Get<UniMainThread>();
            main.Enqueue(call);
        }

        /// <summary>
        /// 复用request operation对象
        /// </summary>
        /// <returns></returns>
        public static UniRequestOperation RentRequestOperation()
        {
            UniDownloadPool pool = UniServiceContainer.Get<UniDownloadPool>();
            return pool.Rent<UniRequestOperation>();
        }

        public static UniDownloadRequest RentDownloadRequest()
        {
            UniDownloadPool pool = UniServiceContainer.Get<UniDownloadPool>();
            return pool.Rent<UniDownloadRequest>();
        }
        
        /*
            获取设备性能分级
        */
        public static DevicePerformance GetDevicePerformance()
        {
            return DevicePerformance.Medium;
        }
        
        /*
            获取设备网络类型
        */
        public static NetworkType GetDeviceNetworkType()
        {
            return NetworkType.Wifi;
        }
        
        /*
            获取设备网络速度
        */
        public static NetworkSpeed GetDeviceNetworkSpeed()
        {
            return NetworkSpeed.Fast;
        }

        /// <summary>
        /// 使用游戏开始到现在的运行帧数作为简易时间戳
        /// </summary>
        /// <returns></returns>
        public static int TimeTicks()
        {
            return UnityEngine.Time.frameCount;
        }
    }
}