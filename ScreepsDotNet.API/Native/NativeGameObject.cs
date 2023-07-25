using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.CompilerServices;

using ScreepsDotNet.API;

namespace ScreepsDotNet.Native
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeGameObject : IGameObject
    {
        internal readonly JSObject ProxyObject;

        public bool Exists => ProxyObject.GetPropertyAsBoolean("exists");

        public string Id => ProxyObject.GetTypeOfProperty("id") == "number" ? ProxyObject.GetPropertyAsInt32("id").ToString() : (ProxyObject.GetPropertyAsString("id") ?? ProxyObject.ToString() ?? "");

        public int? TicksToDecay => ProxyObject.GetTypeOfProperty("ticksToDecay") == "number" ? ProxyObject.GetPropertyAsInt32("ticksToDecay") : null;

        public int X => ProxyObject.GetPropertyAsInt32("x");

        public int Y => ProxyObject.GetPropertyAsInt32("y");

        public Position Position => new(X, Y);

        public NativeGameObject(JSObject wrappedJsObject)
        {
            this.ProxyObject = wrappedJsObject;
        }

        public T? FindClosestByPath<T>(IEnumerable<T> positions, object? options) where T : IPosition
        {
            throw new NotImplementedException();
        }

        public override string ToString()
            => $"GameObject({Id}, {Position})";
    }

    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal static class NativeGameObjectPrototypes<T> where T : IGameObject
    {
        public static JSObject? ConstructorObj;

        static NativeGameObjectPrototypes()
        {
            RuntimeHelpers.RunClassConstructor(typeof(NativeGameObjectUtils).TypeHandle);
        }
    }

    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal static partial class NativeGameObjectUtils
    {
        private static readonly JSObject prototypesObject;
        private static readonly IList<Type> prototypeTypeMappings = new List<Type>();

        [JSImport("getUtils", "game")]
        [return: JSMarshalAsAttribute<JSType.Object>]
        internal static partial JSObject GetUtilsObject();

        [JSImport("getPrototypes", "game")]
        [return: JSMarshalAsAttribute<JSType.Object>]
        internal static partial JSObject GetPrototypesObject();

        [JSImport("getConstructorOf", "object")]
        [return: JSMarshalAsAttribute<JSType.Object>]
        internal static partial JSObject GetConstructorOf([JSMarshalAs<JSType.Object>] JSObject obj);

        [JSImport("create", "object")]
        [return: JSMarshalAsAttribute<JSType.Object>]
        internal static partial JSObject Create([JSMarshalAs<JSType.Object>] JSObject? prototype);

        internal static void RegisterPrototypeTypeMapping<TInterface, TConcrete>(string prototypeName)
            where TInterface : IGameObject
            where TConcrete : NativeGameObject
        {
            var constructor = prototypesObject.GetPropertyAsJSObject(prototypeName);
            if (constructor == null)
            {
                Console.WriteLine($"Failed to retrieve constructor for '{prototypeName}'");
                return;
            }
            int typeId = prototypeTypeMappings.Count;
            constructor.SetProperty("__dotnet_typeId", typeId + 1);
            prototypeTypeMappings.Add(typeof(TConcrete));
            NativeGameObjectPrototypes<TInterface>.ConstructorObj = constructor;
            NativeGameObjectPrototypes<TConcrete>.ConstructorObj = constructor;
        }

        internal static Type? GetWrapperTypeForConstructor(JSObject constructor)
        {
            int typeId = constructor.GetPropertyAsInt32("__dotnet_typeId") - 1;
            if (typeId < 0)
            {
                Console.WriteLine($"Failed to retrieve wrapper type for {constructor} - typeId not found");
                return null;
            }
            return prototypeTypeMappings[typeId];
        }

        internal static Type? GetWrapperTypeForObject(JSObject jsObject)
            => GetWrapperTypeForConstructor(GetConstructorOf(jsObject));

        internal static NativeGameObject CreateWrapperForObject(JSObject proxyObject)
        {
            var wrapperType = GetWrapperTypeForObject(proxyObject);
            if (wrapperType == null) { return new NativeGameObject(proxyObject); }
            return (Activator.CreateInstance(wrapperType, new object[] { proxyObject }) as NativeGameObject)!;
        }

        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeCreep))]
        static NativeGameObjectUtils()
        {
            prototypesObject = GetPrototypesObject();
            RegisterPrototypeTypeMapping<ICreep, NativeCreep>("Creep");
        }
    }

    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal static class NativeGameObjectExtensions
    {
        public static JSObject ToJS(this IPosition position)
        {
            if (position is NativeGameObject nativeGameObject) { return nativeGameObject.ProxyObject; }
            var obj = NativeGameObjectUtils.Create(null);
            obj.SetProperty("x", position.X);
            obj.SetProperty("y", position.Y);
            return obj;
        }
    }
}
