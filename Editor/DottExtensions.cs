using System;
using System.Collections.Generic;
using UnityEngine;
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

        public static bool IsRightMouseButton(this Event @event)
        {
            const int mouseButtonLeft = 0;
            if (Application.platform == RuntimePlatform.OSXEditor && @event.control && @event.button == mouseButtonLeft)
            {
                return true;
            }

            const int mouseButtonRight = 1;
            return @event.button == mouseButtonRight;
        }
    }
}