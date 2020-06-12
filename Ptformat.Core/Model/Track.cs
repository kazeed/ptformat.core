using System;
using System.Diagnostics.CodeAnalysis;

namespace Ptformat.Core.Model
{
    public class Track: IEquatable<Track>, IComparable<Track>
    {
		public string Name { get; set; }

		public int Index { get; set; }

		public short Playlist { get; set; }

		public Region Region { get; set; }

		public static bool operator ==(Track left, Track right)
        {
            if (left is null)
            {
                return right is null;
            }

            return left.Equals(right);
        }

		public static bool operator !=(Track left, Track right) => !(left == right);

		public static bool operator <(Track left, Track right) => left is null ? right is object : left.CompareTo(right) < 0;

		public static bool operator <=(Track left, Track right) => left is null || left.CompareTo(right) <= 0;

		public static bool operator >(Track left, Track right) => left is object && left.CompareTo(right) > 0;

		public static bool operator >=(Track left, Track right) => left is null ? right is null : left.CompareTo(right) >= 0;

		public int CompareTo([AllowNull] Track other) => this.Index.CompareTo(other?.Index);

		public bool Equals([AllowNull] Track other) => this.Index.Equals(other?.Index);

		public override bool Equals(object obj)
        {
            return Equals(obj as Track);
        }

		public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }
}
