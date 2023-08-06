using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeMapVisual : IMapVisual
    {
        #region Imports

        [JSImport("visual.line", "game")]
        internal static partial void Native_Line([JSMarshalAs<JSType.Object>] JSObject pos1, [JSMarshalAs<JSType.Object>] JSObject pos2, [JSMarshalAs<JSType.Object>] JSObject? style);

        [JSImport("visual.circle", "game")]
        internal static partial void Native_Circle([JSMarshalAs<JSType.Object>] JSObject position, [JSMarshalAs<JSType.Object>] JSObject? style);

        [JSImport("visual.rect", "game")]
        internal static partial void Native_Rect([JSMarshalAs<JSType.Object>] JSObject position, [JSMarshalAs<JSType.Number>] double w, [JSMarshalAs<JSType.Number>] double h, [JSMarshalAs<JSType.Object>] JSObject? style);

        [JSImport("visual.poly", "game")]
        internal static partial void Native_Poly([JSMarshalAs<JSType.Array<JSType.Object>>] JSObject[] positions, [JSMarshalAs<JSType.Object>] JSObject? style);

        [JSImport("visual.text", "game")]
        internal static partial void Native_Text([JSMarshalAs<JSType.String>] string text, [JSMarshalAs<JSType.Object>] JSObject position, [JSMarshalAs<JSType.Object>] JSObject? style);

        [JSImport("visual.clear", "game")]
        internal static partial void Native_Clear();

        [JSImport("visual.getSize", "game")]
        [return: JSMarshalAs<JSType.Number>]
        internal static partial int Native_GetSize();

        [JSImport("visual.export", "game")]
        [return: JSMarshalAs<JSType.String>]
        internal static partial string Native_Export();

        [JSImport("visual.import", "game")]
        internal static partial void Native_Import([JSMarshalAs<JSType.String>] string value);

        #endregion

        public IMapVisual Line(RoomPosition pos1, RoomPosition pos2, LineVisualStyle? style = null)
        {
            using var pos1Js = pos1.ToJS();
            using var pos2Js = pos2.ToJS();
            using var styleJs = style?.ToJS();
            Native_Line(pos1Js, pos2Js, styleJs);
            return this;
        }

        public IMapVisual Circle(RoomPosition position, CircleVisualStyle? style = null)
        {
            using var positionJs = position.ToJS();
            using var styleJs = style?.ToJS();
            Native_Circle(positionJs, styleJs);
            return this;
        }

        public IMapVisual Rect(RoomPosition pos, double w, double h, RectVisualStyle? style = null)
        {
            using var posJs = pos.ToJS();
            using var styleJs = style?.ToJS();
            Native_Rect(posJs, w, h, styleJs);
            return this;
        }

        public IMapVisual Poly(IEnumerable<RoomPosition> points, PolyVisualStyle? style = null)
        {
            using var styleJs = style?.ToJS();
            var pointsJs = points.Select(x => x.ToJS()).ToArray();
            try
            {
                Native_Poly(pointsJs, styleJs);
            }
            finally
            {
                pointsJs.DisposeAll();
            }
            return this;
        }

        public IMapVisual Text(string text, RoomPosition pos, TextVisualStyle? style = null)
        {
            using var posJs = pos.ToJS();
            using var styleJs = style?.ToJS();
            Native_Text(text, posJs, styleJs);
            return this;
        }

        public IMapVisual Clear()
        {
            Native_Clear();
            return this;
        }

        public int GetSize()
            => Native_GetSize();

        public string Export()
            => Native_Export();

        public void Import(string str)
            => Native_Import(str);
    }
}
