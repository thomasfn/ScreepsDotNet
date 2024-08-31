using System.Collections.Generic;

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

    /// <param name="Color">Font color, default is white</param>
    /// <param name="FontFamily">Font family, default is sans-serif</param>
    /// <param name="FontSize">Font size in game coordinates, default is 10</param>
    /// <param name="FontStyle">Font style, default is normal</param>
    /// <param name="FontVariant">Font variant, default is normal</param>
    /// <param name="Stroke">Stroke color, default is null (no stroke)</param>
    /// <param name="StrokeWidth">Stroke width, default is 0.15</param>
    /// <param name="BackgroundColor">Background color, default is null (no background). When background is enabled, text vertical align is set to middle (default is baseline)</param>
    /// <param name="BackgroundPadding">Background rectangle padding, default is 2</param>
    /// <param name="Align">Text horizontal align, default is center</param>
    /// <param name="Opacity">Opacity value, default is 0.5</param>
    public readonly record struct MapTextVisualStyle
    (
        Color? Color = null,
        string? FontFamily = null,
        double? FontSize = null,
        MapTextFontStyle? FontStyle = null,
        MapTextFontVariant? FontVariant = null,
        Color? Stroke = null,
        double? StrokeWidth = null,
        Color? BackgroundColor = null,
        double? BackgroundPadding = null,
        TextAlign? Align = null,
        double? Opacity = null
    );

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
