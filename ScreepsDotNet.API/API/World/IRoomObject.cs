using System;
using System.Collections.Generic;

namespace ScreepsDotNet.API.World
{
    public readonly struct Effect
    {
        /// <summary>
        /// Effect ID of the applied effect. Can be either natural effect ID or Power ID.
        /// </summary>
        public readonly int EffectId;

        /// <summary>
        /// Power level of the applied effect. Absent if the effect is not a Power effect.
        /// </summary>
        public readonly int? Level;

        /// <summary>
        /// How many ticks will the effect last.
        /// </summary>
        public readonly int TicksRemaining;

        public Effect(int effectId, int? level, int ticksRemaining)
        {
            EffectId = effectId;
            Level = level;
            TicksRemaining = ticksRemaining;
        }
    }

    public readonly struct RoomPosition : IEquatable<RoomPosition>
    {
        /// <summary>
        /// The position within the room.
        /// </summary>
        public readonly Position Position;

        /// <summary>
        /// The name of the room.
        /// </summary>
        public readonly string RoomName;

        public RoomPosition(Position position, string roomName)
        {
            Position = position;
            RoomName = roomName;
        }

        public override bool Equals(object? obj) => obj is RoomPosition position && Equals(position);

        public bool Equals(RoomPosition other) => Position.Equals(other.Position) && RoomName == other.RoomName;

        public override int GetHashCode() => HashCode.Combine(Position, RoomName);

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
            Span<char> roomName = stackalloc char[6];
            int ptr = 0;
            if (isSimRoom)
            {
                "sim".CopyTo(roomName);
                ptr = 3;
            }
            else
            {
                if (roomX < -10)
                {
                    roomName[ptr++] = 'W';
                    roomName[ptr++] = (char)('0' - ((roomX + 1) / 10));
                    roomName[ptr++] = (char)('0' - ((roomX + 1) % 10));
                }
                else if (roomX < 0)
                {
                    roomName[ptr++] = 'W';
                    roomName[ptr++] = (char)('0' - ((roomX + 1) % 10));
                }
                else if (roomX > 9)
                {
                    roomName[ptr++] = 'E';
                    roomName[ptr++] = (char)('0' + (roomX / 10));
                    roomName[ptr++] = (char)('0' + (roomX % 10));
                }
                else
                {
                    roomName[ptr++] = 'E';
                    roomName[ptr++] = (char)('0' + (roomX % 10));
                }
                if (roomY < -10)
                {
                    roomName[ptr++] = 'S';
                    roomName[ptr++] = (char)('0' - ((roomY + 1) / 10));
                    roomName[ptr++] = (char)('0' - ((roomY + 1) % 10));
                }
                else if (roomY < 0)
                {
                    roomName[ptr++] = 'S';
                    roomName[ptr++] = (char)('0' - ((roomY + 1) % 10));
                }
                else if (roomY > 9)
                {
                    roomName[ptr++] = 'N';
                    roomName[ptr++] = (char)('0' + (roomY / 10));
                    roomName[ptr++] = (char)('0' + (roomY % 10));
                }
                else
                {
                    roomName[ptr++] = 'N';
                    roomName[ptr++] = (char)('0' + (roomY % 10));
                }
            }
            return new(new(localX, localY), roomName[0..ptr].ToString());

        }

        public int ToEncodedInt()
        {
            // bit 0-0   (1): sim room
            // bit 1-7   (7): room x
            // bit 8-14  (7): room y
            // bit 15-20 (6): local x
            // bit 21-26 (6): local y
            int result = 0;
            if (RoomName == "sim")
            {
                result |= 1;
            }
            else
            {
                int roomX, roomY;
                int ptr = 0;

                if (RoomName[ptr] == 'W')
                {
                    if (RoomName.Length > ptr + 2 && char.IsDigit(RoomName[ptr + 2]))
                    {
                        roomX = -((RoomName[ptr + 1] - '0') * 10 + (RoomName[ptr + 2] - '0') + 1);
                        ptr += 3;
                    }
                    else
                    {
                        roomX = -(RoomName[ptr + 1] - '0' + 1);
                        ptr += 2;
                    }
                    
                }
                else if (RoomName[ptr] == 'E')
                {
                    if (RoomName.Length > ptr + 2 && char.IsDigit(RoomName[ptr + 2]))
                    {
                        roomX = (RoomName[ptr + 1] - '0') * 10 + (RoomName[ptr + 2] - '0');
                        ptr += 3;
                    }
                    else
                    {
                        roomX = RoomName[ptr + 1] - '0';
                        ptr += 2;
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Room name '{RoomName}' does not follow standard pattern");
                }

                if (RoomName[ptr] == 'S')
                {
                    if (RoomName.Length > ptr + 2 && char.IsDigit(RoomName[ptr + 2]))
                    {
                        roomY = -((RoomName[ptr + 1] - '0') * 10 + (RoomName[ptr + 2] - '0') + 1);
                        ptr += 3;
                    }
                    else
                    {
                        roomY = -(RoomName[ptr + 1] - '0' + 1);
                        ptr += 2;
                    }

                }
                else if (RoomName[ptr] == 'N')
                {
                    if (RoomName.Length > ptr + 2 && char.IsDigit(RoomName[ptr + 2]))
                    {
                        roomY = (RoomName[ptr + 1] - '0') * 10 + (RoomName[ptr + 2] - '0');
                        ptr += 3;
                    }
                    else
                    {
                        roomY = RoomName[ptr + 1] - '0';
                        ptr += 2;
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Room name '{RoomName}' does not follow standard pattern");
                }

                result |= (roomX + 64) << 1;
                result |= (roomY + 64) << 8;
            }
            result |= Position.X << 15;
            result |= Position.Y << 21;
            return result;
        }

        public override string ToString()
            => $"[{Position.X},{Position.Y}:{RoomName}]";
    }

    /// <summary>
    /// Any object with a position in a room. Almost all game objects prototypes are derived from RoomObject.
    /// </summary>
    public interface IRoomObject
    {
        /// <summary>
        /// Gets if this object still exists.
        /// May return false if a reference to this object is held across ticks and the underlying object is no longer represented in the JS API.
        /// If this returns false, any attempts to access this object's properties or methods will throw an exception.
        /// </summary>
        bool Exists { get; }

        /// <summary>
        /// Applied effects.
        /// </summary>
        IEnumerable<Effect> Effects { get; }

        /// <summary>
        /// An object representing the global position of this object.
        /// </summary>
        RoomPosition RoomPosition { get; }

        /// <summary>
        /// An object representing the local position of this object within the room.
        /// </summary>
        Position LocalPosition => RoomPosition.Position;

        /// <summary>
        /// The link to the Room object. May be undefined in case if an object is a flag or a construction site and is placed in a room that is not visible to you.
        /// </summary>
        IRoom? Room { get; }
    }
}
