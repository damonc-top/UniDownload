using System.Threading;

namespace UniDownload.UniDownloadCore
{
    internal class UniUUID
    {
        private static int _id = 0;

        public static int NextID => _id++;
    }
}