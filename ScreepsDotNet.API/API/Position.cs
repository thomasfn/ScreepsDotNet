using System;

namespace ScreepsDotNet.API
{
    public readonly struct Position : IEquatable<Position>
    {
        public readonly int X;
        public readonly int Y;

        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int LinearDistanceTo(Position other)
            => Math.Max(Math.Abs(X - other.X), Math.Abs(Y - other.Y));

        public int LinearDistanceTo(IPosition other)
            => LinearDistanceTo(other.Position);

        public double CartesianDistanceTo(Position other)
            => Math.Sqrt((X - other.X) * (X - other.X) + (Y - other.Y) * (Y - other.Y));

        public double CartesianDistanceTo(IPosition other)
            => CartesianDistanceTo(other.Position);

        public bool IsNextTo(Position other)
            => LinearDistanceTo(other) <= 1;

        public bool IsNextTo(IPosition other)
            => IsNextTo(other.Position);

        public override bool Equals(object? obj) => obj is Position position && Equals(position);

        public bool Equals(Position other)
            => X == other.X
            && Y == other.Y;

        public override int GetHashCode() => HashCode.Combine(X, Y);

        public static bool operator ==(Position left, Position right) => left.Equals(right);

        public static bool operator !=(Position left, Position right) => !(left == right);

        public static implicit operator Position(Tuple<int, int> tuple) => new (tuple.Item1, tuple.Item2);

        public static implicit operator Position((int, int) tuple) => new(tuple.Item1, tuple.Item2);

        public static Position operator +(Position lhs, (int dx, int dy) rhs)
            => new (lhs.X + rhs.dx, lhs.Y + rhs.dy);

        public static Position operator +(Position lhs, Direction rhs)
            => lhs + rhs.ToLinear();

        public static Position operator -(Position lhs, (int dx, int dy) rhs)
            => new(lhs.X - rhs.dx, lhs.Y - rhs.dy);

        public static Position operator -(Position lhs, Direction rhs)
            => lhs - rhs.ToLinear();

        public override string ToString()
            => $"[{X},{Y}]";
    }
}
