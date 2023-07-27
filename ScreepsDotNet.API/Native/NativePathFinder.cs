using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;
using System.Linq;

using ScreepsDotNet.API;

namespace ScreepsDotNet.Native
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal static class PathOptionsExtensions
    {
        public static JSObject ToJS(this SearchPathOptions options)
        {
            var obj = NativeGameObjectUtils.CreateObject(null);
            // obj.SetProperty("costMatrix", )
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
            NativeGameObjectUtils.SetArrayOnObject(obj, "ignore", ignore);
            return obj;
        }

        public static JSObject? ToJS(this FindPathOptions? options)
            => options?.ToJS();

        public static JSObject ToJS(this Goal goal)
        {
            var obj = NativeGameObjectUtils.CreateObject(null);
            obj.SetProperty("pos", goal.Position.ToJS());
            if (goal.Range != null) { obj.SetProperty("range", goal.Range.Value); }
            return obj;
        }
    }

    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal static class SearchPathResultExtensions
    {
        public static SearchPathResult ToSearchPathResult(this JSObject obj)
            => new(
                    (NativeGameObjectUtils.GetArrayOnObject(obj, "path")?.Select(x => x.ToPosition()) ?? Enumerable.Empty<Position>()).ToArray(),
                    obj.GetPropertyAsInt32("ops"),
                    obj.GetPropertyAsDouble("cost"),
                    obj.GetPropertyAsBoolean("incomplete")
                );
    }

    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativePathFinder : IPathFinder
    {
        #region Imports

        [JSImport("searchPath", "game/pathFinder")]
        [return: JSMarshalAsAttribute<JSType.Object>]
        internal static partial JSObject Native_SearchPath([JSMarshalAs<JSType.Object>] JSObject origin, [JSMarshalAs<JSType.Object>] JSObject goal, [JSMarshalAs<JSType.Object>] JSObject options);

        [JSImport("searchPath", "game/pathFinder")]
        [return: JSMarshalAsAttribute<JSType.Object>]
        internal static partial JSObject Native_SearchPath([JSMarshalAs<JSType.Object>] JSObject origin, [JSMarshalAs<JSType.Array<JSType.Object>>] JSObject[] goals, [JSMarshalAs<JSType.Object>] JSObject options);

        [JSImport("searchPath", "game/pathFinder")]
        [return: JSMarshalAsAttribute<JSType.Object>]
        internal static partial JSObject Native_SearchPath([JSMarshalAs<JSType.Object>] JSObject origin, [JSMarshalAs<JSType.Object>] JSObject goal);

        [JSImport("searchPath", "game/pathFinder")]
        [return: JSMarshalAsAttribute<JSType.Object>]
        internal static partial JSObject Native_SearchPath([JSMarshalAs<JSType.Object>] JSObject origin, [JSMarshalAs<JSType.Array<JSType.Object>>] JSObject[] goals);

        #endregion

        public SearchPathResult SearchPath(Position origin, Goal goal, SearchPathOptions? options)
            => (options != null ? Native_SearchPath(origin.ToJS(), goal.ToJS(), options.Value.ToJS()) : Native_SearchPath(origin.ToJS(), goal.ToJS()))
                .ToSearchPathResult();

        public SearchPathResult SearchPath(Position origin, IEnumerable<Goal> goal, SearchPathOptions? options)
            => (options != null ? Native_SearchPath(origin.ToJS(), goal.Select(x => x.ToJS()).ToArray(), options.Value.ToJS()) : Native_SearchPath(origin.ToJS(), goal.Select(x => x.ToJS()).ToArray()))
                .ToSearchPathResult();
    }
}
