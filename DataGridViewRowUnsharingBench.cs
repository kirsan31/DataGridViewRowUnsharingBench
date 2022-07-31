using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;

namespace DataGridViewRowUnsharingBench;

[Config(typeof(Config))]
public class DataGridViewRowUnsharingBench : IDisposable
{
    private readonly DataGridView _dataGridView = new DataGridView();
    private volatile bool _isDisposed;

    public DataGridViewRowUnsharingBench()
    {
        _dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _dataGridView.AllowUserToAddRows = false;
        _dataGridView.Columns.Add("c1", "c1");
        _dataGridView.Columns.Add("c2", "c2");
        _dataGridView.Columns.Add("c3", "c3");
        _dataGridView.Columns.Add("c4", "c4");
        _dataGridView.CreateControl();
    }

    [Params(1000, 10_000, 20_000)]
    public int RowsCount { get; set; }

    [IterationSetup(Target = nameof(EnumAll_1Uniq))]
    public void AssignData()
    {
        // All rows are shared with one shared row.
        _dataGridView.Rows.Add(RowsCount);
    }

    [IterationSetup(Target = nameof(EnumAll_2Uniq))]
    public void AssignData2()
    {
        // First halve of rows are shared with one shared row and second halve with another shared row.
        _dataGridView.Rows.Add(RowsCount / 2);
        _dataGridView.Rows.Add(RowsCount / 2);      
    }

    [IterationSetup(Target = nameof(EnumAll_HalfUniq))]
    public void AssignData4()
    {
        // Every 2 rows are shared with one individual shared row.
        for (int i = 0; i < RowsCount; i++)
        {
            if (i % 2 == 0)
                _dataGridView.Rows.Add(2);
        }
    }

    [IterationSetup(Target = nameof(EnumAll_AllUniq))]
    public void AssignData3()
    {
        // Every row is individual shared row.
        for (int i = 0; i < RowsCount; i++)
            _dataGridView.Rows.Add(new DataGridViewRow());
    }

    [IterationCleanup]
    public void ClearData()
    {
        _dataGridView.Rows.Clear();
    }

    [Benchmark]
    public long EnumAll_1Uniq()
    {
        return EnumAllRowsPrivate();
    }

    [Benchmark]
    public long EnumAll_2Uniq()
    {
        return EnumAllRowsPrivate();
    }

    [Benchmark]
    public long EnumAll_HalfUniq()
    {
        return EnumAllRowsPrivate();
    }

    [Benchmark]
    public long EnumAll_AllUniq()
    {
        return EnumAllRowsPrivate();
    }

    private long EnumAllRowsPrivate()
    {
        long res = 0;
        foreach (DataGridViewRow r in _dataGridView.Rows)
            res += r.Index;

        return res;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
                _dataGridView.Dispose();

            _isDisposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private class Config : ManualConfig
    {
        public Config()
        {
            AddDiagnoser(MemoryDiagnoser.Default);
            AddJob(Job.InProcess.WithId("New"));
            AddJob(Job.Default.WithId("Original"));
        }
    }
}
