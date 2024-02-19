using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ScreepsDotNet.API.World
{
    /// <summary>
    /// Represents a coordinate of a room in the world map.
    /// Positive X and Y corresponds to south and east, negative to north and west. For example,
    /// S15E5 would be (5, 15) and N5W10 would be (-11, -6)
    /// If running in the simulator, the sim room is represented as (-128, -128).
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 8)]
    public readonly struct RoomCoord : IEquatable<RoomCoord>
    {
        internal const int kMaxWorldSize = 256;
        internal const int kMaxWorldSize2 = kMaxWorldSize >> 1;

        public static readonly RoomCoord Sim = new("sim");

        public readonly int X;
        public readonly int Y;

        public bool IsSim => X == -kMaxWorldSize2 && Y == -kMaxWorldSize2;

        public RoomCoord(int x, int y)
        {
            X = x;
            Y = y;
        }

        public RoomCoord(ReadOnlySpan<char> roomName)
        {
            if (roomName.Equals("sim", StringComparison.InvariantCultureIgnoreCase))
            {
                X = -kMaxWorldSize2;
                Y = -kMaxWorldSize2;
                return;
            }
            if (roomName.Length < 4) { throw new ArgumentException($"Invalid room name '{roomName}'", nameof(roomName)); }
            int ptr = 0;
            if (roomName[ptr] == 'W')
            {
                if (roomName.Length > ptr + 2 && char.IsDigit(roomName[ptr + 2]))
                {
                    X = -((roomName[ptr + 1] - '0') * 10 + (roomName[ptr + 2] - '0') + 1);
                    ptr += 3;
                }
                else
                {
                    X = -(roomName[ptr + 1] - '0' + 1);
                    ptr += 2;
                }

            }
            else if (roomName[ptr] == 'E')
            {
                if (roomName.Length > ptr + 2 && char.IsDigit(roomName[ptr + 2]))
                {
                    X = (roomName[ptr + 1] - '0') * 10 + (roomName[ptr + 2] - '0');
                    ptr += 3;
                }
                else
                {
                    X = roomName[ptr + 1] - '0';
                    ptr += 2;
                }
            }
            else
            {
                throw new InvalidOperationException($"Room name '{roomName}' does not follow standard pattern");
            }
            if (roomName[ptr] == 'N')
            {
                if (roomName.Length > ptr + 2 && char.IsDigit(roomName[ptr + 2]))
                {
                    Y = -((roomName[ptr + 1] - '0') * 10 + (roomName[ptr + 2] - '0') + 1);
                    ptr += 3;
                }
                else
                {
                    Y = -(roomName[ptr + 1] - '0' + 1);
                    ptr += 2;
                }

            }
            else if (roomName[ptr] == 'S')
            {
                if (roomName.Length > ptr + 2 && char.IsDigit(roomName[ptr + 2]))
                {
                    Y = (roomName[ptr + 1] - '0') * 10 + (roomName[ptr + 2] - '0');
                    ptr += 3;
                }
                else
                {
                    Y = roomName[ptr + 1] - '0';
                    ptr += 2;
                }
            }
            else
            {
                throw new InvalidOperationException($"Room name '{roomName}' does not follow standard pattern");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RoomCoord? ParseNullSafe(string? roomName)
            => string.IsNullOrEmpty(roomName) ? null : new(roomName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int LinearDistanceTo(RoomCoord other)
            => Math.Max(Math.Abs(X - other.X), Math.Abs(Y - other.Y));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double CartesianDistanceTo(RoomCoord other)
            => Math.Sqrt((X - other.X) * (X - other.X) + (Y - other.Y) * (Y - other.Y));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNextTo(RoomCoord other)
            => LinearDistanceTo(other) <= 1;

        public override bool Equals(object? obj) => obj is RoomCoord position && Equals(position);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(RoomCoord other)
            => X == other.X
            && Y == other.Y;

        public override int GetHashCode() => HashCode.Combine(X, Y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(RoomCoord left, RoomCoord right) => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(RoomCoord left, RoomCoord right) => !(left == right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator RoomCoord(Tuple<int, int> tuple) => new(tuple.Item1, tuple.Item2);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator RoomCoord((int, int) tuple) => new(tuple.Item1, tuple.Item2);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Vector2(RoomCoord pos) => new(pos.X, pos.Y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RoomCoord operator +(RoomCoord lhs, (int dx, int dy) rhs)
            => new(lhs.X + rhs.dx, lhs.Y + rhs.dy);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RoomCoord operator +(RoomCoord lhs, ExitDirection rhs)
            => lhs + rhs.ToLinear();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RoomCoord operator -(RoomCoord lhs, (int dx, int dy) rhs)
            => new(lhs.X - rhs.dx, lhs.Y - rhs.dy);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RoomCoord operator -(RoomCoord lhs, ExitDirection rhs)
            => lhs - rhs.ToLinear();

        public static ExitDirection operator -(RoomCoord lhs, RoomCoord rhs)
        {
            int dx = lhs.X - rhs.X;
            int dy = lhs.Y - rhs.Y;
            var ang = Math.Atan2(dy, dx) + Math.PI * 0.5;
            if (ang < 0.0) { ang += (Math.PI * 2.0); }
            return (ExitDirection)((int)Math.Round(ang / (Math.PI * 0.25)) % 8 + 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RoomCoord FromEncodedInt(int encodedInt) => new((int)((uint)encodedInt >> 24) - kMaxWorldSize2, ((int)((uint)encodedInt >> 16) & 255) - kMaxWorldSize2);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ToEncodedInt() => (X + kMaxWorldSize2) << 24 | (Y + kMaxWorldSize2) << 16;

        public int ToString(Span<char> outRoomName)
        {
            if (IsSim)
            {
                "sim".CopyTo(outRoomName);
                return 3;
            }
            int ptr = 0;
            //int x = X - kMaxWorldSize2;
            //int y = Y - kMaxWorldSize2;
            int x = X;
            int y = Y;
            if (x < -10)
            {
                outRoomName[ptr++] = 'W';
                outRoomName[ptr++] = (char)('0' - ((x + 1) / 10));
                outRoomName[ptr++] = (char)('0' - ((x + 1) % 10));
            }
            else if (x < 0)
            {
                outRoomName[ptr++] = 'W';
                outRoomName[ptr++] = (char)('0' - ((x + 1) % 10));
            }
            else if (x > 9)
            {
                outRoomName[ptr++] = 'E';
                outRoomName[ptr++] = (char)('0' + (x / 10));
                outRoomName[ptr++] = (char)('0' + (x % 10));
            }
            else
            {
                outRoomName[ptr++] = 'E';
                outRoomName[ptr++] = (char)('0' + (x % 10));
            }
            if (y < -10)
            {
                outRoomName[ptr++] = 'N';
                outRoomName[ptr++] = (char)('0' - ((y + 1) / 10));
                outRoomName[ptr++] = (char)('0' - ((y + 1) % 10));
            }
            else if (y < 0)
            {
                outRoomName[ptr++] = 'N';
                outRoomName[ptr++] = (char)('0' - ((y + 1) % 10));
            }
            else if (y > 9)
            {
                outRoomName[ptr++] = 'S';
                outRoomName[ptr++] = (char)('0' + (y / 10));
                outRoomName[ptr++] = (char)('0' + (y % 10));
            }
            else
            {
                outRoomName[ptr++] = 'S';
                outRoomName[ptr++] = (char)('0' + (y % 10));
            }
            return ptr;
        }

        public override string ToString()
        {
            if (IsSim) { return "sim"; }
            Span<char> roomName = stackalloc char[6];
            return new string(roomName[..ToString(roomName)]);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 16)]
    public readonly struct RoomPosition : IEquatable<RoomPosition>
    {
        /// <summary>
        /// The position within the room.
        /// </summary>
        public readonly Position Position;

        /// <summary>
        /// The coordinate of the room containing the position.
        /// </summary>
        public readonly RoomCoord RoomCoord;

        /// <summary>
        /// The name of the room.
        /// </summary>
        public string RoomName => RoomCoord.ToString();

        public RoomPosition(Position position, RoomCoord roomCoord)
        {
            Position = position;
            RoomCoord = roomCoord;
        }

        public RoomPosition(Position position, ReadOnlySpan<char> roomName)
        {
            Position = position;
            RoomCoord = new(roomName);
        }

        public override bool Equals(object? obj) => obj is RoomPosition position && Equals(position);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(RoomPosition other) => Position.Equals(other.Position) && RoomCoord.Equals(other.RoomCoord);

        public override int GetHashCode() => HashCode.Combine(Position, RoomCoord);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(RoomPosition left, RoomPosition right) => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(RoomPosition left, RoomPosition right) => !(left == right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RoomPosition FromEncodedInt(int encodedInt) => new(new Position((encodedInt >> 8) & 255, encodedInt & 255), RoomCoord.FromEncodedInt(encodedInt));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ToEncodedInt() => RoomCoord.ToEncodedInt() | Position.X << 8 | Position.Y;

        public override string ToString()
            => $"[{Position.X},{Position.Y}:{RoomCoord}]";
    }
}
