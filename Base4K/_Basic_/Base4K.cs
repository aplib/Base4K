namespace Lex4K;
using System.Runtime.CompilerServices;
/// <summary>
/// Base4K encoding provider
/// </summary>
public partial class Base4K
{
    // Constants & calculations

    /// <summary>
    /// The maximum length of the data block in the chain, without size of block header.
    /// Chain format(UTF16):
    ///  {Chain block header[1]}{Chain block data[1..MAX_DATABLOCK_IN_CHAIN]}{Finalizer[1]}
    /// Where
    ///  header - Base4K char (exclude CHAIN_FINALIZATION_CHAR) the index of whose represents the block size in the chain
    ///  Chain block data - block data, Base4K chars
    ///  Finalizer - Base4K CHAIN_FINALIZATION_CHAR (Base4k.Char[0] 'A')
    /// </summary>
    internal const ushort MAX_DATABLOCK_IN_CHAIN = 4095;
    /// <summary>
    /// Max data block in chain (chars, except block-length char only data chars).
    /// Not for calculations, only for buffer memory allocation(!) Real value variative by data length.
    /// </summary>
    internal const ushort MAX_DATABLOCK_IN_CHAIN_OUTPUT_LENGTH = 1 + MAX_DATABLOCK_IN_CHAIN / 3 * 2 + MAX_DATABLOCK_IN_CHAIN % 3;
    /// <summary>
    /// Max data block in chain (bytes, except block-length char only data chars).
    /// Not for calculations, only for buffer memory allocation(!) Real value variative by data length.
    /// </summary>
    internal const ushort MAX_DATABLOCK_IN_CHAIN_OUTPUT_BYTES = 2 + MAX_DATABLOCK_IN_CHAIN / 3 * 4 + MAX_DATABLOCK_IN_CHAIN % 3 * 2;
    /// <summary>
    /// Chain finalization char 'A'
    /// </summary>
    internal const ushort END_OF_CHAIN_CHAR = 'A'; // index 0

    /// <summary>
    /// Returns the length (in bytes) of the resulting encoded Base4K block text.
    /// </summary>
    /// <param name="bytes">binary data bytes count.</param>
    /// <returns>The length (in bytes) of the result Base4K block text.</returns>
    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public static int CalcBlockEncodeOutput(int bytes) => bytes / 3 * 4 + bytes % 3 * 2;
    /// <summary>
    /// Calculate maximum length (in bytes) of the resulting encoded Base4K chain text.
    /// Not for later calculations, only for buffer memory allocation(!) Real value variative by data length.
    /// </summary>
    /// <param name="bytes">binary data bytes count.</param>
    /// <returns>The length (in bytes) of the result Base4K chain text.</returns>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static int CalcChainMaxEncodeOutput(int bytes)
    {
        int max_blocks_count = bytes / MAX_DATABLOCK_IN_CHAIN;
        int max_blocks = max_blocks_count * MAX_DATABLOCK_IN_CHAIN_OUTPUT_BYTES;
        int left = bytes - max_blocks_count * MAX_DATABLOCK_IN_CHAIN;
        return max_blocks + 2 + CalcBlockEncodeOutput(left) + 2;
    }
    /// <summary>
    /// Calculate the size of the decoding output buffer to decode N bytes of input data.
    /// </summary>
    /// <param name="bytes">Binary data (bytes) count of encoded Base4K chain.</param>
    /// <returns>The size of the buffer (bytes) that is required for decoding.</returns>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static int CalcChainDecodeBufferSize(int bytes)
    {
        var left = bytes;
        int max_blocks_count = left / MAX_DATABLOCK_IN_CHAIN_OUTPUT_BYTES;
        int max_blocks = max_blocks_count * MAX_DATABLOCK_IN_CHAIN;
        left -= max_blocks;
        return max_blocks + (left - 2) /4 *3;
    }
}
