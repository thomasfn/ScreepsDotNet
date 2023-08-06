using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API;
using ScreepsDotNet.API.Arena;

namespace ScreepsDotNet.Native.Arena
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeVisual : IVisual
    {
        #region Imports

        [JSImport("Visual.circle", "game/visual")]
        internal static partial void Native_Circle([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Object>] JSObject position, [JSMarshalAs<JSType.Object>] JSObject? style);

        [JSImport("Visual.clear", "game/visual")]
        internal static partial void Native_Clear([JSMarshalAs<JSType.Object>] JSObject proxyObject);

        [JSImport("Visual.line", "game/visual")]
        internal static partial void Native_Line([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Object>] JSObject pos1, [JSMarshalAs<JSType.Object>] JSObject pos2, [JSMarshalAs<JSType.Object>] JSObject? style);

        [JSImport("Visual.poly", "game/visual")]
        internal static partial void Native_Poly([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Array<JSType.Object>>] JSObject[] positions, [JSMarshalAs<JSType.Object>] JSObject? style);

        [JSImport("Visual.rect", "game/visual")]
        internal static partial void Native_Rect([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Object>] JSObject position, [JSMarshalAs<JSType.Number>] double w, [JSMarshalAs<JSType.Number>] double h, [JSMarshalAs<JSType.Object>] JSObject? style);

        [JSImport("Visual.size", "game/visual")]
        [return: JSMarshalAs<JSType.Number>]
        internal static partial int Native_Size([JSMarshalAs<JSType.Object>] JSObject proxyObject);

        [JSImport("Visual.text", "game/visual")]
        internal static partial void Native_Text([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.String>] string text, [JSMarshalAs<JSType.Object>] JSObject position, [JSMarshalAs<JSType.Object>] JSObject? style);

        #endregion

        internal readonly JSObject ProxyObject;

        public int Layer => ProxyObject.GetPropertyAsInt32("layer");

        public bool Persistent => ProxyObject.GetPropertyAsBoolean("persistent");

        public NativeVisual(JSObject proxyObject)
        {
            ProxyObject = proxyObject;
        }

        public IVisual Circle(FractionalPosition position, CircleVisualStyle? style = null)
        {
            using var positionJs = position.ToJS();
            using var styleJs = style?.ToJS();
            Native_Circle(ProxyObject, positionJs, styleJs);
            return this;
        }

        public IVisual Clear()
        {
            Native_Clear(ProxyObject);
            return this;
        }

        public IVisual Line(FractionalPosition pos1, FractionalPosition pos2, LineVisualStyle? style = null)
        {
            using var pos1Js = pos1.ToJS();
            using var pos2Js = pos2.ToJS();
            using var styleJs = style?.ToJS();
            Native_Line(ProxyObject, pos1Js, pos2Js, styleJs);
            return this;
        }

        public IVisual Poly(IEnumerable<FractionalPosition> points, PolyVisualStyle? style = null)
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

        public IVisual Rect(FractionalPosition pos, double w, double h, RectVisualStyle? style = null)
        {
            using var posJs = pos.ToJS();
            using var styleJs = style?.ToJS();
            Native_Rect(ProxyObject, posJs, w, h, styleJs);
            return this;
        }

        public int Size()
            => Native_Size(ProxyObject);

        public IVisual Text(string text, FractionalPosition pos, TextVisualStyle? style = null)
        {
            using var posJs = pos.ToJS();
            using var styleJs = style?.ToJS();
            Native_Text(ProxyObject, text, posJs, styleJs);
            return this;
        }
    }
}
