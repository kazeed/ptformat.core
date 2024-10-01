using Ptformat.Core.Model;
using Ptformat.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ptformat.Core.Extensions
{
    public static class WavFileExtensions
    {
        /// <summary>
        /// Adds length information to an AudioRef by searching the relevant blocks.
        /// </summary>
        /// <param name="wavFile">The WavFile instance to update.</param>
        /// <param name="blocks">List of blocks to search for length information.</param>
        /// <returns>The WavFile with the length updated.</returns>
        public static AudioRef AddLength(this AudioRef wavFile, List<Block> blocks)
        {
            var lengthBlock = blocks
                .Where(b => b.ContentType == ContentType.WavListFull)
                .SelectMany(b => b.Children)
                .Where(c => c.ContentType == ContentType.WavMetadata)
                .SelectMany(c => c.Children.Where(d => d.ContentType == ContentType.WavSampleRateSize))
                .FirstOrDefault();

            if (lengthBlock == null)
            {
                return wavFile;
            }

            try
            {
                var length = EndianReader.ReadInt64(lengthBlock.RawData, 8, true);
                wavFile.Length = length;
                return wavFile;
            }
            catch (Exception)
            {
                return wavFile;
            }
        }
    }
}
