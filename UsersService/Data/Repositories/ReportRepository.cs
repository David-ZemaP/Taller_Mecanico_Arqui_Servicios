using Npgsql;
using Taller_Mecanico_Users.Domain.Common;
using Taller_Mecanico_Users.Domain.Ports;
using Taller_Mecanico_Users.Framework.DTOs.Reports;
using Taller_Mecanico_Users.Framework.Persistence;

namespace Taller_Mecanico_Users.Data.Repositories;

/// <summary>
/// Implementación de IReportRepository con queries SQL nativas (Npgsql)
/// Ejecuta queries para generar reportes de negocio
/// </summary>
public class ReportRepository : IReportRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly ILogger<ReportRepository> _logger;

    public ReportRepository(ISqlConnectionFactory connectionFactory, ILogger<ReportRepository> logger)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Obtiene clientes con vehículos asociados
    /// SQL: LEFT JOIN clientes-vehiculos con filtros opcionales
    /// </summary>
    public async Task<Result<ClientesVehiculosReportDto>> GetClientesVehiculosAsync(
        string? nombreCliente = null,
        string? placaVehiculo = null,
        string? marcaVehiculo = null)
    {
        try
        {
            var connection = _connectionFactory.CreateConnection() as NpgsqlConnection
                ?? throw new InvalidOperationException("Could not cast DbConnection to NpgsqlConnection");

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
    AND (c.nombres ILIKE @nombre OR @nombre IS NULL)
    AND (v.placa ILIKE @placa OR @placa IS NULL)
    AND (v.marca ILIKE @marca OR @marca IS NULL)
ORDER BY c.primer_apellido, c.nombres, v.placa";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@nombre", (object?)(nombreCliente is not null ? $"%{nombreCliente}%" : DBNull.Value) ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@placa", (object?)(placaVehiculo is not null ? $"%{placaVehiculo}%" : DBNull.Value) ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@marca", (object?)(marcaVehiculo is not null ? $"%{marcaVehiculo}%" : DBNull.Value) ?? DBNull.Value);

            using var reader = cmd.ExecuteReader();
            
            var clientes = new Dictionary<int, ClienteReportDto>();

            while (reader.Read())
            {
                var clienteId = reader.GetInt32(0);

                // Crear o recuperar cliente
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

                // Agregar vehículo si existe
                if (!reader.IsDBNull(6))
                {
                    var vehiculo = new VehiculoReportDto
                    {
                        VehiculoId = reader.GetInt32(6),
                        Placa = reader.GetString(7),
                        Marca = reader.GetString(8),
                        Modelo = reader.GetString(9),
                        Anno = reader.GetInt32(10),
                        Activo = reader.GetBoolean(11)
                    };
                    cliente.Vehiculos.Add(vehiculo);
                }
            }

            reader.Close();
            connection.Close();

            var reportData = new ClientesVehiculosReportDto
            {
                Clientes = clientes.Values.ToList(),
                GeneradoEn = DateTime.Now,
                GeneradoPor = "Sistema"
            };

            _logger.LogInformation($"✅ Reporte Clientes-Vehículos generado: {clientes.Count} clientes");
            return Result<ClientesVehiculosReportDto>.Success(reportData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"❌ Error generando Reporte Clientes-Vehículos");
            return Result<ClientesVehiculosReportDto>.Failure("REPORT_ERROR", ex.Message);
        }
    }

    /// <summary>
    /// Obtiene métricas de servicios por rango de fechas
    /// SQL: JOIN ordentrabajo-servicios con agregación (COUNT, SUM)
    /// </summary>
    public async Task<Result<ServiciosOrdenesReportDto>> GetServiciosOrdenesAsync(
        DateTime desde,
        DateTime hasta)
    {
        try
        {
            var connection = _connectionFactory.CreateConnection() as NpgsqlConnection
                ?? throw new InvalidOperationException("Could not cast DbConnection to NpgsqlConnection");

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

            using var reader = cmd.ExecuteReader();

            var servicios = new List<ServicioMetricaDto>();
            decimal totalMonto = 0;
            int totalOrdenes = 0;

            while (reader.Read())
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

            // Calcular porcentajes
            foreach (var servicio in servicios)
            {
                servicio.Porcentaje = totalMonto > 0 ? (servicio.TotalBs / totalMonto) * 100 : 0;
            }

            reader.Close();
            connection.Close();

            var reportData = new ServiciosOrdenesReportDto
            {
                Servicios = servicios,
                TotalBsMonto = totalMonto,
                TotalOrdenes = totalOrdenes,
                GeneradoEn = DateTime.Now,
                GeneradoPor = "Sistema"
            };

            _logger.LogInformation($"✅ Reporte Servicios-Órdenes generado: {servicios.Count} servicios, {totalMonto:C2} Bs");
            return Result<ServiciosOrdenesReportDto>.Success(reportData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"❌ Error generando Reporte Servicios-Órdenes");
            return Result<ServiciosOrdenesReportDto>.Failure("REPORT_ERROR", ex.Message);
        }
    }
}
