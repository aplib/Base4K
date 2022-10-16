namespace Lex4K;
using System.Text;

public partial class Base4K
{
    /// <summary>Encodes binary data into encoded text, represented as a Base4K chain text.</summary>
    /// <param name="bytes">The input span that contains binary data that needs to be encoded.</param>
    /// <returns>Base4K chain string</returns>
    public static string EncodeChainString(ReadOnlySpan<byte> bytes)
    {
        var max_output_length = CalcChainMaxEncodeOutput(bytes.Length);
        Span<byte> output_buffer = (max_output_length < 16384) ? stackalloc byte[max_output_length] : new byte[max_output_length];
        int encoded = EncodeChain(bytes, output_buffer);
        return Encoding.Unicode.GetString(output_buffer[..encoded]);
    }

    /// <summary>Encodes binary data into encoded text, represented as a Base4K chain text and append the encoded to string builder.</summary>
    /// <param name="bytes">The input span that contains binary data that needs to be encoded.</param>
    /// <param name="string_builder">The string builder for output the result.</param>
    /// <returns>Length of the added encoded fragment in characters</returns>
    public static int EncodeChainToStringBuilder(ReadOnlySpan<byte> bytes, StringBuilder string_builder)
    {
        var max_output_length = CalcChainMaxEncodeOutput(bytes.Length);
        Span<byte> output_buffer = (max_output_length < 16384) ? stackalloc byte[max_output_length] : new byte[max_output_length];
        int encoded = EncodeChain(bytes, output_buffer);
        string_builder.Append(Encoding.Unicode.GetString(output_buffer[..encoded]));
        return encoded >> 1;// in chars
    }
}
