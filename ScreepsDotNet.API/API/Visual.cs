using System;

namespace ScreepsDotNet.API
{
    public readonly struct Color
    {
        private readonly int value;

        public byte R => (byte)(value >> 16 & 0xff);
        public byte G => (byte)(value >> 8 & 0xff);
        public byte B => (byte)(value >> 0 & 0xff);

        public double RNorm => R / 255.0;
        public double GNorm => G / 255.0;
        public double BNorm => B / 255.0;

        public Color(byte r, byte g, byte b)
        {
            value = r << 16 | g << 8 | b;
        }

        public override string ToString()
            => $"#{value.ToString("X").PadLeft(6, '0')}";

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

    public readonly struct CircleVisualStyle
    {
        /// <summary>
        /// Circle radius, default is 0.15
        /// </summary>
        public readonly double Radius;

        /// <summary>
        /// Fill color. Default is #ffffff
        /// </summary>
        public readonly Color Fill;

        /// <summary>
        /// Opacity value, default is 0.5
        /// </summary>
        public readonly double Opacity;

        /// <summary>
        /// Stroke color. Default is #ffffff
        /// </summary>
        public readonly Color Stroke;

        /// <summary>
        /// Stroke line width, default is 0.1
        /// </summary>
        public readonly double StrokeWidth;

        /// <summary>
        /// Either undefined (solid line), dashed, or dotted. Default is undefined
        /// </summary>
        public readonly LineStyle? LineStyle;

        public CircleVisualStyle(
            double radius = 0.15,
            Color? fill = null,
            double opacity = 0.5,
            Color? stroke = null,
            double strokeWidth = 0.1,
            LineStyle? lineStyle = null
        )
        {
            Radius = radius;
            Fill = fill ?? Color.White;
            Opacity = opacity;
            Stroke = stroke ?? Color.White;
            StrokeWidth = strokeWidth;
            LineStyle = lineStyle;
        }

        public CircleVisualStyle WithRadius(double newRadius)
            => new(newRadius, Fill, Opacity, Stroke, StrokeWidth, LineStyle);
    }

    public readonly struct LineVisualStyle
    {
        /// <summary>
        /// Line width, default is 0.1
        /// </summary>
        public readonly double Width;

        /// <summary>
        /// Line color. Default is #ffffff
        /// </summary>
        public readonly Color Color;

        /// <summary>
        /// Opacity value, default is 0.5
        /// </summary>
        public readonly double Opacity;

        /// <summary>
        /// Either undefined (solid line), dashed, or dotted. Default is undefined
        /// </summary>
        public readonly LineStyle? LineStyle;

        public LineVisualStyle(
            double width = 0.1,
            Color? color = null,
            double opacity = 0.5,
            LineStyle? lineStyle = null
        )
        {
            Width = width;
            Color = color ?? Color.White;
            Opacity = opacity;
            LineStyle = lineStyle;
        }
    }

    public readonly struct PolyVisualStyle
    {
        /// <summary>
        /// Fill color. Default is #ffffff
        /// </summary>
        public readonly Color Fill;

        /// <summary>
        /// Opacity value, default is 0.5
        /// </summary>
        public readonly double Opacity;

        /// <summary>
        /// Stroke color. Default is #ffffff
        /// </summary>
        public readonly Color Stroke;

        /// <summary>
        /// Stroke line width, default is 0.1
        /// </summary>
        public readonly double StrokeWidth;

        /// <summary>
        /// Either undefined (solid line), dashed, or dotted. Default is undefined
        /// </summary>
        public readonly LineStyle? LineStyle;

        public PolyVisualStyle(
            Color? fill = null,
            double opacity = 0.5,
            Color? stroke = null,
            double strokeWidth = 0.1,
            LineStyle? lineStyle = null
        )
        {
            Fill = fill ?? Color.White;
            Opacity = opacity;
            Stroke = stroke ?? Color.White;
            StrokeWidth = strokeWidth;
            LineStyle = lineStyle;
        }
    }

    public readonly struct RectVisualStyle
    {
        /// <summary>
        /// Fill color. Default is #ffffff
        /// </summary>
        public readonly Color Fill;

        /// <summary>
        /// Opacity value, default is 0.5
        /// </summary>
        public readonly double Opacity;

        /// <summary>
        /// Stroke color. Default is #ffffff
        /// </summary>
        public readonly Color Stroke;

        /// <summary>
        /// Stroke line width, default is 0.1
        /// </summary>
        public readonly double StrokeWidth;

        /// <summary>
        /// Either undefined (solid line), dashed, or dotted. Default is undefined
        /// </summary>
        public readonly LineStyle? LineStyle;

        public RectVisualStyle(
            Color? fill = null,
            double opacity = 0.5,
            Color? stroke = null,
            double strokeWidth = 0.1,
            LineStyle? lineStyle = null
        )
        {
            Fill = fill ?? Color.White;
            Opacity = opacity;
            Stroke = stroke ?? Color.White;
            StrokeWidth = strokeWidth;
            LineStyle = lineStyle;
        }
    }

    public readonly struct TextVisualStyle
    {
        /// <summary>
        /// Text align, either center, left, or right. Default is center
        /// </summary>
        public readonly TextAlign Align;

        /// <summary>
        /// Background color. Default is undefined (no background).
        /// When background is enabled, text vertical align is set to middle(default is baseline)
        /// </summary>
        public readonly Color? BackgroundColor;

        /// <summary>
        /// Background rectangle padding, default is 0.3
        /// </summary>
        public readonly double BackgroundPadding;

        /// <summary>
        /// Font color. Default is #ffffff
        /// </summary>
        public readonly Color Color;

        /// <summary>
        /// Either a number or a string in one of the following forms:
        /// <list type="bullet">
        /// <item>"0.7" (relative size in game coordinates)</item>
        /// <item>"20px" (absolute size in pixels)</item>
        /// <item>"0.7 serif"</item>
        /// <item>"bold italic 1.5 Times New Roman"</item>
        /// </list>
        /// </summary>
        public readonly string? Font;

        /// <summary>
        /// Opacity value, default is 1.0
        /// </summary>
        public readonly double Opacity;

        /// <summary>
        /// Stroke color. Default is undefined (no stroke)
        /// </summary>
        public readonly Color? Stroke;

        /// <summary>
        /// Stroke line width, default is 0.15
        /// </summary>
        public readonly double StrokeWidth;

        public TextVisualStyle(
            TextAlign align = TextAlign.Center,
            Color? backgroundColor = null,
            double backgroundPadding = 0.3,
            Color? color = null,
            string? font = null,
            double opacity = 1.0,
            Color? stroke = null,
            double strokeWidth = 0.15
        )
        {
            Align = align;
            BackgroundColor = backgroundColor;
            BackgroundPadding = backgroundPadding;
            Color = color ?? Color.White;
            Font = font;
            Opacity = opacity;
            Stroke = stroke;
            StrokeWidth = strokeWidth;
        }
    }

}
