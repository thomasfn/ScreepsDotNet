using System;
using System.Collections.Generic;
using ScreepsDotNet.Interop;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeCpu : ICpu
    {
        #region Imports

        [JSImport("cpu.getHeapStatistics", "game")]
        
        internal static partial JSObject Native_GetHeapStatistics();

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
            using var obj = Native_GetHeapStatistics();
            return new HeapInfo(
                obj.GetPropertyAsInt32("total_heap_size"),
                obj.GetPropertyAsInt32("total_heap_size_executable"),
                obj.GetPropertyAsInt32("total_physical_size"),
                obj.GetPropertyAsInt32("total_available_size"),
                obj.GetPropertyAsInt32("used_heap_size"),
                obj.GetPropertyAsInt32("heap_size_limit"),
                obj.GetPropertyAsInt32("malloced_memory"),
                obj.GetPropertyAsInt32("peak_malloced_memory"),
                obj.GetPropertyAsInt32("does_zap_garbage"),
                obj.GetPropertyAsInt32("externally_allocated_size")
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
