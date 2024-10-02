using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using Xunit;

namespace PtInfo.Core.Test
{
    public class BlockAnalyzer
    {
        private const int ZMARK = 0x5A;
        private const int START_OFFSET = 28;
        private const int FMARK = 0x3F; // Field separator

        [Fact]
        public void AnalyzeFirstBlockTest()
        {
            // Load the unxored file
            var filePath = "E:\\Dev\\ptformat.core\\Ptformat.Core.Test\\assets\\sample1_unxored.pts";
            var fileData = File.ReadAllBytes(filePath);

            var outputFilePath = "block1_analysis.txt";
            using (var writer = new StreamWriter(outputFilePath))
            {
                ParseBlock(fileData, writer);
            }

            Assert.IsTrue(File.Exists(outputFilePath));
        }

        private static void ParseBlock(byte[] fileData, StreamWriter writer)
        {
            // Begin reading the block starting at the START_OFFSET
            var pos = START_OFFSET;

            // Ensure the block starts with ZMARK
            if (fileData[pos] != ZMARK)
            {
                writer.WriteLine("No ZMARK found at expected offset.");
                return;
            }

            writer.WriteLine($"ZMARK found at offset {START_OFFSET}");

            // Extract and print fields separated by FMARK (0x3F)
            pos += 1; // Move past the ZMARK

            while (pos < fileData.Length)
            {
                var nextFMark = Array.IndexOf(fileData, (byte)FMARK, pos);
                if (nextFMark == -1) break; // No more fields

                var fieldLength = nextFMark - pos;
                if (fieldLength > 0)
                {
                    var fieldValue = BitConverter.ToString(fileData, pos, fieldLength).Replace("-", " ");
                    writer.WriteLine($"Field: {fieldValue} (Length: {fieldLength} bytes)");
                }

                // Move to the next field after FMARK
                pos = nextFMark + 1;
            }
        }
    }
}