namespace Lex4K;
using System.Text;

public partial class Base4K
{
    /// <summary>Decodes binary data from Base4K block encoded string into allocated new byte[] buffer.</summary>
    /// <param name="count">Output binary data (bytes) count to be decoded.</param>
    /// <param name="encoded">The Base4K block encoded string</param>
    /// <returns>Decoded binary data</returns>
    /// <exception cref="IndexOutOfRangeException">Invalid Base4K block encoded string format.</exception>
    public static byte[] DecodeBlockToNewBuffer(int count, string encoded)
    {
        if (string.IsNullOrWhiteSpace(encoded))
            return null;
        
        byte[] buffer = new byte[count];
        var bytes = Encoding.Unicode.GetBytes(encoded);
        DecodeBlock(count, bytes, buffer);
        return buffer;
    }
}
