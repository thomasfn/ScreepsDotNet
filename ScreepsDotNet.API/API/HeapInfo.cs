using System.Text;

namespace ScreepsDotNet.API
{
    public readonly struct HeapInfo
    {
        public readonly int TotalHeapSize;
        public readonly int TotalHeapSizeExecutable;
        public readonly int TotalPhysicalSize;
        public readonly int TotalAvailableSize;
        public readonly int UsedHeapSize;
        public readonly int HeapSizeLimit;
        public readonly int MallocedMemory;
        public readonly int PeakMallocedMemory;
        public readonly int DoesZapGarbage;
        public readonly int NumberOfNativeContexts;
        public readonly int NumberOfDetachedContexts;
        public readonly int ExternallyAllocatedSize;

        public HeapInfo(int totalHeapSize, int totalHeapSizeExecutable, int totalPhysicalSize, int totalAvailableSize, int usedHeapSize, int heapSizeLimit, int mallocedMemory, int peakMallocedMemory, int doesZapGarbage, int externallyAllocatedSize)
        {
            TotalHeapSize = totalHeapSize;
            TotalHeapSizeExecutable = totalHeapSizeExecutable;
            TotalPhysicalSize = totalPhysicalSize;
            TotalAvailableSize = totalAvailableSize;
            UsedHeapSize = usedHeapSize;
            HeapSizeLimit = heapSizeLimit;
            MallocedMemory = mallocedMemory;
            PeakMallocedMemory = peakMallocedMemory;
            DoesZapGarbage = doesZapGarbage;
            ExternallyAllocatedSize = externallyAllocatedSize;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"TotalHeapSize: {FormatBytes(TotalHeapSize)}");
            sb.AppendLine($"TotalHeapSizeExecutable: {FormatBytes(TotalHeapSizeExecutable)}");
            sb.AppendLine($"TotalPhysicalSize: {FormatBytes(TotalPhysicalSize)}");
            sb.AppendLine($"TotalAvailableSize: {FormatBytes(TotalAvailableSize)}");
            sb.AppendLine($"UsedHeapSize: {FormatBytes(UsedHeapSize)}");
            sb.AppendLine($"HeapSizeLimit: {FormatBytes(HeapSizeLimit)}");
            sb.AppendLine($"MallocedMemory: {FormatBytes(MallocedMemory)}");
            sb.AppendLine($"PeakMallocedMemory: {FormatBytes(PeakMallocedMemory)}");
            sb.AppendLine($"DoesZapGarbage: {DoesZapGarbage}");
            sb.AppendLine($"ExternallyAllocatedSize: {FormatBytes(ExternallyAllocatedSize)}");
            return sb.ToString();
        }

        private static string FormatBytes(int bytes)
            => $"{bytes / 1024} KiB";
    }
}
