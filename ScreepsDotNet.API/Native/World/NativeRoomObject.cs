using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using ScreepsDotNet.Interop;

using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 8)]
    internal struct RoomObjectMetadata
    {
        public int TypeId;
        public IntPtr JSHandle;
    }

    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal abstract partial class NativeRoomObject : NativeObject, IRoomObject
    {
        #region Imports

        [JSImport("getProperty", "__object")]
        internal static partial JSObject[] Native_GetEffects(JSObject proxyObject, Name key);

        #endregion

        private UserDataStorage userDataStorage;

        protected RoomPosition? positionCache;
        protected Effect[]? effectsCache;

        protected virtual bool CanMove { get => false; }

        public IEnumerable<Effect> Effects => CachePerTick(ref effectsCache) ??= FetchEffects();

        public RoomPosition RoomPosition
        {
            get
            {
                if (CanMove)
                {
                    return CachePerTick(ref positionCache) ??= FetchRoomPosition();
                }
                else
                {
                    return CacheLifetime(ref positionCache) ??= FetchRoomPosition();
                }
            }
        }

        public IRoom? Room
        {
            get
            {
                var roomObj = ProxyObject.GetPropertyAsJSObject(Names.Room);
                if (roomObj == null) { return null; }
                return nativeRoot.GetRoomByProxyObject(roomObj);
            }
        }

        public NativeRoomObject(INativeRoot nativeRoot, JSObject proxyObject)
            : base(nativeRoot, proxyObject)
        { }

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            if (CanMove)
            {
                positionCache = null;
            }
        }

        #region User Data

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetUserData<T>(T? userData) where T : class => userDataStorage.SetUserData<T>(userData);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetUserData<T>([MaybeNullWhen(false)] out T userData) where T : class => userDataStorage.TryGetUserData<T>(out userData);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? GetUserData<T>() where T : class => userDataStorage.GetUserData<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasUserData<T>() where T : class => userDataStorage.HasUserData<T>();

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private RoomPosition FetchRoomPosition()
            => RoomPosition.FromEncodedInt(ScreepsDotNet_Native.FetchObjectRoomPosition(ProxyObject.JSHandle));

        private Effect[] FetchEffects()
        {
            var effectsArr = Native_GetEffects(ProxyObject, Names.Effects);
            var result = new Effect[effectsArr.Length];
            try
            {
                for (int i = 0; i < effectsArr.Length; ++i)
                {
                    var obj = effectsArr[i];
                    result[i] = new(
                        effectType: (EffectType)obj.GetPropertyAsInt32(Names.Effect),
                        level: obj.TryGetPropertyAsInt32(Names.Level),
                        ticksRemaining: obj.GetPropertyAsInt32(Names.TicksRemaining)
                    );
                }
                return result;
            }
            finally
            {
                effectsArr.DisposeAll();
            }
        }
    }

    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal abstract partial class NativeRoomObjectWithId : NativeRoomObject, IWithId, IEquatable<NativeRoomObjectWithId?>
    {
        private ObjectId? idCache;

        public ObjectId Id => CacheLifetime(ref idCache) ??= FetchId(ProxyObject);

        protected NativeRoomObjectWithId(INativeRoot nativeRoot, JSObject proxyObject)
            : base(nativeRoot, proxyObject)
        {
        }

        public override JSObject? ReacquireProxyObject()
        {
            if (idCache == null) { throw new InvalidOperationException($"Failed to reacquire proxy object (id was never cached)"); }
            return nativeRoot.GetProxyObjectById(idCache.Value);
        }

        protected override void OnLoseProxyObject(JSObject proxyObject)
        {
            base.OnLoseProxyObject(proxyObject);
            // Here we are losing the proxy object, e.g. we lost visibility of the object
            // If we haven't already cached the id, do so now so that we can recover the object via getObjectById if we regain visibility of it later
            if (idCache == null) { idCache = FetchId(proxyObject); }
        }

        private static ObjectId FetchId(JSObject from)
        {
            ScreepsDotNet_Native.RawObjectId rawObjectId = default;
            unsafe
            {
                if (ScreepsDotNet_Native.GetObjectId(from.JSHandle, &rawObjectId) == 0)
                {
                    throw new InvalidOperationException($"Failed to fetch object id for {from} (native call returned null)");
                }
            }
            return new(rawObjectId.AsSpan);
        }

        public override bool Equals(object? obj) => Equals(obj as NativeRoomObjectWithId);

        public bool Equals(NativeRoomObjectWithId? other) => other is not null && Id.Equals(other.Id);

        public override int GetHashCode() => Id.GetHashCode();

        public static bool operator ==(NativeRoomObjectWithId? left, NativeRoomObjectWithId? right) => EqualityComparer<NativeRoomObjectWithId>.Default.Equals(left, right);

        public static bool operator !=(NativeRoomObjectWithId? left, NativeRoomObjectWithId? right) => !(left == right);
    }


    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal static partial class NativeRoomObjectExtensions
    {
        #region Imports

        [JSImport("interpretDateTime", "object")]
        internal static partial double InterpretDateTime(JSObject obj);

        #endregion

        public static JSObject ToJS(this IRoomObject roomObject)
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            => (roomObject as NativeRoomObject).ProxyObject;
#pragma warning restore CS8602 // Dereference of a possibly null reference.

        public static DateTime ToDateTime(this JSObject obj)
            => DateTime.UnixEpoch + TimeSpan.FromSeconds(InterpretDateTime(obj));
    }
}
