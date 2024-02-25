using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace ScreepsDotNet.Interop
{
    public enum JSPropertyType
    {
        Undefined,
        String,
        Number,
        BigInt,
        Boolean,
        Object,
        Function,
        Symbol,
    }

    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    public partial class JSObject : IDisposable, IEquatable<JSObject?>
    {
        #region Imports

        [JSImport("hasProperty", "__object")]
        private static partial bool HasPropertyOnObject(JSObject obj, string key);

        [JSImport("hasProperty", "__object")]
        private static partial bool HasPropertyOnObject(JSObject obj, Name key);

        [JSImport("getTypeOfProperty", "__object")]
        private static partial int GetTypeOfPropertyOnObject(JSObject obj, string key);

        [JSImport("getTypeOfProperty", "__object")]
        private static partial int GetTypeOfPropertyOnObject(JSObject obj, Name key);

        [JSImport("getKeys", "__object")]
        private static partial ImmutableArray<string> GetKeysOnObject(JSObject obj);

        [JSImport("getKeys", "__object")]
        private static partial ImmutableArray<Name> GetKeysAsNamesOnObject(JSObject obj);

        [JSImport("getProperty", "__object")]
        private static partial bool GetPropertyAsBooleanOnObject(JSObject obj, string key);

        [JSImport("getProperty", "__object")]
        private static partial bool GetPropertyAsBooleanOnObject(JSObject obj, Name key);

        [JSImport("getProperty", "__object")]
        private static partial int GetPropertyAsInt32OnObject(JSObject obj, string key);

        [JSImport("getProperty", "__object")]
        private static partial int GetPropertyAsInt32OnObject(JSObject obj, Name key);

        [JSImport("getProperty", "__object")]
        private static partial double GetPropertyAsDoubleOnObject(JSObject obj, string key);

        [JSImport("getProperty", "__object")]
        private static partial double GetPropertyAsDoubleOnObject(JSObject obj, Name key);

        [JSImport("getProperty", "__object")]
        private static partial bool? GetPropertyAsNullableBooleanOnObject(JSObject obj, string key);

        [JSImport("getProperty", "__object")]
        private static partial bool? GetPropertyAsNullableBooleanOnObject(JSObject obj, Name key);

        [JSImport("getProperty", "__object")]
        private static partial int? GetPropertyAsNullableInt32OnObject(JSObject obj, string key);

        [JSImport("getProperty", "__object")]
        private static partial int? GetPropertyAsNullableInt32OnObject(JSObject obj, Name key);

        [JSImport("getProperty", "__object")]
        private static partial double? GetPropertyAsNullableDoubleOnObject(JSObject obj, string key);

        [JSImport("getProperty", "__object")]
        private static partial double? GetPropertyAsNullableDoubleOnObject(JSObject obj, Name key);

        [JSImport("getProperty", "__object")]
        private static partial string? GetPropertyAsStringOnObject(JSObject obj, string key);

        [JSImport("getProperty", "__object")]
        private static partial string? GetPropertyAsStringOnObject(JSObject obj, Name key);

        [JSImport("getProperty", "__object")]
        private static partial Name GetPropertyAsNameOnObject(JSObject obj, string key);

        [JSImport("getProperty", "__object")]
        private static partial Name GetPropertyAsNameOnObject(JSObject obj, Name key);

        [JSImport("getProperty", "__object")]
        private static partial JSObject? GetPropertyAsJSObjectOnObject(JSObject obj, string key);

        [JSImport("getProperty", "__object")]
        private static partial JSObject? GetPropertyAsJSObjectOnObject(JSObject obj, Name key);

        [JSImport("getProperty", "__object")]
        private static partial byte[]? GetPropertyAsByteArrayOnObject(JSObject obj, string key);

        [JSImport("getProperty", "__object")]
        private static partial byte[]? GetPropertyAsByteArrayOnObject(JSObject obj, Name key);

        [JSImport("setProperty", "__object")]
        private static partial void SetPropertyOnObject(JSObject obj, string key, bool value);

        [JSImport("setProperty", "__object")]
        private static partial void SetPropertyOnObject(JSObject obj, Name key, bool value);

        [JSImport("setProperty", "__object")]
        private static partial void SetPropertyOnObject(JSObject obj, string key, int value);

        [JSImport("setProperty", "__object")]
        private static partial void SetPropertyOnObject(JSObject obj, Name key, int value);

        [JSImport("setProperty", "__object")]
        private static partial void SetPropertyOnObject(JSObject obj, string key, double value);

        [JSImport("setProperty", "__object")]
        private static partial void SetPropertyOnObject(JSObject obj, Name key, double value);

        [JSImport("setProperty", "__object")]
        private static partial void SetPropertyOnObject(JSObject obj, string key, string? value);

        [JSImport("setProperty", "__object")]
        private static partial void SetPropertyOnObject(JSObject obj, Name key, string? value);

        [JSImport("setProperty", "__object")]
        private static partial void SetPropertyOnObject(JSObject obj, string key, Name value);

        [JSImport("setProperty", "__object")]
        private static partial void SetPropertyOnObject(JSObject obj, Name key, Name value);

        [JSImport("setProperty", "__object")]
        private static partial void SetPropertyOnObject(JSObject obj, string key, JSObject? value);

        [JSImport("setProperty", "__object")]
        private static partial void SetPropertyOnObject(JSObject obj, Name key, JSObject? value);

        [JSImport("setProperty", "__object")]
        private static partial void SetPropertyOnObject(JSObject obj, string key, byte[]? value);

        [JSImport("setProperty", "__object")]
        private static partial void SetPropertyOnObject(JSObject obj, Name key, byte[]? value);

        [JSImport("deleteProperty", "__object")]
        private static partial void DeletePropertyOnObject(JSObject obj, string key);

        [JSImport("deleteProperty", "__object")]
        private static partial void DeletePropertyOnObject(JSObject obj, Name key);

        [JSImport("create", "__object")]
        private static partial JSObject CreateObject(JSObject? prototypeObj = null);

        #endregion

        #region Static Interface

        public static JSObject Create(JSObject? prototypeObj = null) => CreateObject(prototypeObj);

        #endregion

        private readonly IntPtr jsHandle;
        private readonly int hash;
        private bool disposedValue;

        internal IntPtr JSHandle => jsHandle;

        public bool IsDisposed => disposedValue;

        internal JSObject(IntPtr jsHandle)
        {
            this.jsHandle = jsHandle;
            hash = HashCode.Combine(jsHandle);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasProperty(string propertyName) => HasPropertyOnObject(this, propertyName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasProperty(Name propertyName) => HasPropertyOnObject(this, propertyName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JSPropertyType GetTypeOfProperty(string propertyName) => (JSPropertyType)GetTypeOfPropertyOnObject(this, propertyName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JSPropertyType GetTypeOfProperty(Name propertyName) => (JSPropertyType)GetTypeOfPropertyOnObject(this, propertyName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ImmutableArray<string> GetPropertyNames() => GetKeysOnObject(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ImmutableArray<Name> GetPropertyNamesAsNames() => GetKeysAsNamesOnObject(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetPropertyAsBoolean(string propertyName) => GetPropertyAsBooleanOnObject(this, propertyName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetPropertyAsBoolean(Name propertyName) => GetPropertyAsBooleanOnObject(this, propertyName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetPropertyAsInt32(string propertyName) => GetPropertyAsInt32OnObject(this, propertyName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetPropertyAsInt32(Name propertyName) => GetPropertyAsInt32OnObject(this, propertyName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double GetPropertyAsDouble(string propertyName) => GetPropertyAsDoubleOnObject(this, propertyName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double GetPropertyAsDouble(Name propertyName) => GetPropertyAsDoubleOnObject(this, propertyName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool? TryGetPropertyAsBoolean(string propertyName) => GetPropertyAsNullableBooleanOnObject(this, propertyName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool? TryGetPropertyAsBoolean(Name propertyName) => GetPropertyAsNullableBooleanOnObject(this, propertyName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int? TryGetPropertyAsInt32(string propertyName) => GetPropertyAsNullableInt32OnObject(this, propertyName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int? TryGetPropertyAsInt32(Name propertyName) => GetPropertyAsNullableInt32OnObject(this, propertyName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double? TryGetPropertyAsDouble(string propertyName) => GetPropertyAsNullableDoubleOnObject(this, propertyName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double? TryGetPropertyAsDouble(Name propertyName) => GetPropertyAsNullableDoubleOnObject(this, propertyName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string? GetPropertyAsString(string propertyName) => GetPropertyAsStringOnObject(this, propertyName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string? GetPropertyAsString(Name propertyName) => GetPropertyAsStringOnObject(this, propertyName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Name GetPropertyAsName(string propertyName) => GetPropertyAsNameOnObject(this, propertyName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Name GetPropertyAsName(Name propertyName) => GetPropertyAsNameOnObject(this, propertyName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JSObject? GetPropertyAsJSObject(string propertyName) => GetPropertyAsJSObjectOnObject(this, propertyName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JSObject? GetPropertyAsJSObject(Name propertyName) => GetPropertyAsJSObjectOnObject(this, propertyName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[]? GetPropertyAsByteArray(string propertyName) => GetPropertyAsByteArrayOnObject(this, propertyName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[]? GetPropertyAsByteArray(Name propertyName) => GetPropertyAsByteArrayOnObject(this, propertyName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetProperty(string propertyName, bool value) => SetPropertyOnObject(this, propertyName, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetProperty(Name propertyName, bool value) => SetPropertyOnObject(this, propertyName, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetProperty(string propertyName, int value) => SetPropertyOnObject(this, propertyName, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetProperty(Name propertyName, int value) => SetPropertyOnObject(this, propertyName, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetProperty(string propertyName, double value) => SetPropertyOnObject(this, propertyName, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetProperty(Name propertyName, double value) => SetPropertyOnObject(this, propertyName, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetProperty(string propertyName, string? value) => SetPropertyOnObject(this, propertyName, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetProperty(Name propertyName, string? value) => SetPropertyOnObject(this, propertyName, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetProperty(string propertyName, Name value) => SetPropertyOnObject(this, propertyName, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetProperty(Name propertyName, Name value) => SetPropertyOnObject(this, propertyName, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetProperty(string propertyName, JSObject? value) => SetPropertyOnObject(this, propertyName, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetProperty(Name propertyName, JSObject? value) => SetPropertyOnObject(this, propertyName, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetProperty(string propertyName, byte[]? value) => SetPropertyOnObject(this, propertyName, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetProperty(Name propertyName, byte[]? value) => SetPropertyOnObject(this, propertyName, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DeleteProperty(string propertyName) => DeletePropertyOnObject(this, propertyName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DeleteProperty(Name propertyName) => DeletePropertyOnObject(this, propertyName);

        public override string ToString()
            => $"JSObject[{jsHandle}]{(disposedValue ? " (DISPOSED)" : "")}";

        public override bool Equals(object? obj) => Equals(obj as JSObject);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(JSObject? other) => other is not null && jsHandle == other.jsHandle;

        public override int GetHashCode() => hash;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(JSObject? left, JSObject? right) => left is null ? right is null : left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(JSObject? left, JSObject? right) => !(left == right);

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                Native.ReleaseJSObject(jsHandle);
                disposedValue = true;
            }
        }

        ~JSObject()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
