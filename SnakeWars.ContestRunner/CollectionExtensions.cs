using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

namespace SnakeTest
{

    static class CollectionExtensions
    {
        public static class ThreadSafeRandom
        {
            [ThreadStatic] private static Random Local;

            public static Random ThisThreadsRandom {
                get { return Local ?? (Local = new Random (unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
            }
        }

        // Source: http://stackoverflow.com/a/1262619/3568443
        public static void Shuffle<T> (this IList<T> list)
        {
            int n = list.Count;
            while (n > 1) {
                n--;
                int k = ThreadSafeRandom.ThisThreadsRandom.Next (n + 1);
                T value = list [k];
                list [k] = list [n];
                list [n] = value;
            }
        }

        public static ISet<T> ToSet<T>(this IEnumerable<T> input) => new HashSet<T>(input);
    }
    
}
