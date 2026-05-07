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
    c.cliente_id,
    c.ci_nit,
    c.nombres,
    c.primer_apellido,
    c.segundo_apellido,
    c.activo,
    v.vehiculo_id,
    v.placa,
    v.marca,
    v.modelo,
    v.anio,
    v.activo AS vehiculo_activo
FROM clientes c
LEFT JOIN vehiculos v ON c.cliente_id = v.cliente_id
WHERE c.activo = true
    AND (@nombre IS NULL OR c.nombres ILIKE @nombre OR c.ci_nit ILIKE @nombre)
    AND (@placa IS NULL OR v.placa ILIKE @placa)
    AND (@marca IS NULL OR v.marca ILIKE @marca)
ORDER BY c.primer_apellido, c.nombres, v.placa";

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
    s.servicio_id,
    s.nombre,
    COUNT(DISTINCT ot.orden_id) AS cantidad_ordenes,
    COALESCE(SUM(ots.cantidad * ots.precio_unitario), 0) AS total_bs
FROM servicios s
LEFT JOIN ordentrabajoservicio ots ON s.servicio_id = ots.servicio_id
LEFT JOIN ordentrabajo ot ON ots.orden_id = ot.orden_id
    AND ot.fecha_ingreso BETWEEN @desde AND @hasta
WHERE s.activo = true
GROUP BY s.servicio_id, s.nombre
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
