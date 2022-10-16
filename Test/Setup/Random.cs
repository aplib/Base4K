using System;

internal static class Setup
{
    internal static byte[] RandomBuffer(int size, int seed = 0)
    {
        Random rnd = new Random(seed);
        byte[] b = new byte[size];
        rnd.NextBytes(b);
        return b;
    }
}