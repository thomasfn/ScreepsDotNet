using System;
using System.Collections.Generic;
using System.Linq;

namespace ScreepsDotNet
{
    public static class Utils
    {
        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> sequence) where T : class
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
            => sequence.Where(x => x != null);
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.

        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> sequence) where T : struct
            => sequence.Where(x => x != null).Select(x => x!.Value);

        public static T Best<T>(this IEnumerable<T> sequence, Func<T, double> scoreFn)
        {
            T? currentBest = default;
            double? currentBestScore = null;
            foreach (var item in sequence)
            {
                double score = scoreFn(item);
                if (currentBestScore == null || score > currentBestScore.Value)
                {
                    currentBestScore = score;
                    currentBest = item;
                }
            }
            if (currentBest == null) { throw new InvalidOperationException("Can't find best of empty sequence"); }
            return currentBest;
        }
    }
}
