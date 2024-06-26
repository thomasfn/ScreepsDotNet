﻿using ScreepsDotNet.Interop;

namespace ScreepsDotNet.Native.World
{
    internal static class Names
    {
        public static readonly Name Fatigue = Name.Create("fatigue");
        public static readonly Name Hits = Name.Create("hits");
        public static readonly Name HitsMax = Name.Create("hitsMax");
        public static readonly Name Memory = Name.Create("memory");
        public static readonly Name My = Name.Create("my");
        public static readonly Name Name = Name.Create("name");
        public static readonly Name Owner = Name.Create("owner");
        public static readonly Name Username = Name.Create("username");
        public static readonly Name Saying = Name.Create("saying");
        public static readonly Name Spawning = Name.Create("spawning");
        public static readonly Name Store = Name.Create("store");
        public static readonly Name TicksToLive = Name.Create("ticksToLive");
        public static readonly Name Progress = Name.Create("progress");
        public static readonly Name ProgressTotal = Name.Create("progressTotal");
        public static readonly Name StructureType = Name.Create("structureType");
        public static readonly Name Limit = Name.Create("limit");
        public static readonly Name TickLimit = Name.Create("tickLimit");
        public static readonly Name Bucket = Name.Create("bucket");
        public static readonly Name Unlocked = Name.Create("unlocked");
        public static readonly Name UnlockedTime = Name.Create("unlockedTime");
        public static readonly Name Cooldown = Name.Create("cooldown");
        public static readonly Name LastCooldown = Name.Create("lastCooldown");
        public static readonly Name DepositType = Name.Create("depositType");
        public static readonly Name TicksToDecay = Name.Create("ticksToDecay");
        public static readonly Name Color = Name.Create("color");
        public static readonly Name SecondaryColor = Name.Create("secondaryColor");
        public static readonly Name Time = Name.Create("time");
        public static readonly Name Creeps = Name.Create("creeps");
        public static readonly Name Flags = Name.Create("flags");
        public static readonly Name Rooms = Name.Create("rooms");
        public static readonly Name Spawns = Name.Create("spawns");
        public static readonly Name Structures = Name.Create("structures");
        public static readonly Name Cpu = Name.Create("cpu");
        public static readonly Name Market = Name.Create("market");
        public static readonly Name Amount = Name.Create("amount");
        public static readonly Name ResourceType = Name.Create("resourceType");
        public static readonly Name Controller = Name.Create("controller");
        public static readonly Name EnergyAvailable = Name.Create("energyAvailable");
        public static readonly Name EnergyCapacityAvailable = Name.Create("energyCapacityAvailable");
        public static readonly Name Storage = Name.Create("storage");
        public static readonly Name Terminal = Name.Create("terminal");
        public static readonly Name Visual = Name.Create("visual");
        public static readonly Name Code = Name.Create("code");
        public static readonly Name Type = Name.Create("type");
        public static readonly Name Effects = Name.Create("effects");
        public static readonly Name Effect = Name.Create("effect");
        public static readonly Name Level = Name.Create("level");
        public static readonly Name TicksRemaining = Name.Create("ticksRemaining");
        public static readonly Name Segments = Name.Create("segments");
        public static readonly Name ForeignSegment = Name.Create("foreignSegment");
        public static readonly Name Id = Name.Create("id");
        public static readonly Name Data = Name.Create("data");
        public static readonly Name DestroyTime = Name.Create("destroyTime");
        public static readonly Name TicksToRegeneration = Name.Create("ticksToRegeneration");
        public static readonly Name Energy = Name.Create("energy");
        public static readonly Name EnergyCapacity = Name.Create("energyCapacity");
        public static readonly Name IsPowerEnabled = Name.Create("isPowerEnabled");
        public static readonly Name TicksToEnd = Name.Create("ticksToEnd");
        public static readonly Name Reservation = Name.Create("reservation");
        public static readonly Name SafeMode = Name.Create("safeMode");
        public static readonly Name SafeModeAvailable = Name.Create("safeModeAvailable");
        public static readonly Name SafeModeCooldown = Name.Create("safeModeCooldown");
        public static readonly Name Sign = Name.Create("sign");
        public static readonly Name Text = Name.Create("text");
        public static readonly Name Datetime = Name.Create("datetime");
        public static readonly Name TicksToDowngrade = Name.Create("ticksToDowngrade");
        public static readonly Name UpgradeBlocked = Name.Create("upgradeBlocked");
        public static readonly Name TicksToDeploy = Name.Create("ticksToDeploy");
        public static readonly Name TicksToSpawn = Name.Create("ticksToSpawn");
        public static readonly Name MineralType = Name.Create("mineralType");
        public static readonly Name Destination = Name.Create("destination");
        public static readonly Name Room = Name.Create("room");
        public static readonly Name Shard = Name.Create("shard");
        public static readonly Name Power = Name.Create("power");
        public static readonly Name NeedTime = Name.Create("needTime");
        public static readonly Name RemainingTime = Name.Create("remainingTime");
        public static readonly Name Directions = Name.Create("directions");
        public static readonly Name Spawn = Name.Create("spawn");
        public static readonly Name DryRun = Name.Create("dryRun");
        public static readonly Name EnergyStructures = Name.Create("energyStructures");
        public static readonly Name DeathTime = Name.Create("deathTime");
        public static readonly Name Creep = Name.Create("creep");
        public static readonly Name RoomName = Name.Create("roomName");
        public static readonly Name Timestamp = Name.Create("timestamp");
        public static readonly Name Status = Name.Create("status");
        public static readonly Name Normal = Name.Create("normal");
        public static readonly Name Closed = Name.Create("closed");
        public static readonly Name Novice = Name.Create("novice");
        public static readonly Name Respawn = Name.Create("respawn");
        public static readonly Name Ptr = Name.Create("ptr");
        public static readonly Name TimeToLand = Name.Create("timeToLand");
        public static readonly Name LaunchRoomName = Name.Create("launchRoomName");
        public static readonly Name Density = Name.Create("density");
        public static readonly Name MineralAmount = Name.Create("mineralAmount");
        public static readonly Name IsPublic = Name.Create("isPublic");
        public static readonly Name ReusePath = Name.Create("reusePath");
        public static readonly Name SerializeMemory = Name.Create("serializeMemory");
        public static readonly Name NoPathFinding = Name.Create("noPathFinding");
        public static readonly Name VisualizePathStyle = Name.Create("visualizePathStyle");
        public static readonly Name Class = Name.Create("class");
        public static readonly Name ClassName = Name.Create("className");
        public static readonly Name PowerCreeps = Name.Create("powerCreeps");
        public static readonly Name DeleteTime = Name.Create("deleteTime");
        public static readonly Name SpawnCooldownTimeCache = Name.Create("spawnCooldownTimeCache");
        public static readonly Name Powers = Name.Create("powers");
        public static readonly Name RoomCallback = Name.Create("roomCallback");
        public static readonly Name PlainCost = Name.Create("plainCost");
        public static readonly Name SwampCost = Name.Create("swampCost");
        public static readonly Name Flee = Name.Create("flee");
        public static readonly Name MaxOps = Name.Create("maxOps");
        public static readonly Name MaxCost = Name.Create("maxCost");
        public static readonly Name MaxRooms = Name.Create("maxRooms");
        public static readonly Name HeuristicWeight = Name.Create("heuristicWeight");
        public static readonly Name CostCallback = Name.Create("costCallback");
        public static readonly Name IgnoreCreeps = Name.Create("ignoreCreeps");
        public static readonly Name IgnoreRoads = Name.Create("ignoreRoads");
        public static readonly Name IgnoreDestructibleStructures = Name.Create("ignoreDestructibleStructures");
        public static readonly Name Ignore = Name.Create("ignore");
        public static readonly Name Avoid = Name.Create("avoid");
        public static readonly Name Range = Name.Create("range");
        public static readonly Name X = Name.Create("x");
        public static readonly Name Y = Name.Create("y");
        public static readonly Name Dx = Name.Create("dx");
        public static readonly Name Dy = Name.Create("dy");
        public static readonly Name Direction = Name.Create("direction");
        public static readonly Name Path = Name.Create("path");
        public static readonly Name Ops = Name.Create("ops");
        public static readonly Name Incomplete = Name.Create("incomplete");
        public static readonly Name Cost = Name.Create("cost");
        public static readonly Name Pos = Name.Create("pos");
        public static readonly Name RouteCallback = Name.Create("routeCallback");
        public static readonly Name Exit = Name.Create("exit");
    }
}
