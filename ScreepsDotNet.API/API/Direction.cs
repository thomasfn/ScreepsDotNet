using System;

namespace ScreepsDotNet.API
{
    public enum Direction
    {
        Top = 1,
        TopRight = 2,
        Right = 3,
        BottomRight = 4,
        Bottom = 5,
        BottomLeft = 6,
        Left = 7,
        TopLeft = 8
    }

    public static class DirectionExtensions
    {
        public static (int dx, int dy) ToLinear(this Direction direction)
            => direction switch
            {
                Direction.Top => (0, -1),
                Direction.TopRight => (1, -1),
                Direction.Right => (1, 0),
                Direction.BottomRight => (1, 1),
                Direction.Bottom => (0, 1),
                Direction.BottomLeft => (-1, 1),
                Direction.Left => (-1, 0),
                Direction.TopLeft => (-1, -1),
                _ => (0, 0),
            };
    }
}
