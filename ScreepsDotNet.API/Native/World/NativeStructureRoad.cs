﻿using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeStructureRoad : NativeStructure, IStructureRoad
    {
        public int TicksToDecay => ProxyObject.GetPropertyAsInt32("ticksToDecay");

        public NativeStructureRoad(INativeRoot nativeRoot, JSObject? proxyObject, ObjectId id)
            : base(nativeRoot, proxyObject, id)
        { }

        public override string ToString()
            => $"StructureRoad[{(Exists ? RoomPosition.ToString() : "DEAD")}]";
    }
}
