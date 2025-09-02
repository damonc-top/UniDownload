using UniDownload.Config;

namespace UniDownload.Utils
{
    /*
        工具类
    */
    internal class UniUtils
    {
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
    }
}