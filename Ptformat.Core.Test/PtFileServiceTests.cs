using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xunit;

namespace Ptformat.Core.Test
{
    public class PtFileServiceTests
    {
        [Fact]
        public async void UnxoringReturnsDecoded()
        {
            var service = new PtFileService();

            var file = await File.ReadAllBytesAsync("assets/sample1.pts");
            using (var ms = new MemoryStream(file))
            {
                var decoded = await service.DecryptFile(ms);
                Assert.IsNotNull(decoded);
                Assert.IsTrue(decoded.Length > 0);
            }
        }
    }
}
