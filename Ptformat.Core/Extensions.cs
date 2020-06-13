using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ptformat.Core
{
    public static class Extensions
    {
        public static long FoundAt(this byte[] haystack, long n, byte[] needle)
        {
            if (haystack is null)
            {
                throw new ArgumentNullException(nameof(haystack));
            }

            if (needle is null)
            {
                throw new ArgumentNullException(nameof(needle));
            }

            long i, j, needle_n;
            needle_n = needle.Length;

            for (i = 0; i < n; i++)
            {
                long found = i;
                for (j = 0; j < needle_n; j++)
                {
                    if (haystack[i + j] != needle[j])
                    {
                        found = -1;
                        break;
                    }
                }

                if (found > 0) return found;
            }

            return -1;
        }

        public static byte[] AsBytes(this string s) => Encoding.UTF8.GetBytes(s);

        public static string AsString(this byte[] b) => Encoding.UTF8.GetString(b);

        public static T[] GetRange<T>(this IEnumerable<T> a, long idx, int n) => a.Skip((int)idx).Take(n).ToArray();
    }
}
