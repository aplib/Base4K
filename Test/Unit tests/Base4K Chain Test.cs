using Lex4K;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Test
{
    public partial class Base4k_UnitTest
    {
        [TestMethod]
        public void ChainEncodeDecode()
        {
            var rnd = new Random((int)DateTime.Now.Ticks);
            for (int r = 0; r < 250; r++)
            {
                var buffersize = rnd.Next(0, 100000);
                var buffer = Setup.RandomBuffer(buffersize,/*seed*/ (int)DateTime.Now.Ticks);
                var offset = rnd.Next(0, buffersize);
                var count = rnd.Next(0, buffersize - offset);
                var test_span = new ReadOnlySpan<byte>(buffer, offset, count);

                var encoded = Base4K.EncodeChainToString(test_span);
                Assert.IsTrue(encoded.All(chr => Base4K.IsBase4KChar(chr)));

                var decoded = Base4K.DecodeChainToNewBuffer(encoded);
                Assert.IsTrue(decoded.SequenceEqual(test_span));
            }
        }

        [TestMethod]
        public async Task ChainStreamEncodeDecode()
        {
            var rnd = new Random((int)DateTime.Now.Ticks);
            for (int r = 0; r < 250; r++)
            {
                var buffersize = rnd.Next(0, 100000);
                var buffer = Setup.RandomBuffer(buffersize,/*seed*/ (int)DateTime.Now.Ticks);
                var offset = rnd.Next(0, buffersize);
                var count = rnd.Next(0, buffersize - offset);
                
                var test_span = new Memory<byte>(buffer, offset, count);
                var test_data = test_span.ToArray();

                using var mem = new MemoryStream(test_data);
                using var encoded = new MemoryStream(Base4K.CalcChainMaxEncodeOutput(test_data.Length));
                using var decoded = new MemoryStream(test_data.Length);

                var encoded_count = await Base4K.EncodeChain(test_data.Length, mem, encoded);
                encoded.Seek(0, SeekOrigin.Begin);
                var decode_count = await Base4K.DecodeChain(encoded, decoded);

                Assert.AreEqual(test_span.Length, decode_count);
                Assert.IsTrue(new Span<byte>(decoded.GetBuffer(), 0, (int)decoded.Position).SequenceEqual(test_data));
            }
        }
    }     
}