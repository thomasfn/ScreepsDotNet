using System;
using System.Collections.Generic;
using System.Linq;
using ScreepsDotNet.Interop;

using ScreepsDotNet.API;
using ScreepsDotNet.API.Arena;

namespace ScreepsDotNet.Native.Arena
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeVisual : IVisual
    {
        #region Imports

        [JSImport("Visual.circle", "game/visual")]
        internal static partial void Native_Circle(JSObject proxyObject, JSObject position, JSObject? style);

        [JSImport("Visual.clear", "game/visual")]
        internal static partial void Native_Clear(JSObject proxyObject);

        [JSImport("Visual.line", "game/visual")]
        internal static partial void Native_Line(JSObject proxyObject, JSObject pos1, JSObject pos2, JSObject? style);

        [JSImport("Visual.poly", "game/visual")]
        internal static partial void Native_Poly(JSObject proxyObject, JSObject[] positions, JSObject? style);

        [JSImport("Visual.rect", "game/visual")]
        internal static partial void Native_Rect(JSObject proxyObject, JSObject position, double w, double h, JSObject? style);

        [JSImport("Visual.size", "game/visual")]
        internal static partial int Native_Size(JSObject proxyObject);

        [JSImport("Visual.text", "game/visual")]
        internal static partial void Native_Text(JSObject proxyObject, string text, JSObject position, JSObject? style);

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
