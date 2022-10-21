namespace Lex4K;
using System.Runtime.InteropServices;

public partial class Base4K
{
    /// <summary>Decodes binary data from Base4K block encoded string into allocated new byte[] buffer.</summary>
    /// <param name="count">Output binary data (bytes) count to be decoded.</param>
    /// <param name="encoded">The Base4K block encoded string</param>
    /// <returns>Decoded binary data</returns>
    /// <exception cref="IndexOutOfRangeException">Invalid Base4K block encoded string format.</exception>
    public static byte[] DecodeBlockToNewBuffer(int count, ReadOnlySpan<char> encoded)
    {
        if (encoded.Length == 0)
            return null;
        byte[] buffer = new byte[count];
        var encoded_as_bytes = MemoryMarshal.Cast<char, byte>(encoded);
        DecodeBlock(count, encoded_as_bytes, buffer.AsSpan());
        return buffer;
    }
}
