using System.Data.Common;
using System.Runtime.CompilerServices;

class SafeDataReader : IDisposable
{
    private bool _disposed;
    private readonly DbDataReader _reader;

    public DbDataReader Target { get=> _reader; }
    public SafeDataReader(DbDataReader reader)
    {
        _reader = reader;
    }
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Dispose(_disposed);
    }

    private void Dispose(bool disposed)
    {
        if (!disposed)
        {
            _reader?.Close();
            _reader?.Dispose();
            _disposed = true;
        }

    }
}