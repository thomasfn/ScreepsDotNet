﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScreepsDotNet.API
{
    /// <summary>
    /// Describes the state of a single body part belonging to a creep.
    /// </summary>
    /// <typeparam name="TBodyPartType"></typeparam>
    public readonly record struct BodyPart<TBodyPartType>
    (
        TBodyPartType Type,
        int Hits,
        World.ResourceType? Boost
    ) where TBodyPartType : unmanaged, Enum;

    /// <summary>
    /// Describes the body structure of a creep.
    /// </summary>
    public readonly struct BodyType<TBodyPartType> : IEquatable<BodyType<TBodyPartType>> where TBodyPartType : unmanaged, Enum
    {
        public static readonly BodyType<TBodyPartType> Invalid = new();

        private readonly (TBodyPartType bodyPartType, int quantity)[]? bodyPartTypes;
        private readonly int? hash;
        private readonly int totalPartCount;

        public ReadOnlySpan<(TBodyPartType bodyPartType, int quantity)> BodyPartTypes => bodyPartTypes ?? ReadOnlySpan<(TBodyPartType bodyPartType, int quantity)>.Empty;

        public bool IsValid => bodyPartTypes != null && bodyPartTypes.Length > 0;

        public IEnumerable<TBodyPartType> AsBodyPartList
        {
            get
            {
                var seq = Enumerable.Empty<TBodyPartType>();
                if (bodyPartTypes != null)
                {
                    foreach ((TBodyPartType tupleBodyPartType, int quantity) in bodyPartTypes)
                    {
                        seq = seq.Concat(Enumerable.Repeat(tupleBodyPartType, quantity));
                    }
                }
                return seq;
            }
        }

        public int TotalBodyPartCount => totalPartCount;

        public int this[TBodyPartType bodyPartType]
        {
            get
            {
                if (bodyPartTypes == null) { return 0; }
                int cnt = 0;
                foreach ((TBodyPartType tupleBodyPartType, int quantity) in bodyPartTypes)
                {
                    if (EqualityComparer<TBodyPartType>.Default.Equals(tupleBodyPartType, bodyPartType)) { cnt += quantity; }
                }
                return cnt;
            }
        }

        public BodyType(ReadOnlySpan<(TBodyPartType bodyPartType, int quantity)> bodyPartTypeTuples)
        {
            bodyPartTypes = bodyPartTypeTuples.ToArray();
            hash = CalculateHash();
            for (int i = 0; i < bodyPartTypes.Length; ++i)
            {
                totalPartCount += bodyPartTypes[i].quantity;
            }
        }

        public BodyType(IEnumerable<TBodyPartType> bodyPartTypes)
        {
            Span<(TBodyPartType bodyPartType, int quantity)> bodyPartTuples = stackalloc (TBodyPartType bodyPartType, int quantity)[bodyPartTypes.Count()];
            int bodyPartTuplesLength = 0;
            foreach (var bodyPartType in bodyPartTypes)
            {
                if (bodyPartTuplesLength == 0 || !EqualityComparer<TBodyPartType>.Default.Equals(bodyPartTuples[bodyPartTuplesLength - 1].bodyPartType, bodyPartType))
                {
                    bodyPartTuples[bodyPartTuplesLength] = (bodyPartType, 1);
                    ++bodyPartTuplesLength;
                }
                else
                {
                    ++bodyPartTuples[bodyPartTuplesLength - 1].quantity;
                }
                ++totalPartCount;
            }
            this.bodyPartTypes = bodyPartTuples[..bodyPartTuplesLength].ToArray();
            hash = CalculateHash();
        }

        public bool Has(TBodyPartType bodyPartType)
            => this[bodyPartType] > 0;

        private int CalculateHash()
        {
            if (bodyPartTypes == null || bodyPartTypes.Length == 0) { return HashCode.Combine(0); }
            int hash = bodyPartTypes[0].GetHashCode();
            for (int i = 1; i < bodyPartTypes.Length; ++i)
            {
                hash = HashCode.Combine(hash, bodyPartTypes[i].GetHashCode());
            }
            return hash;
        }

        public override bool Equals(object? obj) => obj is BodyType<TBodyPartType> type && Equals(type);

        public bool Equals(BodyType<TBodyPartType> other)
        {
            if (hash != other.hash) { return false; }
            if (bodyPartTypes == null || other.bodyPartTypes == null) { return false; }
            return bodyPartTypes.SequenceEqual(other.bodyPartTypes);
        }

        public override int GetHashCode()
            => hash ?? HashCode.Combine(0);

        public static bool operator ==(BodyType<TBodyPartType> left, BodyType<TBodyPartType> right) => left.Equals(right);

        public static bool operator !=(BodyType<TBodyPartType> left, BodyType<TBodyPartType> right) => !(left == right);

        public override string ToString()
        {
            if (bodyPartTypes == null || bodyPartTypes.Length == 0) { return "BodyType(INVALID)"; }
            var sb = new StringBuilder();
            sb.Append("BodyType(");
            bool isFirst = true;
            foreach (var (bodyPartType, quantity) in bodyPartTypes)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    sb.Append(", ");
                }
                sb.Append(quantity);
                sb.Append("x ");
                if (typeof(TBodyPartType) == typeof(World.BodyPartType))
                {
                    sb.Append(World.BodyPartTypes.Names[Convert.ToInt32(bodyPartType)]);
                }
                else
                {
                    sb.Append(bodyPartType.ToString());
                }
            }
            sb.Append(')');
            return sb.ToString();
        }
    }

}
