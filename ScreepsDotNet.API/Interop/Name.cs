using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ScreepsDotNet.Interop
{
    /// <summary>
    /// Encapsulates a non-changing string that is more efficient to marshal between .net and js.
    /// Names should be created ahead of time and reused for best performance.
    /// Do not use names for long or 'one-off' strings that will only ever be copied to js once.
    /// </summary>
    public readonly struct Name : IEquatable<Name>
    {
        #region Static Interface

        private static readonly Dictionary<string, int> nameLookup = [];
        private static (string value, bool copied)[] nameList = new (string, bool)[16];
        private static int nameCount = 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string? GetNameValue(int nameIndex) => nameIndex >= 0 && nameIndex < nameCount ? nameList[nameIndex].value : null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int AllocateName(string value)
        {
            if (nameCount == nameList.Length) { Array.Resize(ref nameList, nameList.Length * 2); }
            nameList[nameCount] = (value, false);
            int nameIndex = nameCount++;
            nameLookup.TryAdd(value, nameIndex);
            return nameIndex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe void CopyIfNeeded(int nameIndex)
        {
            if (nameIndex < 0 || nameIndex >= nameCount) { throw new ArgumentOutOfRangeException(nameof(nameIndex)); }
            ref (string value, bool copied) nameTuple = ref nameList[nameIndex];
            if (nameTuple.copied) { return; }
            fixed (char* valuePtr = nameTuple.value)
            {
                ScreepsDotNet_Interop.SetName(nameIndex, valuePtr);
            }
            nameTuple.copied = true;
        }

        /// <summary>
        /// Creates a new name.
        /// While it is safe to create a name for the same string multiple times, you should cache names where possible.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Name Create(string value) => nameLookup.TryGetValue(value, out var nameIndex) ? new(nameIndex) : new(AllocateName(value));

        #endregion

        internal readonly int NameIndex;

        /// <summary>
        /// Gets the string value of this name.
        /// </summary>
        public string Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => nameList[NameIndex].value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Name(int nameIndex)
        {
            NameIndex = nameIndex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object? obj) => obj is Name name && Equals(name);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Name other) => NameIndex == other.NameIndex;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => HashCode.Combine(NameIndex);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Name left, Name right) => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Name left, string right) => left.Value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Name left, Name right) => !(left == right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Name left, string right) => !(left.Value == right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => Value;
    }
}
