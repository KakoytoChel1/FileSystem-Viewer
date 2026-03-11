namespace FileSystem_Viewer.Models.DataModels
{
    public class TotalScanValues
    {
        public TotalScanValues(long totalFileCount, long totalSize)
        {
            TotalFileCount = totalFileCount;
            TotalSize = totalSize;
        }

        public long TotalFileCount { get; private set; }
        public long TotalSize { get; private set; }
    }
}
