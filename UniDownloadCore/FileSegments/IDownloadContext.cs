using System;

namespace UniDownload.UniDownloadCore
{
    internal interface IDownloadContext
    {
        public int TotalBytes { get; }
        public int BytesReceived { get; }
        public int Progress => BytesReceived / TotalBytes;
        public int[][] SegmentPositions { get; }
    }
}