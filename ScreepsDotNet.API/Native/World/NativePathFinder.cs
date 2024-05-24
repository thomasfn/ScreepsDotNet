using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

using ScreepsDotNet.Interop;

using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal static partial class SearchPathOptionsExtensions
    {
        #region Imports

        [JSImport("PathFinder.getRoomCallbackObject", "game/prototypes/wrapped")]
        internal static partial JSObject Native_GetRoomCallbackObject();

        [JSImport("PathFinder.getCostCallbackObject", "game/prototypes/wrapped")]
        internal static partial JSObject Native_GetCostCallbackObject();

        #endregion

        private static JSObject? roomCallbackObject;
        private static JSObject? costCallbackObject;

        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicMethods, typeof(NativeCallbacks))]
        public static JSObject ToJS(this SearchPathOptions searchPathOptions)
        {
            var obj = JSObject.Create();
            if (searchPathOptions.RoomCallback != null)
            {
                NativeCallbacks.currentRoomCallbackFunc = searchPathOptions.RoomCallback;
                obj.SetProperty(Names.RoomCallback, roomCallbackObject ??= Native_GetRoomCallbackObject());
            }
            if (searchPathOptions.PlainCost != null) { obj.SetProperty(Names.PlainCost, searchPathOptions.PlainCost.Value); }
            if (searchPathOptions.SwampCost != null) { obj.SetProperty(Names.SwampCost, searchPathOptions.SwampCost.Value); }
            if (searchPathOptions.Flee != null) { obj.SetProperty(Names.Flee, searchPathOptions.Flee.Value); }
            if (searchPathOptions.MaxOps != null) { obj.SetProperty(Names.MaxOps, searchPathOptions.MaxOps.Value); }
            if (searchPathOptions.MaxRooms != null) { obj.SetProperty(Names.MaxRooms, searchPathOptions.MaxRooms.Value); }
            if (searchPathOptions.MaxCost != null) { obj.SetProperty(Names.MaxCost, searchPathOptions.MaxCost.Value); }
            if (searchPathOptions.HeuristicWeight != null) { obj.SetProperty(Names.HeuristicWeight, searchPathOptions.HeuristicWeight.Value); }
            return obj;
        }

        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicMethods, typeof(NativeCallbacks))]
        public static JSObject ToJS(this FindPathOptions findPathOptions)
        {
            var obj = JSObject.Create();
            if (findPathOptions.IgnoreCreeps != null) { obj.SetProperty(Names.IgnoreCreeps, findPathOptions.IgnoreCreeps.Value); }
            if (findPathOptions.IgnoreDestructibleStructures != null) { obj.SetProperty(Names.IgnoreDestructibleStructures, findPathOptions.IgnoreDestructibleStructures.Value); }
            if (findPathOptions.IgnoreRoads != null) { obj.SetProperty(Names.IgnoreRoads, findPathOptions.IgnoreRoads.Value); }
            if (findPathOptions.CostCallback != null)
            {
                NativeCallbacks.currentCostCallbackFunc = findPathOptions.CostCallback;
                obj.SetProperty(Names.CostCallback, costCallbackObject ??= Native_GetCostCallbackObject());
            }
            if (findPathOptions.Ignore != null) { JSUtils.SetObjectArrayOnObject(obj, Names.Ignore, findPathOptions.Ignore.Select(x => x.ToJS()).ToArray()); }
            if (findPathOptions.Avoid != null) { JSUtils.SetObjectArrayOnObject(obj, Names.Avoid, findPathOptions.Avoid.Select(x => x.ToJS()).ToArray()); }
            if (findPathOptions.MaxOps != null) { obj.SetProperty(Names.MaxOps, findPathOptions.MaxOps.Value); }
            if (findPathOptions.HeuristicWeight != null) { obj.SetProperty(Names.HeuristicWeight, findPathOptions.HeuristicWeight.Value); }
            if (findPathOptions.MaxRooms != null) { obj.SetProperty(Names.MaxRooms, findPathOptions.MaxRooms.Value); }
            if (findPathOptions.Range != null) { obj.SetProperty(Names.Range, findPathOptions.Range.Value); }
            if (findPathOptions.PlainCost != null) { obj.SetProperty(Names.PlainCost, findPathOptions.PlainCost.Value); }
            if (findPathOptions.SwampCost != null) { obj.SetProperty(Names.SwampCost, findPathOptions.SwampCost.Value); }
            return obj;
        }

        public static JSObject ToJS(this Goal goal)
        {
            var obj = JSObject.Create();
            using var posJs = goal.Position.ToJS();
            obj.SetProperty(Names.Pos, posJs);
            obj.SetProperty(Names.Range, goal.Range);
            return obj;
        }

        public static JSObject ToJS(this PathStep pathStep)
            => pathStep.Position.ToJS();

        public static SearchPathResult ToSearchPathResult(this JSObject obj) => new(
                JSUtils.GetObjectArrayOnObject(obj, Names.Path)?.Select(x => x.ToRoomPosition()).ToArray() ?? ReadOnlySpan<RoomPosition>.Empty,
                (int)obj.GetPropertyAsDouble(Names.Ops),
                (int)obj.GetPropertyAsDouble(Names.Cost),
                obj.GetPropertyAsBoolean(Names.Incomplete)
            );

        public static PathStep ToPathStep(this JSObject obj) => new(
                obj.GetPropertyAsInt32(Names.X),
                obj.GetPropertyAsInt32(Names.Y),
                obj.GetPropertyAsInt32(Names.Dx),
                obj.GetPropertyAsInt32(Names.Dy),
                (Direction)obj.GetPropertyAsInt32(Names.Direction)
            );
    }

    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativePathFinder : IPathFinder
    {
        #region Imports

        [JSImport("PathFinder.search", "game/prototypes/wrapped")]
        
        internal static partial JSObject Native_Search(JSObject origin, JSObject goal, JSObject? opts);

        [JSImport("PathFinder.search", "game/prototypes/wrapped")]
        
        internal static partial JSObject Native_Search(JSObject origin, JSObject[] goals, JSObject? opts);

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
                goalsJS.DisposeAll();
            }
        }
    }
}
