using Npgsql;
using Taller_Mecanico_Users.Domain.Common;
using Taller_Mecanico_Users.Domain.Ports;
using Taller_Mecanico_Users.Framework.DTOs.Reports;
using Taller_Mecanico_Users.Framework.Persistence;

namespace Taller_Mecanico_Users.Data.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly ILogger<ReportRepository> _logger;

    public ReportRepository(ISqlConnectionFactory connectionFactory, ILogger<ReportRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<Result<ClientesVehiculosReportDto>> GetClientesVehiculosAsync(
        string? nombreCliente = null,
        string? placaVehiculo = null,
        string? marcaVehiculo = null)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection() as NpgsqlConnection
                ?? throw new InvalidOperationException("No se pudo obtener NpgsqlConnection");

            await connection.OpenAsync();

            var sql = @"
SELECT
    c.clienteid,
    c.ci::text AS ci_nit,
    c.nombre AS nombres,
    c.primerapellido AS primer_apellido,
    c.segundoapellido AS segundo_apellido,
    NOT c.isdeleted AS activo,
    v.vehiculoid,
    v.placa,
    ma.nombre AS marca,
    mo.nombre AS modelo,
    v.anio,
    NOT v.isdeleted AS vehiculo_activo
FROM cliente c
LEFT JOIN vehiculo v ON c.clienteid = v.clienteid
LEFT JOIN marca ma ON v.marcaid = ma.marcaid
LEFT JOIN modelo mo ON v.modeloid = mo.modeloid
WHERE c.isdeleted = false
    AND (@nombre IS NULL OR c.nombre ILIKE @nombre OR c.ci::text ILIKE @nombre)
    AND (@placa IS NULL OR v.placa ILIKE @placa)
    AND (@marca IS NULL OR ma.nombre ILIKE @marca)
ORDER BY c.primerapellido, c.nombre, v.placa";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@nombre", (object?)(nombreCliente != null ? $"%{nombreCliente}%" : null) ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@placa", (object?)(placaVehiculo != null ? $"%{placaVehiculo}%" : null) ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@marca", (object?)(marcaVehiculo != null ? $"%{marcaVehiculo}%" : null) ?? DBNull.Value);

            using var reader = await cmd.ExecuteReaderAsync();

            var clientes = new Dictionary<int, ClienteReportDto>();

            while (await reader.ReadAsync())
            {
                var clienteId = reader.GetInt32(0);

                if (!clientes.TryGetValue(clienteId, out var cliente))
                {
                    cliente = new ClienteReportDto
                    {
                        ClienteId = clienteId,
                        CiNit = reader.GetString(1),
                        Nombres = reader.GetString(2),
                        PrimerApellido = reader.GetString(3),
                        SegundoApellido = reader.IsDBNull(4) ? null : reader.GetString(4),
                        Activo = reader.GetBoolean(5)
                    };
                    clientes[clienteId] = cliente;
                }

                if (!reader.IsDBNull(6))
                {
                    cliente.Vehiculos.Add(new VehiculoReportDto
                    {
                        VehiculoId = reader.GetInt32(6),
                        Placa = reader.GetString(7),
                        Marca = reader.GetString(8),
                        Modelo = reader.GetString(9),
                        Anno = reader.GetInt32(10),
                        Activo = reader.GetBoolean(11)
                    });
                }
            }

            _logger.LogInformation("Reporte Clientes-Vehículos: {Count} clientes", clientes.Count);
            return Result<ClientesVehiculosReportDto>.Success(new ClientesVehiculosReportDto
            {
                Clientes = clientes.Values.ToList(),
                GeneradoEn = DateTime.Now,
                GeneradoPor = "Sistema"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetClientesVehiculosAsync");
            return Result<ClientesVehiculosReportDto>.Failure("REPORT_ERROR", ex.Message);
        }
    }

    public async Task<Result<ServiciosOrdenesReportDto>> GetServiciosOrdenesAsync(DateTime desde, DateTime hasta)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection() as NpgsqlConnection
                ?? throw new InvalidOperationException("No se pudo obtener NpgsqlConnection");

            await connection.OpenAsync();

            var sql = @"
SELECT
    s.servicioid,
    s.nombre,
    COUNT(DISTINCT ot.ordentrabajoid) AS cantidad_ordenes,
    COALESCE(SUM(ots.cantidad * ots.preciounitario), 0) AS total_bs
FROM servicio s
LEFT JOIN ordentrabajoservicio ots ON s.servicioid = ots.servicioid
LEFT JOIN ordentrabajo ot ON ots.ordentrabajoid = ot.ordentrabajoid
    AND ot.fechaingreso BETWEEN @desde AND @hasta
WHERE s.isdeleted = false
GROUP BY s.servicioid, s.nombre
ORDER BY total_bs DESC";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@desde", desde);
            cmd.Parameters.AddWithValue("@hasta", hasta);

            using var reader = await cmd.ExecuteReaderAsync();

            var servicios = new List<ServicioMetricaDto>();
            decimal totalMonto = 0;
            int totalOrdenes = 0;

            while (await reader.ReadAsync())
            {
                var cantidadOrdenes = reader.GetInt32(2);
                var totalBs = reader.GetDecimal(3);

                servicios.Add(new ServicioMetricaDto
                {
                    ServicioId = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    CantidadOrdenes = cantidadOrdenes,
                    TotalBs = totalBs
                });

                totalMonto += totalBs;
                totalOrdenes += cantidadOrdenes;
            }

            foreach (var s in servicios)
                s.Porcentaje = totalMonto > 0 ? Math.Round((s.TotalBs / totalMonto) * 100, 2) : 0;

            _logger.LogInformation("Reporte Servicios-Órdenes: {Count} servicios, total {Monto} Bs", servicios.Count, totalMonto);
            return Result<ServiciosOrdenesReportDto>.Success(new ServiciosOrdenesReportDto
            {
                Servicios = servicios,
                TotalBsMonto = totalMonto,
                TotalOrdenes = totalOrdenes,
                GeneradoEn = DateTime.Now,
                GeneradoPor = "Sistema"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetServiciosOrdenesAsync");
            return Result<ServiciosOrdenesReportDto>.Failure("REPORT_ERROR", ex.Message);
        }
    }
}
