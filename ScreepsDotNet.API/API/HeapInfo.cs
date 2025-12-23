using System.Text;

namespace ScreepsDotNet.API
{
    public readonly struct HeapInfo(int totalHeapSize, int totalHeapSizeExecutable, int totalPhysicalSize, int totalAvailableSize, int usedHeapSize, int heapSizeLimit, int mallocedMemory, int peakMallocedMemory, int doesZapGarbage, int externallyAllocatedSize)
    {
        public readonly int TotalHeapSize = totalHeapSize;
        public readonly int TotalHeapSizeExecutable = totalHeapSizeExecutable;
        public readonly int TotalPhysicalSize = totalPhysicalSize;
        public readonly int TotalAvailableSize = totalAvailableSize;
        public readonly int UsedHeapSize = usedHeapSize;
        public readonly int HeapSizeLimit = heapSizeLimit;
        public readonly int MallocedMemory = mallocedMemory;
        public readonly int PeakMallocedMemory = peakMallocedMemory;
        public readonly int DoesZapGarbage = doesZapGarbage;
        public readonly int NumberOfNativeContexts;
        public readonly int NumberOfDetachedContexts;
        public readonly int ExternallyAllocatedSize = externallyAllocatedSize;

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
