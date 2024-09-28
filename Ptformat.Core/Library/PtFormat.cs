using System;
using System.IO;

namespace Ptformat.Core.Library
{
    public static partial class PtFormat
    {
        public const int KMaxTracks = 128;

        public static void Main(string[] args)
        {
            if (args is null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            if (args.Length < 1)
            {
                Console.WriteLine("Usage: ptformat <file.ptx>");
                return;
            }

            string filename = args[0];
            try
            {
                using var reader = new BinaryReader(new FileStream(filename, FileMode.Open));
                ParsePtFile(reader);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}");
            }
        }

        // Main parsing logic
        public static void ParsePtFile(BinaryReader reader)
        {
            if (reader is null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            try
            {
                int magicNumber = reader.ReadInt32();
                Console.WriteLine($"Magic Number: {magicNumber}");

                int trackCount = reader.ReadInt32();
                Console.WriteLine($"Number of Tracks: {trackCount}");

                for (int i = 0; i < trackCount; i++)
                {
                    AudioTrack track = new();
                    track.Name = ReadPascalString(reader);

                    int regionCount = reader.ReadInt32();
                    for (int j = 0; j < regionCount; j++)
                    {
                        AudioRegion region = new();
                        region.StartTime = reader.ReadInt32();
                        region.EndTime = reader.ReadInt32();
                        region.Name = ReadPascalString(reader);
                        track.Regions.Add(region);
                    }

                    Console.WriteLine($"Track: {track.Name} with {track.Regions.Count} regions");
                }
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("Reached the end of the file unexpectedly.");
            }
        }

        // Helper method to read Pascal-style strings
        private static string ReadPascalString(BinaryReader reader)
        {
            byte length = reader.ReadByte();
            byte[] strBytes = reader.ReadBytes(length);
            return System.Text.Encoding.ASCII.GetString(strBytes);
        }
    }
}