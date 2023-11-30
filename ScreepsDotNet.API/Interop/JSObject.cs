using System;

namespace ScreepsDotNet.Interop
{
    public class JSObject : IDisposable
    {
        private readonly IntPtr jsHandle;
        private bool disposedValue;

        internal IntPtr JSHandle => jsHandle;

        internal JSObject(IntPtr jsHandle)
        {
            this.jsHandle = jsHandle;
        }

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: release reference to js object stored in jsHandle
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
