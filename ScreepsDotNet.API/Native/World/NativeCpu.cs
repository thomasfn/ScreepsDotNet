using System;
using System.Collections.Generic;
using ScreepsDotNet.Interop;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [JSStruct]
    internal partial struct JSHeapInfo
    {
        [JSStructField("total_heap_size")] public int TotalHeapSize;
        [JSStructField("total_heap_size_executable")] public int TotalHeapSizeExecutable;
        [JSStructField("total_physical_size")] public int TotalPhysicalSize;
        [JSStructField("total_available_size")] public int TotalAvailableSize;
        [JSStructField("used_heap_size")] public int UsedHeapSize;
        [JSStructField("heap_size_limit")] public int HeapSizeLimit;
        [JSStructField("malloced_memory")] public int MallocedMemory;
        [JSStructField("peak_malloced_memory")] public int PeakMallocedMemory;
        [JSStructField("does_zap_garbage")] public int DoesZapGarbage;
        [JSStructField("externally_allocated_size")] public int ExternallyAllocatedSize;
    }

    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeCpu : ICpu
    {
        #region Imports

        [JSImport("cpu.getHeapStatistics", "game")]
        internal static partial JSHeapInfo Native_GetHeapStatistics();

        [JSImport("cpu.getHeapStatistics", "game")]
        internal static partial JSHeapInfo? Native_GetHeapStatisticsNullable();

        [JSImport("cpu.getUsed", "game")]
        internal static partial double Native_GetUsed();

        [JSImport("cpu.halt", "game")]
        internal static partial void Native_Halt();

        [JSImport("cpu.setShardLimits", "game")]
        internal static partial int Native_SetShardLimits(JSObject newShardLimits);

        [JSImport("cpu.unlock", "game")]
        internal static partial int Native_Unlock();

        [JSImport("cpu.generatePixel", "game")]
        internal static partial int Native_GeneratePixel();

        #endregion

        private JSObject proxyObject;

        internal JSObject ProxyObject
        {
            get => proxyObject;
            set
            {
                proxyObject = value;
            }
        }

        public double Limit => ProxyObject.GetPropertyAsDouble(Names.Limit);

        public double TickLimit => ProxyObject.GetPropertyAsDouble(Names.TickLimit);

        public double Bucket => ProxyObject.GetPropertyAsDouble(Names.Bucket);

        public IReadOnlyDictionary<string, double> ShardLimits => throw new NotImplementedException();

        public bool Unlocked =>  ProxyObject.GetPropertyAsBoolean(Names.Unlocked);

        public long? UnlockedTime => ProxyObject.TryGetPropertyAsInt32(Names.UnlockedTime);

        public NativeCpu(JSObject proxyObject)
        {
            this.proxyObject = proxyObject;
        }

        public HeapInfo GetHeapStatistics()
        {
            var heapInfo = Native_GetHeapStatistics();
            return new HeapInfo(
                heapInfo.TotalHeapSize,
                heapInfo.TotalHeapSizeExecutable,
                heapInfo.TotalPhysicalSize,
                heapInfo.TotalAvailableSize,
                heapInfo.UsedHeapSize,
                heapInfo.HeapSizeLimit,
                heapInfo.MallocedMemory,
                heapInfo.PeakMallocedMemory,
                heapInfo.DoesZapGarbage,
                heapInfo.ExternallyAllocatedSize
            );
        }

        public double GetUsed()
            => Native_GetUsed();

        public void Halt()
            => Native_Halt();

        public CpuSetShardLimitsResult SetShardLimits(IReadOnlyDictionary<string, double> shardLimits)
        {
            using var obj = JSObject.Create();
            foreach (var pair in shardLimits)
            {
                obj.SetProperty(pair.Key, pair.Value);
            }
            return (CpuSetShardLimitsResult)Native_SetShardLimits(obj);
        }

        public CpuUnlockResult Unlock()
            => (CpuUnlockResult)Native_Unlock();

        public CpuGeneratePixelResult GeneratePixel()
            => (CpuGeneratePixelResult)Native_GeneratePixel();
    }
}
