using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeRoomVisual : IRoomVisual
    {
        #region Imports

        [JSImport("createRoomVisual", "game")]
        [return: JSMarshalAs<JSType.Object>]
        internal static partial JSObject Native_Ctor([JSMarshalAs<JSType.String>] string? roomName);

        [JSImport("RoomVisual.line", "game/prototypes/wrapped")]
        internal static partial void Native_Line([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Object>] JSObject pos1, [JSMarshalAs<JSType.Object>] JSObject pos2, [JSMarshalAs<JSType.Object>] JSObject? style);

        [JSImport("RoomVisual.circle", "game/prototypes/wrapped")]
        internal static partial void Native_Circle([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Object>] JSObject position, [JSMarshalAs<JSType.Object>] JSObject? style);

        [JSImport("RoomVisual.rect", "game/prototypes/wrapped")]
        internal static partial void Native_Rect([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Object>] JSObject position, [JSMarshalAs<JSType.Number>] double w, [JSMarshalAs<JSType.Number>] double h, [JSMarshalAs<JSType.Object>] JSObject? style);

        [JSImport("RoomVisual.poly", "game/prototypes/wrapped")]
        internal static partial void Native_Poly([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Array<JSType.Object>>] JSObject[] positions, [JSMarshalAs<JSType.Object>] JSObject? style);

        [JSImport("RoomVisual.text", "game/prototypes/wrapped")]
        internal static partial void Native_Text([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.String>] string text, [JSMarshalAs<JSType.Object>] JSObject position, [JSMarshalAs<JSType.Object>] JSObject? style);

        [JSImport("RoomVisual.clear", "game/prototypes/wrapped")]
        internal static partial void Native_Clear([JSMarshalAs<JSType.Object>] JSObject proxyObject);

        [JSImport("RoomVisual.getSize", "game/prototypes/wrapped")]
        [return: JSMarshalAs<JSType.Number>]
        internal static partial int Native_GetSize([JSMarshalAs<JSType.Object>] JSObject proxyObject);

        [JSImport("RoomVisual.export", "game/prototypes/wrapped")]
        [return: JSMarshalAs<JSType.String>]
        internal static partial string Native_Export([JSMarshalAs<JSType.Object>] JSObject proxyObject);

        [JSImport("RoomVisual.import", "game/prototypes/wrapped")]
        internal static partial void Native_Import([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.String>] string value);

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

        public string Export()
            => Native_Export(ProxyObject);

        public void Import(string str)
            => Native_Import(ProxyObject, str);
    }
}
