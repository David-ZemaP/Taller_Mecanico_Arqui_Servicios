using Microsoft.AspNetCore.Mvc;
using Npgsql;
using OrdenTrabajoService.Infrastructure.Persistence;

namespace OrdenTrabajoService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CatalogosVehiculosController : ControllerBase
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public CatalogosVehiculosController(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    [HttpGet("marcas")]
    public async Task<IActionResult> GetMarcas()
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
SELECT marcaid AS Id, nombre
FROM marca
ORDER BY nombre;";

        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        var result = new List<CatalogoMarcaDto>();
        while (await reader.ReadAsync())
        {
            result.Add(new CatalogoMarcaDto
            {
                Id = reader.GetInt32(0),
                Nombre = reader.GetString(1)
            });
        }

        return Ok(result);
    }

    [HttpGet("modelos")]
    public async Task<IActionResult> GetModelos()
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
SELECT mo.modeloid AS Id, mo.marcaid AS MarcaId, m.nombre AS MarcaNombre, mo.nombre
FROM modelo mo
INNER JOIN marca m ON m.marcaid = mo.marcaid
ORDER BY m.nombre, mo.nombre;";

        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        var result = new List<CatalogoModeloDto>();
        while (await reader.ReadAsync())
        {
            result.Add(new CatalogoModeloDto
            {
                Id = reader.GetInt32(0),
                MarcaId = reader.GetInt32(1),
                MarcaNombre = reader.GetString(2),
                Nombre = reader.GetString(3)
            });
        }

        return Ok(result);
    }

    [HttpGet("colores")]
    public async Task<IActionResult> GetColores()
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        const string sql = @"
SELECT colorvehiculoid AS Id, nombre
FROM colorvehiculo
ORDER BY nombre;";

        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        var result = new List<CatalogoColorDto>();
        while (await reader.ReadAsync())
        {
            result.Add(new CatalogoColorDto
            {
                Id = reader.GetInt32(0),
                Nombre = reader.GetString(1)
            });
        }

        return Ok(result);
    }
}

public class CatalogoMarcaDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

public class CatalogoModeloDto
{
    public int Id { get; set; }
    public int MarcaId { get; set; }
    public string MarcaNombre { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
}

public class CatalogoColorDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}
