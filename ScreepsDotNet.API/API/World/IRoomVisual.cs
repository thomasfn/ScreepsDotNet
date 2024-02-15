using System.Collections.Generic;

namespace ScreepsDotNet.API.World
{
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
    public interface IRoomVisual
    {
        /// <summary>
        /// The name of the room.
        /// </summary>
        string RoomName { get; }

        /// <summary>
        /// Draw a line.
        /// </summary>
        /// <param name="pos1"></param>
        /// <param name="pos2"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        IRoomVisual Line(FractionalPosition pos1, FractionalPosition pos2, LineVisualStyle? style = null);

        /// <summary>
        /// Draw a circle.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        IRoomVisual Circle(FractionalPosition position, CircleVisualStyle? style = null);

        /// <summary>
        /// Draw a rectangle.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        IRoomVisual Rect(FractionalPosition pos, double w, double h, RectVisualStyle? style = null);

        /// <summary>
        /// Draw a polyline.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        IRoomVisual Poly(IEnumerable<FractionalPosition> points, PolyVisualStyle? style = null);

        /// <summary>
        /// Draw a text label. You can use any valid Unicode characters, including emoji.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="pos"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        IRoomVisual Text(string text, FractionalPosition pos, TextVisualStyle? style = null);

        /// <summary>
        /// Remove all visuals from the object.
        /// </summary>
        /// <returns></returns>
        IRoomVisual Clear();

        /// <summary>
        /// Gets the size of the visuals in bytes.
        /// </summary>
        int GetSize();

        /// <summary>
        /// Returns a compact representation of all visuals added in the room in the current tick.
        /// </summary>
        /// <returns></returns>
        string? Export();

        /// <summary>
        /// Add previously exported (with RoomVisual.export) room visuals to the room visual data of the current tick.
        /// </summary>
        /// <param name="str"></param>
        void Import(string str);
    }
}
