using Npgsql;
using IsolationLevel = System.Data.IsolationLevel;

namespace Cerpent.AWS.DB.Stuff;

public class ConnectionProvider
{
    private NpgsqlConnection? _connection;
    private NpgsqlTransaction? _transaction;
    private bool _disposed = false;

    private readonly string _connectionString;

    public ConnectionProvider(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<NpgsqlConnection?> GetConnection()
    {
        _connection = new NpgsqlConnection(_connectionString);
        await _connection.OpenAsync();
        return _connection;
    }
    public async ValueTask<NpgsqlTransaction?> OpenTransaction()
    {
        if (_connection != null)
            _transaction = await _connection.BeginTransactionAsync(IsolationLevel.ReadCommitted)!;
        return _transaction;
    }
    
    public NpgsqlConnection? CurrentConnection
    {
        get
        {
            if (_connection == null)
            {
                GetConnection();
            }
            return _connection;
        }
    }

    public NpgsqlTransaction? CurrentTransaction
    {
        get
        {
            if (_transaction == null)
            {
                OpenTransaction();
            }
            return _transaction;
        }
    }

    public NpgsqlTransaction? SetTransaction(NpgsqlTransaction? transaction)
    {
        _transaction = transaction;
        return _transaction;
    }

    public NpgsqlConnection? SetConnection(NpgsqlConnection? connection)
    {
        _connection = connection;
        return _connection;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            if (_transaction != null)
            {
                _transaction.Dispose();
                _transaction = null;
            }
            if (_connection != null)
            {
                _connection.Close();
                _connection = null;
            }
        }
        _disposed = true;
    }

    ~ConnectionProvider()
    {
        Dispose(false);
    }
}