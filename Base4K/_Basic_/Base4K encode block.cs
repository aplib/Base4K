namespace Lex4K;
using System.Text;

public partial class Base4K
{
    /// <summary>Encodes binary data into encoded text, represented as a Base4K block.</summary>
    /// <param name="bytes">The input span that contains binary data that needs to be encoded.</param>
    /// <returns>Base4K block string</returns>
    public static string EncodeBlockToString(ReadOnlySpan<byte> bytes)
    {
        var output_length = CalcBlockEncodeOutput(bytes.Length);
        Span<byte> output_buffer = (output_length < 16384) ? stackalloc byte[output_length] : new byte[output_length];
        EncodeBlock(bytes, output_buffer);
        return Encoding.Unicode.GetString(output_buffer);
    }

    /// <summary>Encodes binary data into encoded text, represented as a Base4K unary block,
    /// and append the encoded to string builder.</summary>
    /// <param name="bytes">The input span that contains binary data that needs to be encoded.</param>
    /// <param name="string_builder">The string builder for output the result.</param>
    /// <returns>Length of the added encoded fragment in characters</returns>
    public static int EncodeBlockToStringBuilder(ReadOnlySpan<byte> bytes, StringBuilder string_builder)
    {
        var output_length = CalcBlockEncodeOutput(bytes.Length);
        Span<byte> output_buffer = (output_length < 16384) ? stackalloc byte[output_length] : new byte[output_length];
        EncodeBlock(bytes, output_buffer);
        string_builder.Append(Encoding.Default.GetString(output_buffer));
        return output_length >> 1;// in chars
    }
}
