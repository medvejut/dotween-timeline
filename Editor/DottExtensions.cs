using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace Dott.Editor
{
    public static class DottExtensions
    {
        public static void ForEach<T, TR>(this IEnumerable<T> source, Func<T, TR> func)
        {
            foreach (var element in source)
            {
                func(element);
            }
        }

        public static T GetRandom<T>(this T[] collection)
        {
            return collection[Random.Range(0, collection.Length)];
        }

        public static int IndexOf<T>(this T[] array, T value)
        {
            return Array.IndexOf(array, value);
        }

        public static int FindIndex<T>(this IList<T> source, Predicate<T> predicate)
        {
            for (var i = 0; i < source.Count; ++i)
            {
                if (predicate(source[i]))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}