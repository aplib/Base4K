namespace Lex4K;
using System.Runtime.InteropServices;

public partial class Base4K
{
    /// <summary>Decodes binary data from Base4K chain encoded string into new span.</summary>
    /// <param name="encoded">The Base4K chain encoded string.</param>
    /// <returns>Span of decoded binary data.</returns>
    /// <exception cref="IndexOutOfRangeException">Invalid Base4K chain encoded string format.</exception>
    public static ReadOnlySpan<byte> DecodeChainToNewBuffer(ReadOnlySpan<char> encoded)
    {
        if (encoded.Length == 0)
            return null;
        byte[] buffer = new byte[CalcChainDecodeBufferSize(encoded.Length * 2)];
        var encoded_as_bytes = MemoryMarshal.Cast<char, byte>(encoded);
        var decoded = DecodeChain(encoded_as_bytes, buffer);
        return buffer.AsSpan(0, decoded);
    }
}
