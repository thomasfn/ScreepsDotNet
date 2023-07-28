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

        public static Direction Opposite(this Direction direction)
            => direction switch
            {
                Direction.Top => Direction.Bottom,
                Direction.TopRight => Direction.BottomLeft,
                Direction.Right => Direction.Left,
                Direction.BottomRight => Direction.TopLeft,
                Direction.Bottom => Direction.Top,
                Direction.BottomLeft => Direction.TopRight,
                Direction.Left => Direction.Right,
                Direction.TopLeft => Direction.BottomRight,
                _ => throw new ArgumentException("Invalid direction", nameof(direction)),
            };

        public static bool IsDiagonal(this Direction direction)
            => direction == Direction.TopRight || direction == Direction.BottomRight || direction == Direction.BottomLeft || direction == Direction.TopLeft;

        public static bool IsStraight(this Direction direction)
            => direction == Direction.Top || direction == Direction.Right || direction == Direction.Bottom || direction == Direction.Left;
    }
}
