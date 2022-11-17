namespace Lex4K;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Basic decoding methods

public partial class Base4K
{
    /// <summary>Decodes binary data from Base4K block encoded string into output span.</summary>
    /// <param name="count">Output binary data (bytes) count to be decoded.</param>
    /// <param name="bytes">The input span that contains binary data that needs to be decoded.</param>
    /// <param name="output">The output span for encoded block</param>
    /// <exception cref="IndexOutOfRangeException">Invalid Base4K block encoded string format.</exception>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public unsafe static void DecodeBlock(int count, ReadOnlySpan<byte> bytes, Span<byte> output)
    {
        var left = count; // bytes
        fixed (ushort* revert = Revert)
        fixed (byte* fix_s_as_bytes = &MemoryMarshal.GetReference(bytes), fix_t_as_bytes = &MemoryMarshal.GetReference(output))
        {
            var s_as_shorts = (ushort*)fix_s_as_bytes;
            var t_as_shorts = (ushort*)fix_t_as_bytes;

            // x6
            int x6 = left / 6;
            left -= x6 * 6;
            while (x6-- > 0)
            {
                ushort index_0 = revert[*s_as_shorts];
                ushort index_1 = revert[*(s_as_shorts + 1)];
                ushort index_2 = revert[*(s_as_shorts + 2)];
                ushort index_3 = revert[*(s_as_shorts + 3)];
                s_as_shorts += 4;
                *t_as_shorts = (ushort)(index_0 | index_1 << 12);
                *(t_as_shorts + 1) = (ushort)(index_1 >> 4 | index_2 << 8);
                *(t_as_shorts + 2) = (ushort)(index_2 >> 8 | index_3 << 4);
                t_as_shorts += 3;
            }

            byte* t_as_bytes = (byte*)t_as_shorts;

            // x3
            if (left > 2)
            {
                left -= 3;
                ushort index_0 = revert[*s_as_shorts];
                ushort index_1 = revert[*(s_as_shorts + 1)];
                s_as_shorts += 2;
                *t_as_bytes = (byte)index_0;
                *(t_as_bytes + 1) = (byte)(index_0 >> 8 | (index_1 & 0x000f) << 4);
                *(t_as_bytes + 2) = (byte)(index_1 >> 4);
                t_as_bytes += 3;
            }

            switch (left)
            {
                case 1:
                    *t_as_bytes = (byte)revert[*s_as_shorts];
                    break;
                case 2:
                    *t_as_bytes = (byte)revert[*s_as_shorts];
                    *(t_as_bytes + 1) = (byte)revert[*(s_as_shorts + 1)];
                    break;
            }
        }
    }

    // <summary>Decodes binary data from input span  Base4K chain encoded into output span.</summary>
    /// <param name="bytes">The input stream.</param>
    /// <param name="output">The output stream.</param>
    /// <exception cref="IndexOutOfRangeException">Invalid Base4K chain encoded string format.</exception>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static int DecodeChain(ReadOnlySpan<byte> bytes, Span<byte> output)
    {
        if ((bytes.Length & 1) != 0)
#pragma warning disable CA2201 // Do not raise reserved exception types
            throw new IndexOutOfRangeException("Invalid Base4K chain format.");
#pragma warning restore CA2201 // Do not raise reserved exception types

        var s_as_shorts = MemoryMarshal.Cast<byte, ushort>(bytes);
        int s_as_shorts_position = 0; // shorts
        int t_as_bytes_position = 0; // shorts

        while (s_as_shorts_position < bytes.Length)
        {
            // read length of block

            int block_length = Revert[s_as_shorts[s_as_shorts_position++]];
            if (block_length == 0)
                return t_as_bytes_position;

            // read inline block

            int source_block_bytes = CalcBlockEncodeOutput(block_length);
            DecodeBlock(block_length, bytes.Slice(s_as_shorts_position << 1, source_block_bytes), output.Slice(t_as_bytes_position, block_length));
            s_as_shorts_position += source_block_bytes >> 1;
            t_as_bytes_position += block_length;
        }
        return t_as_bytes_position;
    }

    // <summary>Decodes binary data from Base4K block into output span.</summary>
    /// <param name="input">The input stream.</param>
    /// <param name="output">The output stream.</param>
    /// <exception cref="IndexOutOfRangeException">Invalid Base4K chain encoded string format.</exception>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public async static Task<int> DecodeChain(Stream input, Stream output)
    {
        int output_count = 0; // all chain data bytes counter
        byte[] buffer = new byte[MAX_DATABLOCK_IN_CHAIN_OUTPUT_BYTES];
        while (true)
        {
            // read length of block
            int buffer_pos = 0, to_read = 2;
            do
            {
                int bytes_read = await input.ReadAsync(buffer, buffer_pos, to_read);
                to_read -= bytes_read;
                buffer_pos += bytes_read;

            } while (to_read > 0);

            int block_length = Revert[(ushort)BitConverter.ToInt16(buffer, 0)];
            if (block_length == 0)
                return output_count;

            // read inline block

            to_read = CalcBlockEncodeOutput(block_length);
            buffer_pos = 0;
            do
            {
                int bytes_read = await input.ReadAsync(buffer, buffer_pos, to_read);
                to_read -= bytes_read;
                buffer_pos += bytes_read;

            } while (to_read > 0);

            // dencode buffer part that was filled in place
            DecodeBlock(block_length, new ReadOnlySpan<byte>(buffer, 0, buffer_pos), buffer);

            // write decoded part
            await output.WriteAsync(buffer, 0, block_length);
            output_count += block_length;
        }
    }
}