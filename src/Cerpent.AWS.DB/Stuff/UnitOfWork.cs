using Npgsql;

namespace Cerpent.AWS.DB.Repositories.Stuff;

public class UnitOfWork : IDisposable
{
    protected ConnectionProvider _connectionProvider;
    protected bool _disposed;

    NpgsqlConnection? _connection = null;
    NpgsqlTransaction? _transaction = null;

    public UnitOfWork(ConnectionProvider connectionProvider)
    {
        _connectionProvider = connectionProvider;
    }

    public async Task Begin()
    {
        _connection ??= await _connectionProvider?.GetConnection()!;

        _connectionProvider.SetConnection(_connection);

        _transaction ??= await _connectionProvider.OpenTransaction();
        
        _connectionProvider.SetTransaction(_transaction);
    }

    public void Commit()
    {
        try
        {
            _connectionProvider.CurrentTransaction?.Commit();
        }
        catch
        {
            _connectionProvider.CurrentTransaction?.Rollback();
            throw;
        }
        finally
        {
            _connectionProvider.CurrentTransaction?.Dispose();
            _transaction = null;
        }
    }

    private void dispose(bool disposing)
    {
        if (_disposed)
            return;
        
        if (disposing)
        {
            if (_connectionProvider != null)
            {
                _connectionProvider.Dispose();
                _connectionProvider = null;
            }
        }

        _disposed = true;
    }

    ~UnitOfWork() => dispose(false);
    
    public void Dispose()
    {
        dispose(true);
        GC.SuppressFinalize(this);
    }
}