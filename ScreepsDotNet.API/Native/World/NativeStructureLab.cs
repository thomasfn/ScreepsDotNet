using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeStructureLab : NativeOwnedStructure, IStructureLab
    {
        #region Imports

        [JSImport("StructureLab.boostCreep", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_BoostCreep([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Object>] JSObject creep, [JSMarshalAs<JSType.Number>] int? bodyPartsCount);

        [JSImport("StructureLab.reverseReaction", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_ReverseReaction([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Object>] JSObject lab1, [JSMarshalAs<JSType.Object>] JSObject lab2);

        [JSImport("StructureLab.runReaction", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_RunReaction([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Object>] JSObject lab1, [JSMarshalAs<JSType.Object>] JSObject lab2);

        [JSImport("StructureLab.unboostCreep", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_UnboostCreep([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Object>] JSObject creep);

        #endregion

        private int? cooldownCache;
        private ResourceType? mineralTypeCache;
        private NativeStore? storeCache;

        public int Cooldown => CachePerTick(ref cooldownCache) ??= ProxyObject.GetPropertyAsInt32("cooldown");

        public ResourceType MineralType => CachePerTick(ref mineralTypeCache) ??= ProxyObject.GetPropertyAsString("mineralType")!.ParseResourceType();

        public IStore Store => CachePerTick(ref storeCache) ??= new NativeStore(ProxyObject.GetPropertyAsJSObject("store"));

        public NativeStructureLab(INativeRoot nativeRoot, JSObject? proxyObject, ObjectId id)
            : base(nativeRoot, proxyObject, id)
        { }

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            cooldownCache = null;
            mineralTypeCache = null;
            storeCache = null;
        }

        public LabBoostResult BoostCreep(ICreep creep, int? bodyPartsCount = null)
            => (LabBoostResult)Native_BoostCreep(ProxyObject, creep.ToJS(), bodyPartsCount);

        public LabReactionResult ReverseReaction(IStructureLab lab1, IStructureLab lab2)
            => (LabReactionResult)Native_ReverseReaction(ProxyObject, lab1.ToJS(), lab2.ToJS());

        public LabReactionResult RunReaction(IStructureLab lab1, IStructureLab lab2)
            => (LabReactionResult)Native_RunReaction(ProxyObject, lab1.ToJS(), lab2.ToJS());

        public LabBoostResult UnboostCreep(ICreep creep)
            => (LabBoostResult)Native_UnboostCreep(ProxyObject, creep.ToJS());

        public override string ToString()
            => $"StructureLab[{Id}]";
    }
}
