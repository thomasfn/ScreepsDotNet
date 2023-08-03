using System;

namespace ScreepsDotNet.API.World
{
    public readonly struct ControllerReservation
    {
        /// <summary>
        /// The name of a player who reserved this controller.
        /// </summary>
        public readonly string Username;
        /// <summary>
        /// The amount of game ticks when the reservation will end.
        /// </summary>
        public readonly int TicksToEnd;

        public ControllerReservation(string username, int ticksToEnd)
        {
            Username = username;
            TicksToEnd = ticksToEnd;
        }
    }

    public readonly struct ControllerSign
    {
        /// <summary>
        /// The name of a player who signed this controller.
        /// </summary>
        public readonly string Username;
        /// <summary>
        /// The sign text.
        /// </summary>
        public readonly string Text;
        /// <summary>
        /// The sign time in game ticks.
        /// </summary>
        public readonly int Time;
        /// <summary>
        /// The sign real date.
        /// </summary>
        public readonly DateTime DateTime;

        public ControllerSign(string username, string text, int time, DateTime dateTime)
        {
            Username = username;
            Text = text;
            Time = time;
            DateTime = dateTime;
        }
    }

    public enum ControllerActivateSafeModeResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of this controller.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// There is another room in safe mode already.
        /// </summary>
        Busy = -4,
        /// <summary>
        /// There is no safe mode activations available.
        /// </summary>
        NotEnoughResources = -6,
        /// <summary>
        /// The previous safe mode is still cooling down, or the controller is upgradeBlocked, or the controller is downgraded for 50% plus 5000 ticks or more.
        /// </summary>
        Tired = -11,
    }

    public enum ControllerUnclaimResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of this controller.
        /// </summary>
        NotOwner = -1
    }

    /// <summary>
    /// Claim this structure to take control over the room. The controller structure cannot be damaged or destroyed.
    /// </summary>
    public interface IStructureController : IOwnedStructure
    {
        /// <summary>
        /// Whether using power is enabled in this room. Use PowerCreep.enableRoom to turn powers on.
        /// </summary>
        bool IsPowerEnabled { get; }

        /// <summary>
        /// Current controller level, from 0 to 8.
        /// </summary>
        int Level { get; }

        /// <summary>
        /// The current progress of upgrading the controller to the next level.
        /// </summary>
        int Progress { get; }

        /// <summary>
        /// The progress needed to reach the next level.
        /// </summary>
        int ProgressTotal { get; }

        /// <summary>
        /// An object with the controller reservation info if present.
        /// </summary>
        ControllerReservation? Reservation { get; }

        /// <summary>
        /// How many ticks of safe mode remaining, or undefined.
        /// </summary>
        int? SafeMode { get; }

        /// <summary>
        /// Safe mode activations available to use.
        /// </summary>
        int SafeModeAvailable { get; }

        /// <summary>
        /// During this period in ticks new safe mode activations will be blocked, undefined if cooldown is inactive.
        /// </summary>
        int? SafeModeCooldown { get; }

        /// <summary>
        /// An object with the controller sign info if present.
        /// </summary>
        ControllerSign? Sign { get; }

        /// <summary>
        /// The amount of game ticks when this controller will lose one level.
        /// This timer is set to 50% on level upgrade or downgrade, and it can be increased by using Creep.upgradeController.
        /// Must be full to upgrade the controller to the next level.
        /// </summary>
        int TicksToDowngrade { get; }

        /// <summary>
        /// The amount of game ticks while this controller cannot be upgraded due to attack. Safe mode is also unavailable during this period.
        /// </summary>
        int UpgradeBlocked { get; }

        /// <summary>
        /// Activate safe mode if available.
        /// </summary>
        /// <returns></returns>
        ControllerActivateSafeModeResult ActivateSafeMode();

        /// <summary>
        /// Make your claimed controller neutral again.
        /// </summary>
        /// <returns></returns>
        ControllerUnclaimResult Unclaim();
    }
}
