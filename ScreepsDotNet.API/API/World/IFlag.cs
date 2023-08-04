using System;

namespace ScreepsDotNet.API.World
{
    public enum FlagSetColorResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// color or secondaryColor is not a valid color constant.
        /// </summary>
        InvalidArgs = -10
    }

    public enum FlagSetPositionResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// The target provided is invalid.
        /// </summary>
        InvalidTarget = -7
    }

    /// <summary>
    /// A flag. Flags can be used to mark particular spots in a room. Flags are visible to their owners only. You cannot have more than 10,000 flags.
    /// </summary>
    public interface IFlag : IRoomObject
    {
        /// <summary>
        /// Flag primary color.
        /// </summary>
        Color Color { get; }

        /// <summary>
        /// A shorthand to Memory.flags[flag.name]. You can use it for quick access the flag's specific memory data object.
        /// </summary>
        object Memory { get; }

        /// <summary>
        /// Flag’s name. You can choose the name while creating a new flag, and it cannot be changed later. This name is a hash key to access the flag via the Game.flags object. The maximum name length is 100 charactes.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Flag secondary color.
        /// </summary>
        Color SecondaryColor { get; }

        /// <summary>
        /// Remove the flag.
        /// </summary>
        void Remove();

        /// <summary>
        /// Set new color of the flag.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="secondaryColor"></param>
        /// <returns></returns>
        FlagSetColorResult SetColor(Color color, Color? secondaryColor = null);

        /// <summary>
        /// Set new position of the flag.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        FlagSetPositionResult SetPosition(Position position);

        /// <summary>
        /// Set new position of the flag.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        FlagSetPositionResult SetPosition(RoomPosition position);
    }
}
