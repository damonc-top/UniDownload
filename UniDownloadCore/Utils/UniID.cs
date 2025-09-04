
using System.Threading;

namespace UniDownload
{
    /*
        唯一ID
    */
    internal static class UniID
    {
        private static int _id = 0;

        public static int ID => Interlocked.Increment(ref _id);
    }
}