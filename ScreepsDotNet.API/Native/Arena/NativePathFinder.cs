﻿using System.Collections.Generic;
using ScreepsDotNet.Interop;
using System.Linq;

using ScreepsDotNet.API;
using ScreepsDotNet.API.Arena;

namespace ScreepsDotNet.Native.Arena
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal static class PathOptionsExtensions
    {
        public static JSObject ToJS(this SearchPathOptions options)
        {
            var obj = JSObject.Create();
            if (options.CostMatrix is NativeCostMatrix nativeCostMatrix)
            {
                obj.SetProperty("costMatrix", nativeCostMatrix.ProxyObject);
            }
            obj.SetProperty("plainCost", options.PlainCost);
            obj.SetProperty("swampCost", options.SwampCost);
            obj.SetProperty("flee", options.Flee);
            obj.SetProperty("maxOps", options.MaxOps);
            obj.SetProperty("maxCost", options.MaxCost);
            obj.SetProperty("heuristicWeight", options.HeuristicWeight);
            return obj;
        }

        public static JSObject? ToJS(this SearchPathOptions? options)
            => options?.ToJS();

        public static JSObject ToJS(this FindPathOptions options)
        {
            var obj = options.BaseOptions.ToJS();
            var ignore = options.Ignore.ToArray().Select(x => x.ToJS()).ToArray();
            JSUtils.SetObjectArrayOnObject(obj, "ignore", ignore);
            return obj;
        }

        public static JSObject? ToJS(this FindPathOptions? options)
            => options?.ToJS();

        public static JSObject ToJS(this Goal goal)
        {
            var obj = JSObject.Create();
            obj.SetProperty("pos", goal.Position.ToJS());
            if (goal.Range != null) { obj.SetProperty("range", goal.Range.Value); }
            return obj;
        }
    }

    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal static class SearchPathResultExtensions
    {
        public static SearchPathResult ToSearchPathResult(this JSObject obj)
            => new(
                    (JSUtils.GetObjectArrayOnObject(obj, "path")?.Select(x => x.ToPosition()) ?? Enumerable.Empty<Position>()).ToArray(),
                    obj.GetPropertyAsInt32("ops"),
                    obj.GetPropertyAsDouble("cost"),
                    obj.GetPropertyAsBoolean("incomplete")
                );
    }

    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativePathFinder : IPathFinder
    {
        #region Imports

        [JSImport("searchPath", "game/pathFinder")]
        
        internal static partial JSObject Native_SearchPath(JSObject origin, JSObject goal, JSObject options);

        [JSImport("searchPath", "game/pathFinder")]
        
        internal static partial JSObject Native_SearchPath(JSObject origin, JSObject[] goals, JSObject options);

        [JSImport("searchPath", "game/pathFinder")]
        
        internal static partial JSObject Native_SearchPath(JSObject origin, JSObject goal);

        [JSImport("searchPath", "game/pathFinder")]
        
        internal static partial JSObject Native_SearchPath(JSObject origin, JSObject[] goals);

        #endregion

        public ICostMatrix CreateCostMatrix()
            => new NativeCostMatrix();

        public SearchPathResult SearchPath(Position origin, Goal goal, SearchPathOptions? options)
            => (options != null ? Native_SearchPath(origin.ToJS(), goal.ToJS(), options.Value.ToJS()) : Native_SearchPath(origin.ToJS(), goal.ToJS()))
                .ToSearchPathResult();

        public SearchPathResult SearchPath(Position origin, IEnumerable<Goal> goal, SearchPathOptions? options)
            => (options != null ? Native_SearchPath(origin.ToJS(), goal.Select(x => x.ToJS()).ToArray(), options.Value.ToJS()) : Native_SearchPath(origin.ToJS(), goal.Select(x => x.ToJS()).ToArray()))
                .ToSearchPathResult();
    }
}
