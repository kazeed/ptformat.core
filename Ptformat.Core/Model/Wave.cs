using System;

namespace Ptformat.Core.Model
{
    public class Wave : IEquatable<Wave>
    {
        public string Filename { get; set; }

        public int Index { get; set; }

        public long AbsolutePosition { get; set; }

        public long Length { get; set; }

        public bool Equals(Wave other)
        {
            if (other is null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            return this.Filename.Equals(other.Filename);
        }

        public override bool Equals(object obj) => Equals(obj as Wave);

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }
}
