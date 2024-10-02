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
            var inputfile = File.ReadAllBytes(inputFilePath);
            using var xorReader = new XorDecoderStream(inputfile, logger);
            var unxoredContent = xorReader.Decode();
            using var outputStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write);
            await outputStream.WriteAsync(unxoredContent);

            // Assert
            Assert.IsTrue(File.Exists(outputFilePath), "The unXORed output file was not created.");

            var originalSize = new FileInfo(inputFilePath).Length;
            var outputSize = new FileInfo(outputFilePath).Length;
            Assert.IsTrue(outputSize > 0, "The output file should not be empty.");
            //Assert.AreEqual(originalSize, outputSize); // Ensure the sizes are consistent
        }
    }
}
