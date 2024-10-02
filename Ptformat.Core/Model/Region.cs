namespace PtInfo.Core.Model
{
    public abstract class Region
    {
        public string Name { get; internal set; }
        public long Start { get; internal set; }
        public long Offset { get; internal set; }
        public long Length { get; internal set; }
    }
}
