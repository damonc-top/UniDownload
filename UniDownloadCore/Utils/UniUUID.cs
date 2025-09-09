
using System.Threading;

namespace UniDownload
{
    /*
        唯一ID
    */
    internal static class UniUUID
    {
        private static int _uuid = 0;

        public static int ID => Interlocked.Increment(ref _uuid);
    }
}