using ScreepsDotNet.Interop;

using ScreepsDotNet.API.Arena;

namespace ScreepsDotNet.Native.Arena
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeConstructionSite : NativeGameObject, IConstructionSite
    {
        #region Imports

        [JSImport("ConstructionSite.remove", "game/prototypes/wrapped")]
        
        internal static partial int Native_Remove(JSObject proxyObject);

        #endregion

        public int Progress => ProxyObject.GetPropertyAsInt32("progress");

        public int ProgressTotal => ProxyObject.GetPropertyAsInt32("progressTotal");

        public IStructure? Structure
        {
            get
            {
                var obj = ProxyObject.GetPropertyAsJSObject("structure");
                if (obj == null) {  return null; }
                return NativeGameObjectUtils.CreateWrapperForObject(obj) as IStructure;
            }
        }

        public bool My => ProxyObject.GetPropertyAsBoolean("my");

        public NativeConstructionSite(JSObject proxyObject)
            : base(proxyObject)
        { }

        public void Remove()
            => Native_Remove(ProxyObject);

        public override string ToString()
            => $"ConstructionSite({Id}, {Position})";
    }
}
