﻿using System.Collections.Generic;

namespace ScreepsDotNet.API.World
{
    public enum MapTextFontStyle
    {
        Normal,
        Italic,
        Oblique
    }

    public enum MapTextFontVariant
    {
        Normal,
        SmallCaps
    }

    public readonly struct MapTextVisualStyle
    {
        /// <summary>
        /// Font color. Default is white.
        /// </summary>
        public readonly Color? Color;
        /// <summary>
        /// The font family, default is sans-serif
        /// </summary>
        public readonly string? FontFamily;
        /// <summary>
        /// The font size in game coordinates, default is 10
        /// </summary>
        public readonly double? FontSize;
        /// <summary>
        /// The font style ('normal', 'italic' or 'oblique')
        /// </summary>
        public readonly MapTextFontStyle? FontStyle;
        /// <summary>
        /// The font variant ('normal' or 'small-caps')
        /// </summary>
        public readonly MapTextFontVariant? FontVariant;
        /// <summary>
        /// Stroke color. Default is undefined (no stroke).
        /// </summary>
        public readonly Color? Stroke;
        /// <summary>
        /// Stroke width, default is 0.15.
        /// </summary>
        public readonly double? StrokeWidth;
        /// <summary>
        /// Background color. Default is undefined (no background). When background is enabled, text vertical align is set to middle (default is baseline).
        /// </summary>
        public readonly Color? BackgroundColor;
        /// <summary>
        /// Background rectangle padding, default is 2.
        /// </summary>
        public readonly double? BackgroundPadding;
        /// <summary>
        /// Text align, either center, left, or right. Default is center.
        /// </summary>
        public readonly TextAlign? Align;
        /// <summary>
        /// Opacity value, default is 0.5.
        /// </summary>
        public readonly double? Opacity;

        public MapTextVisualStyle(Color? color = null, string? fontFamily = null, double? fontSize = null, MapTextFontStyle? fontStyle = null, MapTextFontVariant? fontVariant = null, Color? stroke = null, double? strokeWidth = null, Color? backgroundColor = null, double? backgroundPadding = null, TextAlign? align = null, double? opacity = null)
        {
            Color = color;
            FontFamily = fontFamily;
            FontSize = fontSize;
            FontStyle = fontStyle;
            FontVariant = fontVariant;
            Stroke = stroke;
            StrokeWidth = strokeWidth;
            BackgroundColor = backgroundColor;
            BackgroundPadding = backgroundPadding;
            Align = align;
            Opacity = opacity;
        }
    }

    /// <summary>
    /// Room visuals provide a way to show various visual debug info in game rooms.
    /// You can use the RoomVisual object to draw simple shapes that are visible only to you.
    /// Every existing Room object already contains the visual property, but you also can create new RoomVisual objects for any room (even without visibility) using the constructor.
    /// Room visuals are not stored in the database, their only purpose is to display something in your browser.All drawings will persist for one tick and will disappear if not updated.
    /// All RoomVisual API calls have no added CPU cost (their cost is natural and mostly related to simple JSON.serialize calls).
    /// However, there is a usage limit: you cannot post more than 500 KB of serialized data per one room(see getSize method).
    /// All draw coordinates are measured in game coordinates and centered to tile centers, i.e. (10,10) will point to the center of the creep at x:10; y:10 position.
    /// Fractional coordinates are allowed.
    /// </summary>
    public interface IMapVisual
    {
        /// <summary>
        /// Draw a line.
        /// </summary>
        /// <param name="pos1"></param>
        /// <param name="pos2"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        IMapVisual Line(RoomPosition pos1, RoomPosition pos2, LineVisualStyle? style = null);

        /// <summary>
        /// Draw a circle.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        IMapVisual Circle(RoomPosition position, CircleVisualStyle? style = null);

        /// <summary>
        /// Draw a rectangle.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        IMapVisual Rect(RoomPosition pos, double w, double h, RectVisualStyle? style = null);

        /// <summary>
        /// Draw a polyline.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        IMapVisual Poly(IEnumerable<RoomPosition> points, PolyVisualStyle? style = null);

        /// <summary>
        /// Draw a text label. You can use any valid Unicode characters, including emoji.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="pos"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        IMapVisual Text(string text, RoomPosition pos, MapTextVisualStyle? style = null);

        /// <summary>
        /// Remove all visuals from the object.
        /// </summary>
        /// <returns></returns>
        IMapVisual Clear();

        /// <summary>
        /// Gets the size of the visuals in bytes.
        /// </summary>
        int GetSize();

        /// <summary>
        /// Returns a compact representation of all visuals added in the room in the current tick.
        /// </summary>
        /// <returns></returns>
        string Export();

        /// <summary>
        /// Add previously exported (with MapVisual.export) room visuals to the room visual data of the current tick.
        /// </summary>
        /// <param name="str"></param>
        void Import(string str);
    }
}
