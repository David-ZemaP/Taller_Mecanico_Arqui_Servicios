using TallerMecanico.Core.Contracts;
using TallerMecanico.Services.Infrastructure.Repositories;

namespace TallerMecanico.Services.Infrastructure;

public sealed class PostgreSqlRepositoryCreator : RepositoryCreator
{
    private readonly string? _connectionString;
    private readonly InMemoryRepositoryCreator _fallback = new();
    private PostgreSqlDatabase? _database;

    public PostgreSqlRepositoryCreator(string? connectionString)
    {
        _connectionString = string.IsNullOrWhiteSpace(connectionString) ? null : connectionString;
    }

    public override IClienteRepository CreateClienteRepository() => Resolve(() => new PostgreSqlClienteRepository(Database), () => _fallback.CreateClienteRepository());

    public override IVehiculoRepository CreateVehiculoRepository() => Resolve(() => new PostgreSqlVehiculoRepository(Database), () => _fallback.CreateVehiculoRepository());

    public override IProductoRepository CreateProductoRepository() => Resolve(() => new PostgreSqlProductoRepository(Database), () => _fallback.CreateProductoRepository());

    public override IOrdenTrabajoRepository CreateOrdenTrabajoRepository() => Resolve(() => new PostgreSqlOrdenTrabajoRepository(Database), () => _fallback.CreateOrdenTrabajoRepository());

    private PostgreSqlDatabase Database => _database ??= new PostgreSqlDatabase(_connectionString!);

    private bool HasConnectionString => !string.IsNullOrWhiteSpace(_connectionString);

    private T Resolve<T>(Func<T> postgresFactory, Func<T> fallbackFactory)
    {
        if (!HasConnectionString)
        {
            return fallbackFactory();
        }

        try
        {
            return postgresFactory();
        }
        catch
        {
            return fallbackFactory();
        }
    }
}