using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Ptformat.Core.Test
{
    public class BlockTypeExtractor
    {
        private const byte ZMARK = 0x5A; // Define ZMARK constant for readability
        private readonly string outputPath = "FoundBlockTypes.bin"; // Output binary file path

        [Fact]
        public void ExtractBlockTypes_FromUnxoredFile_ShouldSaveBlockTypesToBinaryFile()
        {
            // Adjust this path to point to your unxored PTS file
            var filePath = "E:\\Dev\\ptformat.core\\Ptformat.Core.Test\\assets\\sample1_unxored.pts";

            // Read the file data
            var fileData = File.ReadAllBytes(filePath);

            var uniqueBlockTypes = new SortedSet<ushort>(); // Store unique block types as ushort values

            // Scan through the file to identify ZMARK occurrences
            for (int index = 0; index < fileData.Length - 2; index++)
            {
                // Check for ZMARK
                if (fileData[index] == ZMARK)
                {
                    // Extract the two bytes following the ZMARK as the block type
                    var blockType = (ushort)((fileData[index + 1] << 8) | fileData[index + 2]);
                    uniqueBlockTypes.Add(blockType);

                    // Skip past the block header (advance 2 more bytes)
                    index += 2;
                }
            }

            // Assert that we found at least one block type
            Assert.AreNotEqual(uniqueBlockTypes.Count, 0);

            // Write the unique block types to a binary file
            using (var fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
            using (var bw = new BinaryWriter(fs))
            {
                foreach (var blockType in uniqueBlockTypes)
                {
                    bw.Write(blockType);
                }
            }

            // Verify that the file was written correctly
            Assert.IsTrue(File.Exists(outputPath), $"The binary file '{outputPath}' was not created.");
        }
    }
}
