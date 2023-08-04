using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal static partial class SearchPathOptionsExtensions
    {
        #region Imports

        [JSImport("set", "object")]
        internal static partial void SetRoomCallbackOnObject([JSMarshalAs<JSType.Object>] JSObject obj, [JSMarshalAs<JSType.String>] string key, [JSMarshalAs<JSType.Function<JSType.String, JSType.Object>>] Func<string, JSObject?> fn);

        [JSImport("set", "object")]
        internal static partial void SetCostCallbackOnObject([JSMarshalAs<JSType.Object>] JSObject obj, [JSMarshalAs<JSType.String>] string key, [JSMarshalAs<JSType.Function<JSType.String, JSType.Object, JSType.Object>>] Func<string, JSObject, JSObject?> fn);

        #endregion

        public static JSObject ToJS(this SearchPathOptions searchPathOptions)
        {
            var obj = NativeRoomObjectUtils.CreateObject(null);
            if (searchPathOptions.RoomCallback != null) { SetRoomCallbackOnObject(obj, "roomCallback", roomName => (searchPathOptions.RoomCallback(roomName) as NativeCostMatrix)?.ProxyObject); }
            if (searchPathOptions.PlainCost != null) { obj.SetProperty("plainCost", searchPathOptions.PlainCost.Value); }
            if (searchPathOptions.SwampCost != null) { obj.SetProperty("swampCost", searchPathOptions.SwampCost.Value); }
            if (searchPathOptions.Flee != null) { obj.SetProperty("flee", searchPathOptions.Flee.Value); }
            if (searchPathOptions.MaxOps != null) { obj.SetProperty("maxOps", searchPathOptions.MaxOps.Value); }
            if (searchPathOptions.MaxRooms != null) { obj.SetProperty("maxRooms", searchPathOptions.MaxRooms.Value); }
            if (searchPathOptions.MaxCost != null) { obj.SetProperty("maxCost", searchPathOptions.MaxCost.Value); }
            if (searchPathOptions.HeuristicWeight != null) { obj.SetProperty("heuristicWeight", searchPathOptions.HeuristicWeight.Value); }
            return obj;
        }

        public static JSObject ToJS(this FindPathOptions findPathOptions)
        {
            var obj = NativeRoomObjectUtils.CreateObject(null);
            if (findPathOptions.IgnoreCreeps != null) { obj.SetProperty("ignoreCreeps", findPathOptions.IgnoreCreeps.Value); }
            if (findPathOptions.IgnoreDestructibleStructures != null) { obj.SetProperty("ignoreDestructibleStructures", findPathOptions.IgnoreDestructibleStructures.Value); }
            if (findPathOptions.IgnoreRoads != null) { obj.SetProperty("ignoreRoads", findPathOptions.IgnoreRoads.Value); }
            if (findPathOptions.CostCallback != null) { SetCostCallbackOnObject(obj, "costCallback", (roomName, costMatrixJs) => (findPathOptions.CostCallback(roomName, new NativeCostMatrix(costMatrixJs)) as NativeCostMatrix)?.ProxyObject); }
            if (findPathOptions.Ignore != null) { NativeRoomObjectUtils.SetObjectArrayOnObject(obj, "ignore", findPathOptions.Ignore.Select(x => x.ToJS()).ToArray()); }
            if (findPathOptions.Avoid != null) { NativeRoomObjectUtils.SetObjectArrayOnObject(obj, "avoid", findPathOptions.Avoid.Select(x => x.ToJS()).ToArray()); }
            if (findPathOptions.MaxOps != null) { obj.SetProperty("maxOps", findPathOptions.MaxOps.Value); }
            if (findPathOptions.HeuristicWeight != null) { obj.SetProperty("heuristicWeight", findPathOptions.HeuristicWeight.Value); }
            if (findPathOptions.MaxRooms != null) { obj.SetProperty("maxRooms", findPathOptions.MaxRooms.Value); }
            if (findPathOptions.Range != null) { obj.SetProperty("range", findPathOptions.Range.Value); }
            if (findPathOptions.PlainCost != null) { obj.SetProperty("plainCost", findPathOptions.PlainCost.Value); }
            if (findPathOptions.SwampCost != null) { obj.SetProperty("swampCost", findPathOptions.SwampCost.Value); }
            return obj;
        }

        public static JSObject ToJS(this Goal goal)
        {
            var obj = NativeRoomObjectUtils.CreateObject(null);
            using var posJs = goal.Position.ToJS();
            obj.SetProperty("pos", posJs);
            obj.SetProperty("range", goal.Range);
            return obj;
        }

        public static JSObject ToJS(this PathStep pathStep)
            => pathStep.Position.ToJS();

        public static SearchPathResult ToSearchPathResult(this JSObject obj) => new(
                NativeRoomObjectUtils.GetObjectArrayOnObject(obj, "path")!.Select(x => x.ToRoomPosition()).ToArray(),
                obj.GetPropertyAsInt32("ops"),
                obj.GetPropertyAsInt32("cost"),
                obj.GetPropertyAsBoolean("incomplete")
            );

        public static PathStep ToPathStep(this JSObject obj) => new(
                obj.GetPropertyAsInt32("x"),
                obj.GetPropertyAsInt32("y"),
                obj.GetPropertyAsInt32("dx"),
                obj.GetPropertyAsInt32("dy"),
                (Direction)obj.GetPropertyAsInt32("direction")
            );
    }

    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativePathFinder : IPathFinder
    {
        #region Imports

        [JSImport("PathFinder.search", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Object>]
        internal static partial JSObject Native_Search([JSMarshalAs<JSType.Object>] JSObject origin, [JSMarshalAs<JSType.Object>] JSObject goal, [JSMarshalAs<JSType.Object>] JSObject? opts);

        [JSImport("PathFinder.search", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Object>]
        internal static partial JSObject Native_Search([JSMarshalAs<JSType.Object>] JSObject origin, [JSMarshalAs<JSType.Array<JSType.Object>>] JSObject[] goals, [JSMarshalAs<JSType.Object>] JSObject? opts);

        #endregion

        public SearchPathResult Search(RoomPosition origin, Goal goal, SearchPathOptions? opts = null)
        {
            using var originJs = origin.ToJS();
            using var goalJs = goal.ToJS();
            using var optsJs = opts?.ToJS();
            using var resultJs = Native_Search(originJs, goalJs, optsJs);
            return resultJs.ToSearchPathResult();
        }

        public SearchPathResult Search(RoomPosition origin, IEnumerable<Goal> goals, SearchPathOptions? opts = null)
        {
            using var originJs = origin.ToJS();
            using var optsJs = opts?.ToJS();
            var goalsJS = goals.Select(x => x.ToJS()).ToArray();
            try
            {
                using var resultJs = Native_Search(originJs, goalsJS, optsJs);
                return resultJs.ToSearchPathResult();
            }
            finally
            {
                foreach (var goalJs in goalsJS)
                {
                    goalJs.Dispose();
                }
            }
        }
    }
}
