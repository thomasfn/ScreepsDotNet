using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ScreepsDotNet.API
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 8)]
    public readonly struct Position : IEquatable<Position>
    {
        public readonly int X;
        public readonly int Y;

        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int LinearDistanceTo(Position other)
            => Math.Max(Math.Abs(X - other.X), Math.Abs(Y - other.Y));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double CartesianDistanceTo(Position other)
            => Math.Sqrt((X - other.X) * (X - other.X) + (Y - other.Y) * (Y - other.Y));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNextTo(Position other)
            => LinearDistanceTo(other) <= 1;

        public override bool Equals(object? obj) => obj is Position position && Equals(position);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Position other)
            => X == other.X
            && Y == other.Y;

        public override int GetHashCode() => HashCode.Combine(X, Y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Position left, Position right) => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Position left, Position right) => !(left == right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Position(Tuple<int, int> tuple) => new (tuple.Item1, tuple.Item2);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Position((int, int) tuple) => new(tuple.Item1, tuple.Item2);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Vector2(Position pos) => new(pos.X, pos.Y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Position operator +(Position lhs, (int dx, int dy) rhs)
            => new (lhs.X + rhs.dx, lhs.Y + rhs.dy);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Position operator +(Position lhs, Direction rhs)
            => lhs + rhs.ToLinear();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FractionalPosition operator +(Position lhs, (double dx, double dy) rhs)
            => new(lhs.X + rhs.dx, lhs.Y + rhs.dy);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Position operator -(Position lhs, (int dx, int dy) rhs)
            => new(lhs.X - rhs.dx, lhs.Y - rhs.dy);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Position operator -(Position lhs, Direction rhs)
            => lhs - rhs.ToLinear();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        public FractionalPosition(int x, int y)
        {
            X = Convert.ToDouble(x);
            Y = Convert.ToDouble(y);
        }

        public FractionalPosition( Position position )
        {
            X = Convert.ToDouble(position.X);
            Y = Convert.ToDouble(position.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double CartesianDistanceTo(FractionalPosition other)
            => Math.Sqrt((X - other.X) * (X - other.X) + (Y - other.Y) * (Y - other.Y));

        public override bool Equals(object? obj) => obj is FractionalPosition position && Equals(position);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(FractionalPosition other)
            => X == other.X
            && Y == other.Y;

        public override int GetHashCode() => HashCode.Combine(X, Y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(FractionalPosition left, FractionalPosition right) => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(FractionalPosition left, FractionalPosition right) => !(left == right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator FractionalPosition(Tuple<double, double> tuple) => new(tuple.Item1, tuple.Item2);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator FractionalPosition((double, double) tuple) => new(tuple.Item1, tuple.Item2);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator FractionalPosition(Position pos) => new(pos.X, pos.Y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Vector2(FractionalPosition pos) => new((float)pos.X, (float)pos.Y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FractionalPosition operator +(FractionalPosition lhs, (double dx, double dy) rhs)
            => new(lhs.X + rhs.dx, lhs.Y + rhs.dy);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FractionalPosition operator +(FractionalPosition lhs, Vector2 rhs)
            => new(lhs.X + rhs.X, lhs.Y + rhs.Y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FractionalPosition operator -(FractionalPosition lhs, (double dx, double dy) rhs)
            => new(lhs.X - rhs.dx, lhs.Y - rhs.dy);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FractionalPosition operator -(FractionalPosition lhs, Vector2 rhs)
            => new(lhs.X - rhs.X, lhs.Y - rhs.Y);

        public override string ToString()
            => $"[{X:N},{Y:N}]";
    }
}
