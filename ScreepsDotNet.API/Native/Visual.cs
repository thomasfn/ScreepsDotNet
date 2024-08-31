using System;
using System.Collections.Generic;

using ScreepsDotNet.Interop;
using ScreepsDotNet.API;

namespace ScreepsDotNet.Native
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal static class VisualExtensions
    {
        private static class Names
        {
            public static readonly Name Dotted = Name.Create("dotted");
            public static readonly Name Dashed = Name.Create("dashed");
            public static readonly Name Center = Name.Create("center");
            public static readonly Name Left = Name.Create("left");
            public static readonly Name Right = Name.Create("right");
            public static readonly Name Radius = Name.Create("radius");
            public static readonly Name Fill = Name.Create("fill");
            public static readonly Name Opacity = Name.Create("opacity");
            public static readonly Name Stroke = Name.Create("stroke");
            public static readonly Name StrokeWidth = Name.Create("strokeWidth");
            public static readonly Name LineStyle = Name.Create("lineStyle");
            public static readonly Name Width = Name.Create("width");
            public static readonly Name Color = Name.Create("color");
            public static readonly Name Align = Name.Create("align");
            public static readonly Name BackgroundColor = Name.Create("backgroundColor");
            public static readonly Name BackgroundPadding = Name.Create("backgroundPadding");
            public static readonly Name Font = Name.Create("font");
        }

        private static readonly Dictionary<CircleVisualStyle, JSObject> circleVisualStyleCache = [];
        private static readonly Dictionary<LineVisualStyle, JSObject> lineVisualStyleCache = [];
        private static readonly Dictionary<PolyVisualStyle, JSObject> polyVisualStyleCache = [];
        private static readonly Dictionary<RectVisualStyle, JSObject> rectVisualStyleCache = [];
        private static readonly Dictionary<TextVisualStyle, JSObject> textVisualStyleCache = [];

        public static Name ToJS(this LineStyle lineStyle)
            => lineStyle switch
            {
                LineStyle.Dotted => Names.Dotted,
                LineStyle.Dashed => Names.Dashed,
                _ => throw new ArgumentException($"Unknown line style", nameof(lineStyle)),
            };

        public static Name ToJS(this TextAlign textAlign)
            => textAlign switch
            {
                TextAlign.Center => Names.Center,
                TextAlign.Left => Names.Left,
                TextAlign.Right => Names.Right,
                _ => throw new ArgumentException($"Unknown text align", nameof(textAlign)),
            };

        public static JSObject ToJSRaw(this CircleVisualStyle circleVisualStyle)
        {
            var obj = JSObject.Create();
            if (circleVisualStyle.Radius != null) { obj.SetProperty(Names.Radius, circleVisualStyle.Radius.Value); }
            if (circleVisualStyle.Fill != null) { obj.SetProperty(Names.Fill, circleVisualStyle.Fill.Value.ToString()); }
            if (circleVisualStyle.Opacity != null) { obj.SetProperty(Names.Opacity, circleVisualStyle.Opacity.Value); }
            if (circleVisualStyle.Stroke != null) { obj.SetProperty(Names.Stroke, circleVisualStyle.Stroke.Value.ToString()); }
            if (circleVisualStyle.StrokeWidth != null) { obj.SetProperty(Names.StrokeWidth, circleVisualStyle.StrokeWidth.Value); }
            if (circleVisualStyle.LineStyle != null) { obj.SetProperty(Names.LineStyle, circleVisualStyle.LineStyle.Value.ToJS()); }
            return obj;
        }

        public static JSObject ToJS(this CircleVisualStyle circleVisualStyle)
        {
            if (!circleVisualStyleCache.TryGetValue(circleVisualStyle, out var obj))
            {
                obj = circleVisualStyle.ToJSRaw();
                circleVisualStyleCache.Add(circleVisualStyle, obj);
            }
            return obj;
        }

        public static JSObject ToJSRaw(this LineVisualStyle lineVisualStyle)
        {
            var obj = JSObject.Create();
            if (lineVisualStyle.Width != null) { obj.SetProperty(Names.Width, lineVisualStyle.Width.Value); }
            if (lineVisualStyle.Color != null) { obj.SetProperty(Names.Color, lineVisualStyle.Color.Value.ToString()); }
            if (lineVisualStyle.Opacity != null) { obj.SetProperty(Names.Opacity, lineVisualStyle.Opacity.Value); }
            if (lineVisualStyle.LineStyle != null) { obj.SetProperty(Names.LineStyle, lineVisualStyle.LineStyle.Value.ToJS()); }
            return obj;
        }

        public static JSObject ToJS(this LineVisualStyle lineVisualStyle)
        {
            if (!lineVisualStyleCache.TryGetValue(lineVisualStyle, out var obj))
            {
                obj = lineVisualStyle.ToJSRaw();
                lineVisualStyleCache.Add(lineVisualStyle, obj);
            }
            return obj;
        }

        public static JSObject ToJSRaw(this PolyVisualStyle polyVisualStyle)
        {
            var obj = JSObject.Create();
            if (polyVisualStyle.Fill != null) { obj.SetProperty(Names.Fill, polyVisualStyle.Fill.Value.ToString()); }
            if (polyVisualStyle.Opacity != null) { obj.SetProperty(Names.Opacity, polyVisualStyle.Opacity.Value); }
            if (polyVisualStyle.Stroke != null) { obj.SetProperty(Names.Stroke, polyVisualStyle.Stroke.Value.ToString()); }
            if (polyVisualStyle.StrokeWidth != null) { obj.SetProperty(Names.StrokeWidth, polyVisualStyle.StrokeWidth.Value); }
            if (polyVisualStyle.LineStyle != null) { obj.SetProperty(Names.LineStyle, polyVisualStyle.LineStyle.Value.ToJS()); }
            return obj;
        }

        public static JSObject ToJS(this PolyVisualStyle polyVisualStyle)
        {
            if (!polyVisualStyleCache.TryGetValue(polyVisualStyle, out var obj))
            {
                obj = polyVisualStyle.ToJSRaw();
                polyVisualStyleCache.Add(polyVisualStyle, obj);
            }
            return obj;
        }

        public static JSObject ToJSRaw(this RectVisualStyle rectVisualStyle)
        {
            var obj = JSObject.Create();
            if (rectVisualStyle.Fill != null) { obj.SetProperty(Names.Fill, rectVisualStyle.Fill.Value.ToString()); }
            if (rectVisualStyle.Opacity != null) { obj.SetProperty(Names.Opacity, rectVisualStyle.Opacity.Value.ToString()); }
            if (rectVisualStyle.Stroke != null) { obj.SetProperty(Names.Stroke, rectVisualStyle.Stroke.Value.ToString()); }
            if (rectVisualStyle.StrokeWidth != null) { obj.SetProperty(Names.StrokeWidth, rectVisualStyle.StrokeWidth.Value); }
            if (rectVisualStyle.LineStyle != null) { obj.SetProperty(Names.LineStyle, rectVisualStyle.LineStyle.Value.ToJS()); }
            return obj;
        }

        public static JSObject ToJS(this RectVisualStyle rectVisualStyle)
        {
            if (!rectVisualStyleCache.TryGetValue(rectVisualStyle, out var obj))
            {
                obj = rectVisualStyle.ToJSRaw();
                rectVisualStyleCache.Add(rectVisualStyle, obj);
            }
            return obj;
        }

        public static JSObject ToJSRaw(this TextVisualStyle textVisualStyle)
        {
            var obj = JSObject.Create();
            if (textVisualStyle.Align != null) { obj.SetProperty(Names.Align, textVisualStyle.Align.Value.ToJS()); }
            if (textVisualStyle.BackgroundColor != null) { obj.SetProperty(Names.BackgroundColor, textVisualStyle.BackgroundColor.Value.ToString()); }
            if (textVisualStyle.BackgroundPadding != null) { obj.SetProperty(Names.BackgroundPadding, textVisualStyle.BackgroundPadding.Value); }
            if (textVisualStyle.Color != null) { obj.SetProperty(Names.Color, textVisualStyle.Stroke.ToString()); }
            if (textVisualStyle.Font != null) { obj.SetProperty(Names.Font, textVisualStyle.Font); }
            if (textVisualStyle.Opacity != null) { obj.SetProperty(Names.Opacity, textVisualStyle.Opacity.Value); }
            if (textVisualStyle.Stroke != null) { obj.SetProperty(Names.Stroke, textVisualStyle.Stroke.Value.ToString()); }
            if (textVisualStyle.StrokeWidth != null) { obj.SetProperty(Names.StrokeWidth, textVisualStyle.StrokeWidth.Value); }
            return obj;
        }

        public static JSObject ToJS(this TextVisualStyle textVisualStyle)
        {
            if (!textVisualStyleCache.TryGetValue(textVisualStyle, out var obj))
            {
                obj = textVisualStyle.ToJSRaw();
                textVisualStyleCache.Add(textVisualStyle, obj);
            }
            return obj;
        }
    }
}
