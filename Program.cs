using BenchmarkDotNet.Running;

namespace DataGridViewRowUnsharingBench;

internal class Program
{
    static void Main(string[] args)
    {
        BenchmarkRunner.Run<DataGridViewRowUnsharingBench>();
    }
}
