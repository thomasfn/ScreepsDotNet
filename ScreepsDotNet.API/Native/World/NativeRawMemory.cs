using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal class NativeRawMemorySegments : IDictionary<int, string>
    {
        private const int MaxSegmentCount = 100;

        private readonly JSObject proxyObject;

        private readonly IDictionary<int, string?> segmentCache = new Dictionary<int, string?>();

        public string this[int key]
        {
            get => (segmentCache.TryGetValue(key, out var segment) ? segment : null) ?? throw new InvalidOperationException($"Can't read raw segment that was not requested previous tick.");
            set
            {
                segmentCache[key] = value;
                proxyObject.SetProperty(key.ToString(), value);
            }
        }

        public ICollection<int> Keys => throw new NotImplementedException();

        public ICollection<string> Values => throw new NotImplementedException();

        public int Count => throw new NotImplementedException();

        public bool IsReadOnly => throw new NotImplementedException();

        public NativeRawMemorySegments(JSObject proxyObject)
        {
            this.proxyObject = proxyObject;
        }

        public void Add(int key, string value)
            => throw new InvalidOperationException($"New segments can't be added, they must be requested first");

        public void Add(KeyValuePair<int, string> item)
            => throw new InvalidOperationException($"New segments can't be added, they must be requested first");

        public void Clear()
            => throw new InvalidOperationException($"Segments can't be cleared");

        public bool Contains(KeyValuePair<int, string> item)
            => TryGetValue(item.Key, out var str) && str == item.Value;

        public bool ContainsKey(int key)
            => segmentCache.ContainsKey(key) || proxyObject.HasProperty(key.ToString());

        public void CopyTo(KeyValuePair<int, string>[] array, int arrayIndex)
            => throw new NotImplementedException();

        public IEnumerator<KeyValuePair<int, string>> GetEnumerator()
        {
            CacheAll();
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
            return segmentCache.Where(x => x.Value != null).GetEnumerator();
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
        }

        public bool Remove(int key)
            => throw new InvalidOperationException($"Segments can't be removed");

        public bool Remove(KeyValuePair<int, string> item)
            => throw new InvalidOperationException($"Segments can't be removed");

        public bool TryGetValue(int key, [MaybeNullWhen(false)] out string value)
        {
            if (segmentCache.TryGetValue(key, out var str))
            {
                if (str != null)
                {
                    value = str;
                    return true;
                }
                else
                {
                    value = null;
                    return false;
                }
            }
            str = proxyObject.GetPropertyAsString(key.ToString());
            segmentCache.Add(key, str);
            if (str != null)
            {
                value = str;
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        private void CacheAll()
        {
            for (int i = 0; i < MaxSegmentCount; ++i)
            {
                TryGetValue(i, out _);
            }
        }
    }


    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeRawMemory : IRawMemory
    {
        #region Imports

        [JSImport("rawMemory.get", "game")]
        [return: JSMarshalAsAttribute<JSType.String>]
        internal static partial string Native_Get();

        [JSImport("rawMemory.set", "game")]
        internal static partial void Native_Set([JSMarshalAs<JSType.String>] string value);

        [JSImport("rawMemory.setActiveSegments", "game")]
        internal static partial void Native_SetActiveSegments([JSMarshalAs<JSType.Array<JSType.Number>>] int[] ids);

        [JSImport("rawMemory.setActiveForeignSegment", "game")]
        internal static partial void Native_SetActiveForeignSegment([JSMarshalAs<JSType.String>] string? username, [JSMarshalAs<JSType.Number>] int? id);

        [JSImport("rawMemory.setDefaultPublicSegment", "game")]
        internal static partial void Native_SetDefaultPublicSegment([JSMarshalAs<JSType.Number>] int? id);

        [JSImport("rawMemory.setPublicSegments", "game")]
        internal static partial void Native_SetPublicSegments([JSMarshalAs<JSType.Array<JSType.Number>>] int[] ids);

        #endregion

        private JSObject proxyObject;

        internal JSObject ProxyObject
        {
            get => proxyObject;
            set
            {
                proxyObject = value;
                segmentsCache = null;
                foreignSegmentCache = null;
            }
        }

        private NativeRawMemorySegments? segmentsCache;
        private ForeignSegment? foreignSegmentCache;

        public IDictionary<int, string> Segments => segmentsCache ??= new NativeRawMemorySegments(proxyObject.GetPropertyAsJSObject("segments")!);

        public ForeignSegment? ForeignSegment
        {
            get
            {
                if (foreignSegmentCache != null) { return foreignSegmentCache.Value; }
                var obj = proxyObject.GetPropertyAsJSObject("foreignSegment");
                if (obj == null) { return null; }
                return foreignSegmentCache = new(
                    obj.GetPropertyAsString("username") ?? "",
                    obj.GetPropertyAsInt32("id"),
                    obj.GetPropertyAsString("data") ?? ""
                );
            }
        }

        public NativeRawMemory(JSObject proxyObject)
        {
            this.proxyObject = proxyObject;
        }

        public string Get()
            => Native_Get();

        public void Set(string value)
            => Native_Set(value);

        public void SetActiveSegments(IEnumerable<int> ids)
            => Native_SetActiveSegments(ids.ToArray());

        public void SetActiveForeignSegment(string? username, int? id = null)
            => Native_SetActiveForeignSegment(username, id);

        public void SetDefaultPublicSegment(int? id)
            => Native_SetDefaultPublicSegment(id);

        public void SetPublicSegments(IEnumerable<int> ids)
            => Native_SetPublicSegments(ids.ToArray());

    }
}
