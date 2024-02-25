using System;
using System.Linq;

using ScreepsDotNet.Interop;

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
                obj.SetProperty(Names.CostMatrix, nativeCostMatrix.ProxyObject);
            }
            obj.SetProperty(Names.PlainCost, options.PlainCost);
            obj.SetProperty(Names.SwampCost, options.SwampCost);
            obj.SetProperty(Names.Flee, options.Flee);
            obj.SetProperty(Names.MaxOps, options.MaxOps);
            obj.SetProperty(Names.MaxCost, options.MaxCost);
            obj.SetProperty(Names.HeuristicWeight, options.HeuristicWeight);
            return obj;
        }

        public static JSObject? ToJS(this SearchPathOptions? options)
            => options?.ToJS();

        public static JSObject ToJS(this FindPathOptions options)
        {
            var obj = options.BaseOptions.ToJS();
            var ignore = options.Ignore.ToArray().Select(x => x.ToJS()).ToArray();
            JSUtils.SetObjectArrayOnObject(obj, Names.Ignore, ignore);
            return obj;
        }

        public static JSObject? ToJS(this FindPathOptions? options)
            => options?.ToJS();
    }

    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal static class SearchPathResultExtensions
    {
        public static SearchPathResult ToSearchPathResult(this JSObject obj, ReadOnlySpan<Position> path)
            => new(
                    path,
                    obj.GetPropertyAsInt32(Names.Ops),
                    obj.GetPropertyAsDouble(Names.Cost),
                    obj.GetPropertyAsBoolean(Names.Incomplete)
                );
    }

    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativePathFinder : IPathFinder
    {
        #region Imports

        [JSImport("searchPath", "game/pathFinder")]
        internal static partial JSObject Native_SearchPath(int origin, IntPtr goalsPtr, int goalsCnt, JSObject? options);

        [JSImport("decodePath", "game/pathFinder")]
        internal static partial int Native_DecodePath(JSObject result, IntPtr outPtr);

        #endregion

        internal static readonly Position[] pathPositionBuffer = new Position[100 * 100];

        public ICostMatrix CreateCostMatrix()
            => new NativeCostMatrix();

        public SearchPathResult SearchPath(Position origin, Goal goal, SearchPathOptions? options = null)
        {
            using var optionsJs = options?.ToJS();
            JSObject? resultObj = null;
            int pathLength;
            try
            {
                unsafe
                {
                    resultObj = Native_SearchPath((origin.X << 16) | origin.Y, (IntPtr)(&goal), 1, optionsJs);
                    fixed (Position* pathPositionBufferPtr = pathPositionBuffer)
                    {
                        pathLength = Native_DecodePath(resultObj, (IntPtr)pathPositionBufferPtr);
                    }
                }
                return resultObj.ToSearchPathResult(pathPositionBuffer.AsSpan()[..pathLength]);
            }
            finally
            {
                resultObj?.Dispose();
            }
        }

        public SearchPathResult SearchPath(Position origin, ReadOnlySpan<Goal> goals, SearchPathOptions? options = null)
        {
            using var optionsJs = options?.ToJS();
            JSObject? resultObj = null;
            int pathLength;
            try
            {
                unsafe
                {
                    fixed (Goal* goalsPtr = goals)
                    {
                        resultObj = Native_SearchPath((origin.X << 16) | origin.Y, (IntPtr)goalsPtr, goals.Length, optionsJs);
                    }
                    fixed (Position* pathPositionBufferPtr = pathPositionBuffer)
                    {
                        pathLength = Native_DecodePath(resultObj, (IntPtr)pathPositionBufferPtr);
                    }
                }
                return resultObj.ToSearchPathResult(pathPositionBuffer.AsSpan()[..pathLength]);
            }
            finally
            {
                resultObj?.Dispose();
            }
        }
    }
}
