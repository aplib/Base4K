namespace Lex4K;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Basic encoding methods

public partial class Base4K
{
    /// <summary>Encodes binary data as fixed size Base4K text block into output span.</summary>
    /// <param name="bytes">The input span that contains binary data that needs to be encoded.</param>
    /// <param name="output">The output span for encoded block.</param>
    /// <returns>Bytes written to output</returns>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public unsafe static int EncodeBlock(ReadOnlySpan<byte> bytes, Span<byte> output)
    {
        var left = bytes.Length; // bytes
        fixed (ushort* bin = Bin)
        fixed (byte* fix_s_as_bytes = &MemoryMarshal.GetReference(bytes), fix_t_as_bytes = &MemoryMarshal.GetReference(output))
        {
            var s_as_shorts = (ushort*)fix_s_as_bytes;
            var t_as_shorts = (ushort*)fix_t_as_bytes;

            // x6 --> x8
            int x6 = left / 6;
            left -= x6 * 6;
            while (x6-- > 0)
            {
                uint short_0 = *s_as_shorts;
                uint short_1 = *(s_as_shorts + 1);
                uint short_2 = *(s_as_shorts + 2);
                s_as_shorts += 3;

                *t_as_shorts = bin[short_0 & 0x0fff];
                *(t_as_shorts + 1) = bin[(short_0 >> 12) | ((short_1 & 0x00ff) << 4)];
                *(t_as_shorts + 2) = bin[((short_1 & 0xff00) >> 8) | ((short_2 & 0x000f) << 8)];
                *(t_as_shorts + 3) = bin[short_2 >> 4];
                t_as_shorts += 4;
            }

            var s_as_bytes = (byte*)s_as_shorts;

            // x3 --> x4
            if (left >= 3)
            {
                left -= 3;
                uint byte_0 = *s_as_bytes;
                uint byte_1 = *(s_as_bytes + 1);
                uint byte_2 = *(s_as_bytes + 2);
                s_as_bytes += 3;
                *t_as_shorts = bin[byte_0 | ((byte_1 & 0x000f) << 8)];
                *(t_as_shorts + 1) = bin[(byte_1 >> 4) | (byte_2 << 4)];
                t_as_shorts += 2;
            }

            // x1 --> x2
            switch (left)
            {
                case 1:
                    *t_as_shorts = bin[*s_as_bytes];
                    break;
                case 2:
                    *t_as_shorts = bin[*s_as_bytes];
                    *(t_as_shorts + 1) = bin[*(s_as_bytes + 1)];
                    break;
            }
        }
        return bytes.Length / 3 * 4 + bytes.Length % 3 * 2; ;
    }

    /// <summary>Encodes binary data from a stream to an output stream as Base4K block text</summary>
    /// <param name="count">binary data bytes count.</param>
    /// <param name="input">The input stream.</param>
    /// <param name="output">The output stream.</param>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static async Task EncodeBlock(int count, Stream input, Stream output)
    {
        var left = count; // bytes
        int buffer_length = count < 6 ? 6 : count > 252 ? 252 : count;
        byte[] buffer = new byte[buffer_length];
        int buffer_decount;
        int buffer_pos;
        int encode_buffer_length = Math.Min(CalcBlockEncodeOutput(count), 336);
        byte[] encode_buffer = new byte[encode_buffer_length];
        int bytes_read;

        while (left > 0)
        {
            // read available data to buffer
            buffer_pos = 0;
            buffer_decount = left > 252 ? 252 : left;
            do
            {
                bytes_read = await input.ReadAsync(buffer, buffer_pos, buffer_decount);
                left -= bytes_read;
                buffer_pos += bytes_read;
                buffer_decount -= bytes_read;

            } while (buffer_pos < buffer_decount);

            // encode buffer part that was filled
            int encoded = EncodeBlock(new ReadOnlySpan<byte>(buffer, 0, buffer_pos), encode_buffer);

            // write encoded part
            await output.WriteAsync(encode_buffer, 0, encoded);
        }
    }

    /// <summary>Encodes binary data as Base4K chain string into output span.</summary>
    /// <param name="bytes">The input span that contains binary data that needs to be encoded.</param>
    /// <param name="output">The output span for encoded chain.</param>
    /// <returns>Bytes written to output</returns>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static int EncodeChain(ReadOnlySpan<byte> bytes, Span<byte> output)
    {
        var left = bytes.Length; // bytes
        var t_as_shorts = MemoryMarshal.Cast<byte, ushort>(output);
        int t_as_shorts_position = 0; // shorts
        int bytes_position = 0; // bytes

        // xMAX_DATABLOCK_IN_CHAIN
        int xMAX_DATABLOCK_IN_CHAIN = left / MAX_DATABLOCK_IN_CHAIN;
        while (xMAX_DATABLOCK_IN_CHAIN-- > 0)
        {
            t_as_shorts[t_as_shorts_position++] = Bin[MAX_DATABLOCK_IN_CHAIN];
            int encodedx6 = EncodeBlock(bytes.Slice(bytes_position, MAX_DATABLOCK_IN_CHAIN), output.Slice(t_as_shorts_position << 1, MAX_DATABLOCK_IN_CHAIN_OUTPUT_BYTES));
            bytes_position += MAX_DATABLOCK_IN_CHAIN;
            t_as_shorts_position += (encodedx6 >> 1);
            left -= MAX_DATABLOCK_IN_CHAIN;
        }

        t_as_shorts[t_as_shorts_position++] = Bin[left];
        int last_output = CalcBlockEncodeOutput(left);
        int encoded = EncodeBlock(bytes.Slice(bytes_position, left), output.Slice(t_as_shorts_position << 1, last_output));
        t_as_shorts_position += (encoded >> 1);
        // end of chain char
        t_as_shorts[t_as_shorts_position++] = END_OF_CHAIN_CHAR;
        return t_as_shorts_position << 1;
    }

    /// <summary>Encodes binary data from a stream to an output stream in Base4K chain format</summary>
    /// <param name="count">Input binary data (bytes) count to be encoded.</param>
    /// <param name="input">The input stream.</param>
    /// <param name="output">The output stream.</param>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static async Task<int> EncodeChain(int count, Stream input, Stream output)
    {
        int output_count = 0;
        int left = count;
        byte[] buffer = new byte[Math.Min(count, MAX_DATABLOCK_IN_CHAIN)];
        byte[] encoded_buffer = new byte[CalcBlockEncodeOutput(buffer.Length) + 4];

        while (left > 0)
        {
            int buffer_pos = 0;
            // read available data to buffer
            int to_read = left > MAX_DATABLOCK_IN_CHAIN ? MAX_DATABLOCK_IN_CHAIN : left;
            do
            {
                int bytes_read = await input.ReadAsync(buffer, buffer_pos, to_read);
                left -= bytes_read;
                to_read -= bytes_read;
                buffer_pos += bytes_read;

            } while (to_read > 0);

            // encode buffer part that was filled
            int encoded = EncodeBlock(new ReadOnlySpan<byte>(buffer, 0, buffer_pos), encoded_buffer);

            // Block-length char
            await output.WriteAsync(BitConverter.GetBytes(Bin[buffer_pos]), 0, 2);

            // write encoded part
            await output.WriteAsync(encoded_buffer, 0, encoded);
            output_count += encoded;
        }

        // Chain finalization
        await output.WriteAsync(BitConverter.GetBytes(END_OF_CHAIN_CHAR), 0, 2);
        return output_count + 2;
    }
}