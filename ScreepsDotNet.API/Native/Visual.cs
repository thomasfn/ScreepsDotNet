using System;
using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API;

namespace ScreepsDotNet.Native
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal static class VisualExtensions
    {
        public static string ToJS(this LineStyle lineStyle)
            => lineStyle switch
            {
                LineStyle.Dotted => "dotted",
                LineStyle.Dashed => "dashed",
                _ => throw new ArgumentException($"Unknown line style", nameof(lineStyle)),
            };

        public static string ToJS(this TextAlign textAlign)
            => textAlign switch
            {
                TextAlign.Center => "center",
                TextAlign.Left => "left",
                TextAlign.Right => "right",
                _ => throw new ArgumentException($"Unknown text align", nameof(textAlign)),
            };

        public static JSObject ToJS(this CircleVisualStyle circleVisualStyle)
        {
            var obj = JSUtils.CreateObject(null);
            obj.SetProperty("radius", circleVisualStyle.Radius);
            obj.SetProperty("fill", circleVisualStyle.Fill.ToString());
            obj.SetProperty("opacity", circleVisualStyle.Opacity);
            obj.SetProperty("stroke", circleVisualStyle.Stroke.ToString());
            obj.SetProperty("strokeWidth", circleVisualStyle.StrokeWidth);
            if (circleVisualStyle.LineStyle.HasValue) { obj.SetProperty("lineStyle", circleVisualStyle.LineStyle.Value.ToJS()); }
            return obj;
        }

        public static JSObject ToJS(this LineVisualStyle lineVisualStyle)
        {
            var obj = JSUtils.CreateObject(null);
            obj.SetProperty("width", lineVisualStyle.Width);
            obj.SetProperty("color", lineVisualStyle.Color.ToString());
            obj.SetProperty("opacity", lineVisualStyle.Opacity);
            if (lineVisualStyle.LineStyle.HasValue) { obj.SetProperty("lineStyle", lineVisualStyle.LineStyle.Value.ToJS()); }
            return obj;
        }

        public static JSObject ToJS(this PolyVisualStyle polyVisualStyle)
        {
            var obj = JSUtils.CreateObject(null);
            obj.SetProperty("fill", polyVisualStyle.Fill.ToString());
            obj.SetProperty("opacity", polyVisualStyle.Opacity);
            obj.SetProperty("stroke", polyVisualStyle.Stroke.ToString());
            obj.SetProperty("strokeWidth", polyVisualStyle.StrokeWidth);
            if (polyVisualStyle.LineStyle.HasValue) { obj.SetProperty("lineStyle", polyVisualStyle.LineStyle.Value.ToJS()); }
            return obj;
        }

        public static JSObject ToJS(this RectVisualStyle rectVisualStyle)
        {
            var obj = JSUtils.CreateObject(null);
            obj.SetProperty("fill", rectVisualStyle.Fill.ToString());
            obj.SetProperty("opacity", rectVisualStyle.Opacity);
            obj.SetProperty("stroke", rectVisualStyle.Stroke.ToString());
            obj.SetProperty("strokeWidth", rectVisualStyle.StrokeWidth);
            if (rectVisualStyle.LineStyle.HasValue) { obj.SetProperty("lineStyle", rectVisualStyle.LineStyle.Value.ToJS()); }
            return obj;
        }

        public static JSObject ToJS(this TextVisualStyle textVisualStyle)
        {
            var obj = JSUtils.CreateObject(null);
            obj.SetProperty("align", textVisualStyle.Align.ToJS());
            if (textVisualStyle.BackgroundColor.HasValue) { obj.SetProperty("backgroundColor", textVisualStyle.BackgroundColor.Value.ToString()); }
            obj.SetProperty("backgroundPadding", textVisualStyle.BackgroundPadding);
            obj.SetProperty("color", textVisualStyle.Stroke.ToString());
            if (textVisualStyle.Font != null) { obj.SetProperty("font", textVisualStyle.Font); }
            obj.SetProperty("opacity", textVisualStyle.Opacity);
            if (textVisualStyle.Stroke.HasValue) { obj.SetProperty("stroke", textVisualStyle.Stroke.Value.ToString()); }
            obj.SetProperty("strokeWidth", textVisualStyle.StrokeWidth);
            return obj;
        }
    }
}
