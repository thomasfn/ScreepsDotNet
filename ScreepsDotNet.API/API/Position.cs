using System;
using System.Numerics;

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

        public double CartesianDistanceTo(Position other)
            => Math.Sqrt((X - other.X) * (X - other.X) + (Y - other.Y) * (Y - other.Y));

        public bool IsNextTo(Position other)
            => LinearDistanceTo(other) <= 1;

        public override bool Equals(object? obj) => obj is Position position && Equals(position);

        public bool Equals(Position other)
            => X == other.X
            && Y == other.Y;

        public override int GetHashCode() => HashCode.Combine(X, Y);

        public static bool operator ==(Position left, Position right) => left.Equals(right);

        public static bool operator !=(Position left, Position right) => !(left == right);

        public static implicit operator Position(Tuple<int, int> tuple) => new (tuple.Item1, tuple.Item2);

        public static implicit operator Position((int, int) tuple) => new(tuple.Item1, tuple.Item2);

        public static implicit operator Vector2(Position pos) => new(pos.X, pos.Y);

        public static Position operator +(Position lhs, (int dx, int dy) rhs)
            => new (lhs.X + rhs.dx, lhs.Y + rhs.dy);

        public static Position operator +(Position lhs, Direction rhs)
            => lhs + rhs.ToLinear();

        public static FractionalPosition operator +(Position lhs, (double dx, double dy) rhs)
            => new(lhs.X + rhs.dx, lhs.Y + rhs.dy);

        public static Position operator -(Position lhs, (int dx, int dy) rhs)
            => new(lhs.X - rhs.dx, lhs.Y - rhs.dy);

        public static Position operator -(Position lhs, Direction rhs)
            => lhs - rhs.ToLinear();

        public static FractionalPosition operator -(Position lhs, (double dx, double dy) rhs)
           => new(lhs.X - rhs.dx, lhs.Y - rhs.dy);

        public static Direction operator -(Position lhs, Position rhs)
        {
            int dx = lhs.X - rhs.X;
            int dy = lhs.Y - rhs.Y;
            var ang = Math.Atan2(dy, dx) + Math.PI * 0.5;
            if (ang < 0.0) { ang += (Math.PI * 2.0); }
            return (Direction)((int)Math.Round(ang / (Math.PI * 0.25)) % 8 + 1);
        }

        public override string ToString()
            => $"[{X},{Y}]";
    }

    public readonly struct FractionalPosition : IEquatable<FractionalPosition>
    {
        public readonly double X;
        public readonly double Y;

        public FractionalPosition(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double CartesianDistanceTo(FractionalPosition other)
            => Math.Sqrt((X - other.X) * (X - other.X) + (Y - other.Y) * (Y - other.Y));

        public override bool Equals(object? obj) => obj is FractionalPosition position && Equals(position);

        public bool Equals(FractionalPosition other)
            => X == other.X
            && Y == other.Y;

        public override int GetHashCode() => HashCode.Combine(X, Y);

        public static bool operator ==(FractionalPosition left, FractionalPosition right) => left.Equals(right);

        public static bool operator !=(FractionalPosition left, FractionalPosition right) => !(left == right);

        public static implicit operator FractionalPosition(Tuple<double, double> tuple) => new(tuple.Item1, tuple.Item2);

        public static implicit operator FractionalPosition((double, double) tuple) => new(tuple.Item1, tuple.Item2);

        public static implicit operator FractionalPosition(Position pos) => new(pos.X, pos.Y);

        public static implicit operator Vector2(FractionalPosition pos) => new((float)pos.X, (float)pos.Y);

        public static FractionalPosition operator +(FractionalPosition lhs, (double dx, double dy) rhs)
            => new(lhs.X + rhs.dx, lhs.Y + rhs.dy);

        public static FractionalPosition operator +(FractionalPosition lhs, Vector2 rhs)
            => new(lhs.X + rhs.X, lhs.Y + rhs.Y);

        public static FractionalPosition operator -(FractionalPosition lhs, (double dx, double dy) rhs)
            => new(lhs.X - rhs.dx, lhs.Y - rhs.dy);

        public static FractionalPosition operator -(FractionalPosition lhs, Vector2 rhs)
            => new(lhs.X - rhs.X, lhs.Y - rhs.Y);

        public override string ToString()
            => $"[{X:N},{Y:N}]";
    }
}
