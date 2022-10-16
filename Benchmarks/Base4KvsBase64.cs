using BenchmarkDotNet.Attributes;
using Lex4K;
using System.Buffers.Text;

[MemoryDiagnoser(true)]
public class Base4KvsBase64
{
    [Params(110, 1000, 50000)]
    public int N { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public byte[] data;
    public byte[] decoded;

    public byte[] Base4K_Encoded_data;
    public int Base4K_Encoded_data_size;

    public byte[] Base64_Encoded_data;
    public int Base64_Encoded_data_size;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    [GlobalSetup]
    public void Setup()
    {
        var rnd = new Random((int)DateTime.Now.Ticks);
        //Console.WriteLine("Buffersize: " + buffersize);
        using var mem = new MemoryStream(N);
        for (int i = 0; i < N; i++)
        {
            byte x = (byte)(0xff & (i ^ (int)DateTime.Now.Ticks ^ rnd.Next(65535)));
            mem.WriteByte(x);
        }
        var data = mem.ToArray();

        Base4K_Encoded_data_size = Lex4K.Base4K.CalcBlockEncodeOutput(N);
        Base4K_Encoded_data = new byte[Base4K_Encoded_data_size];
        Base4K.EncodeBlock(data, Base4K_Encoded_data);

        Base64_Encoded_data_size = Base64.GetMaxEncodedToUtf8Length(N);
        Base64_Encoded_data = new byte[Base64_Encoded_data_size];
        Base64.EncodeToUtf8(data, Base64_Encoded_data, out var consumed, out var final_block);

        decoded = new byte[N];
    }

    [Benchmark]
    public void Base4K_EncodeBlock()
    {
        Base4K.EncodeBlock(data, Base4K_Encoded_data);
    }

    [Benchmark]
    public void Base64_EncodeToUtf8()
    {
        Base64.EncodeToUtf8(data, Base64_Encoded_data, out var consumed, out var final_block);
    }

    [Benchmark]
    public void Base4K_Decode()
    {
        Base4K.DecodeBlock(N, Base4K_Encoded_data, decoded);
    }

    [Benchmark]
    public void Base64_Decode()
    {
        Base64.DecodeFromUtf8(Base64_Encoded_data, decoded, out var consumed, out var written);
    }
}