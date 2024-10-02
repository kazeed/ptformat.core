using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PtInfo.Core.Readers;
using Xunit;

namespace PtInfo.Core.Tests
{
    public class XorDecoderReaderTests
    {
        [Fact]
        public async Task UnXorFileAndSaveToLocalFile()
        {
            // Arrange
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<XorDecoderStream>();

            var inputFilePath = "assets/sample1.pts"; // Replace with the path to your input PTS file
            var outputFilePath = "assets/sample1_unxored.pts"; // Replace with the desired output file path

            // Ensure the output directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath));

            // Act
            using (var inputStream = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read))
            using (var xorReader = new XorDecoderStream(inputStream, logger))
            using (var outputStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
            {
                var unxoredContent = await xorReader.ReadToEndAsync();

                // Convert the unXORed string to bytes and write to output file
                var unxoredBytes = Encoding.ASCII.GetBytes(unxoredContent);
                await outputStream.WriteAsync(unxoredBytes);
            }

            // Assert
            Assert.IsTrue(File.Exists(outputFilePath), "The unXORed output file was not created.");

            var originalSize = new FileInfo(inputFilePath).Length;
            var outputSize = new FileInfo(outputFilePath).Length;
            Assert.IsTrue(outputSize > 0, "The output file should not be empty.");
            //Assert.AreEqual(originalSize, outputSize); // Ensure the sizes are consistent
        }
    }
}
