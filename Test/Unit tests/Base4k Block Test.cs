using Lex4K;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Test
{
    [TestClass]
    public partial class Base4k_UnitTest
    {
        
        [TestMethod]
        public void BlockEncodeDecodeTest()
        {
            var rnd = new Random((int)DateTime.Now.Ticks);
            for (int r = 0; r < 500; r++)
            {
                var buffersize = rnd.Next(0, 100000);
                var buffer = Setup.RandomBuffer(buffersize,/*seed*/ (int)DateTime.Now.Ticks);
                var offset = rnd.Next(0, buffersize);
                var count = rnd.Next(0, buffersize - offset);
                var test_span = new ReadOnlySpan<byte>(buffer, offset, count);


                var encoded = Base4K.EncodeBlockToString(new ReadOnlySpan<byte>(buffer, offset, count));
                Assert.IsTrue(encoded.All(chr => Base4K.IsBase4KChar(chr)));

                var decoded = Base4K.DecodeBlockToNewBuffer(count, encoded);
                Assert.IsTrue(((Span<byte>)decoded).SequenceEqual(test_span));

            }
        }
    }
}