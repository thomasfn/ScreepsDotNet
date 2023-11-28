using System.Collections.Generic;

namespace ScreepsDotNet.API.World
{
    public enum CpuSetShardLimitsResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// 12-hours cooldown period is not over yet.
        /// </summary>
        Busy = -4,
        /// <summary>
        /// The argument is not a valid shard limits object.
        /// </summary>
        InvalidArgs = -10
    }

    public enum CpuUnlockResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// Your account does not have enough cpuUnlock resource.
        /// </summary>
        NotEnoughResources = -6,
        /// <summary>
        /// Your CPU is unlocked with a subscription.
        /// </summary>
        Full = -8
    }

    public enum CpuGeneratePixelResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// Your bucket does not have enough CPU.
        /// </summary>
        NotEnoughResources = -6
    }

    public interface ICpu
    {
        /// <summary>
        /// Your assigned CPU limit for the current shard.
        /// </summary>
        double Limit { get; }

        /// <summary>
        /// An amount of available CPU time at the current game tick.
        /// Usually it is higher than Game.cpu.limit.
        /// </summary>
        double TickLimit { get; }

        /// <summary>
        /// An amount of unused CPU accumulated in your bucket.
        /// </summary>
        double Bucket { get; }

        /// <summary>
        /// An object with limits for each shard with shard names as keys.
        /// You can use setShardLimits method to re-assign them.
        /// </summary>
        IReadOnlyDictionary<string, double> ShardLimits { get; }

        /// <summary>
        /// Whether full CPU is currently unlocked for your account.
        /// </summary>
        bool Unlocked { get; }

        /// <summary>
        /// The time in milliseconds since UNIX epoch time until full CPU is unlocked for your account.
        /// This property is not defined when full CPU is not unlocked for your account or it's unlocked with a subscription.
        /// </summary>
        long? UnlockedTime {  get; }

        /// <summary>
        /// Use this method to get heap statistics for your virtual machine.
        /// </summary>
        /// <returns></returns>
        HeapInfo GetHeapStatistics();

        /// <summary>
        /// Get amount of CPU time used from the beginning of the current game tick.
        /// Always returns 0 in the Simulation mode.
        /// </summary>
        /// <returns></returns>
        double GetUsed();

        /// <summary>
        /// Reset your runtime environment and wipe all data in heap memory.
        /// </summary>
        void Halt();

        /// <summary>
        /// Allocate CPU limits to different shards.
        /// Total amount of CPU should remain equal to Game.cpu.shardLimits.
        /// This method can be used only once per 12 hours.
        /// </summary>
        /// <param name="shardLimits"></param>
        CpuSetShardLimitsResult SetShardLimits(IReadOnlyDictionary<string, double> shardLimits);

        /// <summary>
        /// Unlock full CPU for your account for additional 24 hours. 
        /// This method will consume 1 CPU unlock bound to your account (See Game.resources).
        /// If full CPU is not currently unlocked for your account, it may take some time (up to 5 minutes) before unlock is applied to your account.
        /// </summary>
        /// <returns></returns>
        CpuUnlockResult Unlock();

        /// <summary>
        /// Generate 1 pixel resource unit for 10000 CPU from your bucket.
        /// </summary>
        /// <returns></returns>
        CpuGeneratePixelResult GeneratePixel();
    }
}
