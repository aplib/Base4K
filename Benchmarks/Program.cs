using BenchmarkDotNet.Running;

public static class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<Base4KvsBase64>();
    }
}



