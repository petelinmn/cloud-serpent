using Npgsql;

namespace Cerpent.AWS.DB.Repositories.Stuff;

public abstract class BaseRepository
{
    protected BaseRepository(string connectionString)
    {
        _connectionProvider = new ConnectionProvider(connectionString);
    }

    public async Task<UnitOfWork> GetUow()
    {
        var uow = new UnitOfWork(_connectionProvider);
        await uow.Begin();
        return uow;
    }

    public async Task<TResponse> UsingUow<TResponse>(Func<Task<TResponse>> func)
    {
        var uow = await GetUow();
        var result = await func();
        uow.Commit();
        return result;
    }
    
    protected NpgsqlTransaction? Transaction => _connectionProvider.CurrentTransaction;

    protected NpgsqlConnection? Connection => _connectionProvider.CurrentTransaction?.Connection;

    private readonly ConnectionProvider _connectionProvider;
}
