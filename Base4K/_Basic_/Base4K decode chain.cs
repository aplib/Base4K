namespace Lex4K;
using System.Text;

public partial class Base4K
{
    /// <summary>Decodes binary data from Base4K chain encoded string into new span.</summary>
    /// <param name="encoded">The Base4K chain encoded string.</param>
    /// <returns>Span of decoded binary data.</returns>
    /// <exception cref="IndexOutOfRangeException">Invalid Base4K chain encoded string format.</exception>
    public static ReadOnlySpan<byte> DecodeChain(string encoded)
    {
        var bytes = Encoding.Unicode.GetBytes(encoded);
        byte[] buffer = new byte[CalcChainDecodeBufferSize(bytes.Length)];
        var decoded = DecodeChain(bytes, buffer);
        return buffer.AsSpan(0, decoded);
    }
}
