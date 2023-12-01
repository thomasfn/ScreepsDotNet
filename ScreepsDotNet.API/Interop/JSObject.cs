using System;
using System.Collections.Generic;

namespace ScreepsDotNet.Interop
{
    public class JSObject : IDisposable, IEquatable<JSObject?>
    {
        private readonly IntPtr jsHandle;
        private bool disposedValue;

        internal IntPtr JSHandle => jsHandle;

        internal JSObject(IntPtr jsHandle)
        {
            this.jsHandle = jsHandle;
        }

        public override string ToString()
            => $"JSObject[{jsHandle}]{(disposedValue ? " (DISPOSED)" : "")}";

        public override bool Equals(object? obj) => Equals(obj as JSObject);

        public bool Equals(JSObject? other) => other is not null && jsHandle.Equals(other.jsHandle);

        public override int GetHashCode() => HashCode.Combine(jsHandle);

        public static bool operator ==(JSObject? left, JSObject? right) => EqualityComparer<JSObject>.Default.Equals(left, right);

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
