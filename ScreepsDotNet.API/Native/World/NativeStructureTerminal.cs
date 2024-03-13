using System;
using ScreepsDotNet.Interop;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeStructureTerminal : NativeOwnedStructureWithStore, IStructureTerminal
    {
        #region Imports

        [JSImport("StructureTerminal.send", "game/prototypes/wrapped")]
        internal static partial int Native_Send(JSObject proxyObject, Name resourceType, int amount, string destination, string? description);

        #endregion

        public int Cooldown => ProxyObject.GetPropertyAsInt32(Names.Cooldown);

        public NativeStructureTerminal(INativeRoot nativeRoot, JSObject proxyObject)
            : base(nativeRoot, proxyObject)
        { }

        public TerminalSendResult Send(ResourceType resourceType, int amount, string destination, string? description = null)
            => (TerminalSendResult)Native_Send(ProxyObject, resourceType.ToJS(), amount, destination, description);

        public override string ToString()
            => $"StructureTerminal[{Id}]";
    }
}
