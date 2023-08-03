using System;

namespace ScreepsDotNet.API.World
{
    public readonly struct OwnerInfo : IEquatable<OwnerInfo>
    {
        /// <summary>
        /// The name of the owner user.
        /// </summary>
        public readonly string Username;

        public OwnerInfo(string username)
        {
            Username = username;
        }

        public override bool Equals(object? obj) => obj is OwnerInfo info && Equals(info);

        public bool Equals(OwnerInfo other) => Username == other.Username;

        public override int GetHashCode() => HashCode.Combine(Username);

        public static bool operator ==(OwnerInfo left, OwnerInfo right) => left.Equals(right);

        public static bool operator !=(OwnerInfo left, OwnerInfo right) => !(left == right);
    }
}
