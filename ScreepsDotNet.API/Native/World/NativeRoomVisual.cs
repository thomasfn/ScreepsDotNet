using System;
using System.Collections.Generic;
using System.Linq;
using ScreepsDotNet.Interop;

using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeRoomVisual : IRoomVisual
    {
        #region Imports

        [JSImport("createRoomVisual", "game")]
        internal static partial JSObject Native_Ctor(string? roomName);

        [JSImport("RoomVisual.line", "game/prototypes/wrapped")]
        internal static partial void Native_Line(JSObject proxyObject, JSObject pos1, JSObject pos2, JSObject? style);

        [JSImport("RoomVisual.circle", "game/prototypes/wrapped")]
        internal static partial void Native_Circle(JSObject proxyObject, JSObject position, JSObject? style);

        [JSImport("RoomVisual.rect", "game/prototypes/wrapped")]
        internal static partial void Native_Rect(JSObject proxyObject, JSObject position, double w, double h, JSObject? style);

        [JSImport("RoomVisual.poly", "game/prototypes/wrapped")]
        internal static partial void Native_Poly(JSObject proxyObject, JSObject[] positions, JSObject? style);

        [JSImport("RoomVisual.text", "game/prototypes/wrapped")]
        internal static partial void Native_Text(JSObject proxyObject, string text, JSObject position, JSObject? style);

        [JSImport("RoomVisual.clear", "game/prototypes/wrapped")]
        internal static partial void Native_Clear(JSObject proxyObject);

        [JSImport("RoomVisual.getSize", "game/prototypes/wrapped")]
        internal static partial int Native_GetSize(JSObject proxyObject);

        [JSImport("RoomVisual.export", "game/prototypes/wrapped")]
        internal static partial string? Native_Export(JSObject proxyObject);

        [JSImport("RoomVisual.import", "game/prototypes/wrapped")]
        internal static partial void Native_Import(JSObject proxyObject, string value);

        #endregion

        internal readonly JSObject ProxyObject;

        public string RoomName => ProxyObject.GetPropertyAsString("roomName")!;

        public NativeRoomVisual(JSObject proxyObject)
        {
            ProxyObject = proxyObject;
        }

        public NativeRoomVisual(string? roomName = null)
            : this(Native_Ctor(roomName))
        { }

        public IRoomVisual Line(FractionalPosition pos1, FractionalPosition pos2, LineVisualStyle? style = null)
        {
            using var pos1Js = pos1.ToJS();
            using var pos2Js = pos2.ToJS();
            using var styleJs = style?.ToJS();
            Native_Line(ProxyObject, pos1Js, pos2Js, styleJs);
            return this;
        }

        public IRoomVisual Circle(FractionalPosition position, CircleVisualStyle? style = null)
        {
            using var positionJs = position.ToJS();
            using var styleJs = style?.ToJS();
            Native_Circle(ProxyObject, positionJs, styleJs);
            return this;
        }

        public IRoomVisual Rect(FractionalPosition pos, double w, double h, RectVisualStyle? style = null)
        {
            using var posJs = pos.ToJS();
            using var styleJs = style?.ToJS();
            Native_Rect(ProxyObject, posJs, w, h, styleJs);
            return this;
        }

        public IRoomVisual Poly(IEnumerable<FractionalPosition> points, PolyVisualStyle? style = null)
        {
            using var styleJs = style?.ToJS();
            var pointsJs = points.Select(x => x.ToJS()).ToArray();
            try
            {
                Native_Poly(ProxyObject, pointsJs, styleJs);
            }
            finally
            {
                pointsJs.DisposeAll();
            }
            return this;
        }

        public IRoomVisual Text(string text, FractionalPosition pos, TextVisualStyle? style = null)
        {
            using var posJs = pos.ToJS();
            using var styleJs = style?.ToJS();
            Native_Text(ProxyObject, text, posJs, styleJs);
            return this;
        }

        public IRoomVisual Clear()
        {
            Native_Clear(ProxyObject);
            return this;
        }

        public int GetSize()
            => Native_GetSize(ProxyObject);

        public string? Export()
            => Native_Export(ProxyObject);

        public void Import(string str)
            => Native_Import(ProxyObject, str);
    }
}
