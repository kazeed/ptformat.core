using Ptformat.Core.Model;
using System;

namespace Ptformat.Core.Extensions
{
    public static class IntExtensions
    {
        /// <summary>
        /// Converts a long value to the corresponding ContentType enum value, if defined.
        /// </summary>
        /// <param name="value">The long value to convert.</param>
        /// <returns>The corresponding ContentType enum value if it exists; otherwise, null.</returns>
        public static ContentType ToContentType(this int value)
        {
            if (Enum.IsDefined(typeof(ContentType), value))
            {
                return (ContentType)value;
            }

            return ContentType.Invalid;
        }
    }
}
