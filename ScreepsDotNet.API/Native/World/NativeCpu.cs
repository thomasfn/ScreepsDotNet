using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeCpu : ICpu
    {
        #region Imports

        [JSImport("cpu.getHeapStatistics", "game")]
        [return: JSMarshalAsAttribute<JSType.Object>]
        internal static partial JSObject Native_GetHeapStatistics();

        [JSImport("get", "object")]
        [return: JSMarshalAsAttribute<JSType.Function<JSType.Number>>]
        internal static partial Func<double> Native_Get_GetUsed([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.String>] string key);

        #endregion

        private JSObject proxyObject;

        internal JSObject ProxyObject
        {
            get => proxyObject;
            set
            {
                proxyObject = value;
                getUsedCache = null;
            }
        }

        private Func<double>? getUsedCache;

        public double Limit => ProxyObject.GetPropertyAsDouble("limit");

        public double TickLimit => ProxyObject.GetPropertyAsDouble("tickLimit");

        public double Bucket => ProxyObject.GetPropertyAsDouble("bucket");

        public IReadOnlyDictionary<string, double> ShardLimits => throw new NotImplementedException();

        public bool Unlocked =>  ProxyObject.GetPropertyAsBoolean("unlocked");

        public long? UnlockedTime => ProxyObject.GetTypeOfProperty("unlockedTime") == "number" ? ProxyObject.GetPropertyAsInt32("unlockedTime") : null;

        public NativeCpu(JSObject proxyObject)
        {
            this.proxyObject = proxyObject;
        }

        public HeapInfo GetHeapStatistics()
        {
            var obj = Native_GetHeapStatistics();
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
                obj.GetPropertyAsInt32("number_of_native_contexts"),
                obj.GetPropertyAsInt32("number_of_detached_contexts"),
                obj.GetPropertyAsInt32("externally_allocated_size")
            );
        }

        public double GetUsed()
            => (getUsedCache ??= Native_Get_GetUsed(ProxyObject, "getUsed"))();

        public void Halt()
        {
            throw new NotImplementedException();
        }

        public CpuSetShardLimitsResult SetShardLimits(IReadOnlyDictionary<string, double> shardLimits)
        {
            throw new NotImplementedException();
        }

        public CpuUnlockResult Unlock()
        {
            throw new NotImplementedException();
        }

        public CpuGeneratePixelResult GeneratePixel()
        {
            throw new NotImplementedException();
        }

        public void Notify(string message, int groupInterval)
        {
            throw new NotImplementedException();
        }
    }
}
