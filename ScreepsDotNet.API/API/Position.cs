using System;
using System.Runtime.InteropServices.JavaScript;

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

        public override bool Equals(object? obj) => obj is Position position && Equals(position);

        public bool Equals(Position other)
            => X == other.X
            && Y == other.Y;

        public override int GetHashCode() => HashCode.Combine(X, Y);

        public static bool operator ==(Position left, Position right) => left.Equals(right);

        public static bool operator !=(Position left, Position right) => !(left == right);

        public static implicit operator Position(Tuple<int, int> tuple) => new (tuple.Item1, tuple.Item2);

        public static implicit operator Position(JSObject jsObj) => new (jsObj.GetPropertyAsInt32("x"), jsObj.GetPropertyAsInt32("y"));

        public void ToObject(JSObject jsObj)
        {
            jsObj.SetProperty("x", X);
            jsObj.SetProperty("y", Y);
        }

        public override string ToString()
            => $"[{X},{Y}]";
    }
}
