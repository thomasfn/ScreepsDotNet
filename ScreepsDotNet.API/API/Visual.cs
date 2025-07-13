using System;

namespace ScreepsDotNet.API
{
    public readonly struct Color(byte a, byte r, byte g, byte b)
    {
        private readonly uint value = (uint)(a << 24) | (uint)(r << 16) | (uint)(g << 8) | b;

        public byte A => (byte)((value >> 24) & 0xff);
        public byte R => (byte)((value >> 16) & 0xff);
        public byte G => (byte)((value >> 8) & 0xff);
        public byte B => (byte)((value >> 0) & 0xff);

        public double ANorm => A / 255.0;
        public double RNorm => R / 255.0;
        public double GNorm => G / 255.0;
        public double BNorm => B / 255.0;

        public Color(byte r, byte g, byte b)
            : this(255, r, g, b)
        { }

        public override string ToString()
            => a switch { 0 => "transparent", 255 => $"#{(value & 0xffffff).ToString("X").PadLeft(6, '0')}", _ => $"#{(value & 0xffffff).ToString("X").PadLeft(6, '0')}{A.ToString("X").PadLeft(2, '0')}" };

        public static Color FromNorm(double rNorm, double gNorm, double bNorm)
            => new((byte)(Math.Clamp(rNorm, 0.0, 1.0) * 255), (byte)(Math.Clamp(gNorm, 0.0, 1.0) * 255), (byte)(Math.Clamp(bNorm, 0.0, 1.0) * 255));

        #region Standard Colors

        public static readonly Color Black = new(0, 0, 0);
        public static readonly Color White = new(255, 255, 255);
        public static readonly Color Grey = new(128, 128, 128);
        public static readonly Color LightGrey = new(192, 192, 192);
        public static readonly Color DarkGray = new(64, 64, 64);

        public static readonly Color Red = new(255, 0, 0);
        public static readonly Color LightRed = new(255, 128, 128);
        public static readonly Color DarkRed = new(128, 0, 0);

        public static readonly Color Green = new(0, 255, 0);
        public static readonly Color LightGreen = new(128, 255, 128);
        public static readonly Color DarkGreen = new(0, 128, 0);

        public static readonly Color Blue = new(0, 0, 255);
        public static readonly Color LightBlue = new(128, 128, 255);
        public static readonly Color DarkBlue = new(0, 0, 128);

        public static readonly Color Transparent = new(0, 255, 255, 255);

        #endregion
    }

    public enum LineStyle
    {
        Dashed,
        Dotted
    }

    public enum TextAlign
    {
        Center,
        Left,
        Right
    }

    /// <param name="Radius">Circle radius, default is 0.15</param>
    /// <param name="Fill">Fill color, default is white</param>
    /// <param name="Opacity">Opacity value, default is 0.5</param>
    /// <param name="Stroke">Stroke color, default is white</param>
    /// <param name="StrokeWidth">Stroke line width, default is 0.1</param>
    /// <param name="LineStyle">Either null (solid line), dashed, or dotted, default is solid</param>
    public readonly record struct CircleVisualStyle
    (
        double? Radius = null,
        Color? Fill = null,
        double? Opacity = null,
        Color? Stroke = null,
        double? StrokeWidth = null,
        LineStyle? LineStyle = null
    );

    /// <param name="Width">Line width, default is 0.1</param>
    /// <param name="Color">Line color, default is white</param>
    /// <param name="Opacity">Opacity value, default is 0.5</param>
    /// <param name="LineStyle">Either null (solid line), dashed, or dotted, default is solid</param>
    public readonly record struct LineVisualStyle
    (
        double? Width = null,
        Color? Color = null,
        double? Opacity = null,
        LineStyle? LineStyle = null
    );

    /// <param name="Fill">Fill color, default is white</param>
    /// <param name="Opacity">Opacity value, default is 0.5</param>
    /// <param name="Stroke">Stroke color, default is white</param>
    /// <param name="StrokeWidth">Stroke line width, default is 0.1</param>
    /// <param name="LineStyle">Either null (solid line), dashed, or dotted, default is solid</param>
    public readonly record struct PolyVisualStyle
    (
        Color? Fill = null,
        double? Opacity = null,
        Color? Stroke = null,
        double? StrokeWidth = null,
        LineStyle? LineStyle = null
    );

    /// <param name="Fill">Fill color, default is white</param>
    /// <param name="Opacity">Opacity value, default is 0.5</param>
    /// <param name="Stroke">Stroke color, default is white</param>
    /// <param name="StrokeWidth">Stroke line width, default is 0.1</param>
    /// <param name="LineStyle">Either null (solid line), dashed, or dotted, default is solid</param>
    public readonly record struct RectVisualStyle
    (
        Color? Fill = null,
        double? Opacity = null,
        Color? Stroke = null,
        double? StrokeWidth = null,
        LineStyle? LineStyle = null
    );

    /// <param name="Align">Text align, either center, left, or right, default is center</param>
    /// <param name="BackgroundColor">Background color, default is null (no background). When background is enabled, text vertical align is set to middle (default is baseline)</param>
    /// <param name="BackgroundPadding">Background rectangle padding, default is 0.3</param>
    /// <param name="Color">Font color, default is white</param>
    /// <param name="Font">
    /// Either a number or a string in one of the following forms:
    /// <list type="bullet">
    /// <item>"0.7" (relative size in game coordinates)</item>
    /// <item>"20px" (absolute size in pixels)</item>
    /// <item>"0.7 serif"</item>
    /// <item>"bold italic 1.5 Times New Roman"</item>
    /// </list>
    /// </param>
    /// <param name="Opacity">Opacity value, default is 1.0</param>
    /// <param name="Stroke">Stroke color, default is null (no stroke)</param>
    /// <param name="StrokeWidth">Stroke line width, default is 0.15</param>
    public readonly record struct TextVisualStyle
    (
        TextAlign? Align = null,
        Color? BackgroundColor = null,
        double? BackgroundPadding = null,
        Color? Color = null,
        string? Font = null,
        double? Opacity = null,
        Color? Stroke = null,
        double? StrokeWidth = null
    );
}
