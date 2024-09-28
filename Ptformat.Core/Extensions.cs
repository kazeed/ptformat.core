using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ptformat.Core
{
    public static class Extensions
    {
        public static long IndexOf<T>(this IEnumerable<T> haystack, IEnumerable<T> needle) where T : struct, IEquatable<T>
        {
            ArgumentNullException.ThrowIfNull(haystack);
            ArgumentNullException.ThrowIfNull(needle);

            // Convert both the haystack and needle to arrays for efficient indexing
            T[] haystackArray = haystack as T[] ?? haystack.ToArray();
            T[] needleArray = needle as T[] ?? needle.ToArray();

            int haystackLength = haystackArray.Length;
            int needleLength = needleArray.Length;

            if (needleLength == 0 || haystackLength == 0 || needleLength > haystackLength)
                return -1;

            // Loop through the haystack to find the needle
            for (int i = 0; i <= haystackLength - needleLength; i++)
            {
                if (haystackArray.AsSpan(i, needleLength).SequenceEqual(needleArray))
                {
                    return i;
                }
            }

            return -1;
        }

        public static byte[] AsBytes(this string s) => Encoding.ASCII.GetBytes(s);

        public static string AsString(this byte[] b) => Encoding.ASCII.GetString(b);

        public static T[] GetRange<T>(this IEnumerable<T> a, long idx, int n) where T : struct, IEquatable<T> => a.Skip((int)idx).Take(n).ToArray();
    }
}
