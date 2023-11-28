using System;
using System.Collections.Generic;

namespace ScreepsDotNet.API.Arena
{
    /// <summary>
    /// Visuals provide a way to show various visual debug info in the game.
    /// All draw coordinates are measured in game coordinates and centered to tile centers,
    /// i.e. (10,10) will point to the center of the creep at x:10; y:10 position. Fractional coordinates are allowed.
    /// </summary>
    public interface IVisual
    {
        /// <summary>
        /// The layer of visuals in the object.
        /// </summary>
        int Layer { get; }

        /// <summary>
        /// Whether visuals in this object are persistent.
        /// </summary>
        bool Persistent { get; }

        /// <summary>
        /// Remove all visuals from the object.
        /// </summary>
        /// <returns></returns>
        IVisual Clear();

        /// <summary>
        /// Draw a circle.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        IVisual Circle(FractionalPosition position, CircleVisualStyle? style = null);

        /// <summary>
        /// Draw a line.
        /// </summary>
        /// <param name="pos1"></param>
        /// <param name="pos2"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        IVisual Line(FractionalPosition pos1, FractionalPosition pos2, LineVisualStyle? style = null);

        /// <summary>
        /// Draw a polyline.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        IVisual Poly(IEnumerable<FractionalPosition> points, PolyVisualStyle? style = null);

        /// <summary>
        /// Draw a rectangle.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        IVisual Rect(FractionalPosition pos, double w, double h, RectVisualStyle? style = null);

        /// <summary>
        /// Gets the size of the visuals in bytes.
        /// </summary>
        int Size();

        /// <summary>
        /// Draw a text label. You can use any valid Unicode characters, including emoji.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="pos"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        IVisual Text(string text, FractionalPosition pos, TextVisualStyle? style = null);
    }
}
