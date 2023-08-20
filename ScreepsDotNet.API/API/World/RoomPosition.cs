using System;
using System.Numerics;

namespace ScreepsDotNet.API.World
{
    /// <summary>
    /// Represents a coordinate of a room in the world map.
    /// Positive X and Y corresponds to north and east, negative to south and west. For example,
    /// N15E5 would be (5, 15) and S5W10 would be (-11, -6)
    /// If running in the simulator, the sim room is represented as (int.MaxValue, int.MaxValue).
    /// </summary>
    public readonly struct RoomCoord : IEquatable<RoomCoord>
    {
        public static readonly RoomCoord Sim = new("sim");

        public readonly int X;
        public readonly int Y;

        public bool IsSim => X == int.MaxValue && Y == int.MaxValue;

        public RoomCoord(int x, int y)
        {
            X = x;
            Y = y;
        }

        public RoomCoord(ReadOnlySpan<char> roomName)
        {
            if (roomName.Equals("sim", StringComparison.InvariantCultureIgnoreCase))
            {
                X = int.MaxValue;
                Y = int.MaxValue;
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
            if (roomName[ptr] == 'S')
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
            else if (roomName[ptr] == 'N')
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

        public int LinearDistanceTo(RoomCoord other)
            => Math.Max(Math.Abs(X - other.X), Math.Abs(Y - other.Y));

        public double CartesianDistanceTo(RoomCoord other)
            => Math.Sqrt((X - other.X) * (X - other.X) + (Y - other.Y) * (Y - other.Y));

        public bool IsNextTo(RoomCoord other)
            => LinearDistanceTo(other) <= 1;

        public override bool Equals(object? obj) => obj is RoomCoord position && Equals(position);

        public bool Equals(RoomCoord other)
            => X == other.X
            && Y == other.Y;

        public override int GetHashCode() => HashCode.Combine(X, Y);

        public static bool operator ==(RoomCoord left, RoomCoord right) => left.Equals(right);

        public static bool operator !=(RoomCoord left, RoomCoord right) => !(left == right);

        public static implicit operator RoomCoord(Tuple<int, int> tuple) => new(tuple.Item1, tuple.Item2);

        public static implicit operator RoomCoord((int, int) tuple) => new(tuple.Item1, tuple.Item2);

        public static implicit operator Vector2(RoomCoord pos) => new(pos.X, pos.Y);

        public static RoomCoord operator +(RoomCoord lhs, (int dx, int dy) rhs)
            => new(lhs.X + rhs.dx, lhs.Y + rhs.dy);

        public static RoomCoord operator +(RoomCoord lhs, ExitDirection rhs)
            => lhs + rhs.ToLinear();

        public static RoomCoord operator -(RoomCoord lhs, (int dx, int dy) rhs)
            => new(lhs.X - rhs.dx, lhs.Y - rhs.dy);

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

        public int ToString(Span<char> outRoomName)
        {
            if (IsSim)
            {
                "sim".CopyTo(outRoomName);
                return 3;
            }
            int ptr = 0;
            if (X < -10)
            {
                outRoomName[ptr++] = 'W';
                outRoomName[ptr++] = (char)('0' - ((X + 1) / 10));
                outRoomName[ptr++] = (char)('0' - ((X + 1) % 10));
            }
            else if (X < 0)
            {
                outRoomName[ptr++] = 'W';
                outRoomName[ptr++] = (char)('0' - ((X + 1) % 10));
            }
            else if (X > 9)
            {
                outRoomName[ptr++] = 'E';
                outRoomName[ptr++] = (char)('0' + (X / 10));
                outRoomName[ptr++] = (char)('0' + (X % 10));
            }
            else
            {
                outRoomName[ptr++] = 'E';
                outRoomName[ptr++] = (char)('0' + (X % 10));
            }
            if (Y < -10)
            {
                outRoomName[ptr++] = 'S';
                outRoomName[ptr++] = (char)('0' - ((Y + 1) / 10));
                outRoomName[ptr++] = (char)('0' - ((Y + 1) % 10));
            }
            else if (Y < 0)
            {
                outRoomName[ptr++] = 'S';
                outRoomName[ptr++] = (char)('0' - ((Y + 1) % 10));
            }
            else if (Y > 9)
            {
                outRoomName[ptr++] = 'N';
                outRoomName[ptr++] = (char)('0' + (Y / 10));
                outRoomName[ptr++] = (char)('0' + (Y % 10));
            }
            else
            {
                outRoomName[ptr++] = 'N';
                outRoomName[ptr++] = (char)('0' + (Y % 10));
            }
            return ptr;
        }

        public override string ToString()
        {
            Span<char> roomName = stackalloc char[6];
            return new string(roomName[..ToString(roomName)]);
        }
    }

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

        public bool Equals(RoomPosition other) => Position.Equals(other.Position) && RoomCoord.Equals(other.RoomCoord);

        public override int GetHashCode() => HashCode.Combine(Position, RoomCoord);

        public static bool operator ==(RoomPosition left, RoomPosition right) => left.Equals(right);

        public static bool operator !=(RoomPosition left, RoomPosition right) => !(left == right);

        public static RoomPosition FromEncodedInt(int encodedInt)
        {
            // bit 0-0   (1): sim room
            // bit 1-7   (7): room x
            // bit 8-14  (7): room y
            // bit 15-20 (6): local x
            // bit 21-26 (6): local y
            bool isSimRoom = (encodedInt & 1) != 0;
            int roomX = ((encodedInt >> 1) & 127) - 64;
            int roomY = ((encodedInt >> 8) & 127) - 64;
            int localX = (encodedInt >> 15) & 63;
            int localY = (encodedInt >> 21) & 63;
            return new(new(localX, localY), isSimRoom ? RoomCoord.Sim : new(roomX, roomY));
        }

        public int ToEncodedInt()
        {
            // bit 0-0   (1): sim room
            // bit 1-7   (7): room x
            // bit 8-14  (7): room y
            // bit 15-20 (6): local x
            // bit 21-26 (6): local y
            int result = 0;
            if (RoomCoord.IsSim)
            {
                result |= 1;
            }
            else
            {
                result |= (RoomCoord.X + 64) << 1;
                result |= (RoomCoord.Y + 64) << 8;
            }
            result |= Position.X << 15;
            result |= Position.Y << 21;
            return result;
        }

        public override string ToString()
            => $"[{Position.X},{Position.Y}:{RoomCoord}]";
    }
}
