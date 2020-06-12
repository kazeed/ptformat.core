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

            var file = File.ReadAllBytes("assets/sample1.pts");
            var decoded = service.DecryptFile(file);
            Assert.IsNotNull(decoded);
            Assert.IsTrue(decoded.Length > 0);
        }
    }
}
