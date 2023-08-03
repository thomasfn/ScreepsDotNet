using System;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    internal static class BodyPartTypeExtensions
    {
        public static string ToJS(this BodyPartType bodyPartType)
            => bodyPartType switch
            {
                BodyPartType.Attack => "attack",
                BodyPartType.Carry => "carry",
                BodyPartType.Heal => "heal",
                BodyPartType.Move => "move",
                BodyPartType.RangedAttack => "ranged_attack",
                BodyPartType.Tough => "tough",
                BodyPartType.Work => "work",
                BodyPartType.Claim => "claim",
                _ => throw new NotImplementedException($"Unknown body part type '{bodyPartType}'"),
            };

        public static BodyPartType ParseBodyPartType(this string str)
            => str switch
            {
                "attack" => BodyPartType.Attack,
                "carry" => BodyPartType.Carry,
                "heal" => BodyPartType.Heal,
                "move" => BodyPartType.Move,
                "ranged_attack" => BodyPartType.RangedAttack,
                "tough" => BodyPartType.Tough,
                "work" => BodyPartType.Work,
                "claim" => BodyPartType.Claim,
                _ => throw new NotImplementedException($"Unknown body part type '{str}'"),
            };
    }
}
