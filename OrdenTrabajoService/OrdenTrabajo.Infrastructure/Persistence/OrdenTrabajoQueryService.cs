using System.Data;
using Npgsql;
using OrdenTrabajoService.Domain.Entities;
using OrdenTrabajoService.Domain.Enums;

namespace OrdenTrabajoService.Infrastructure.Persistence
{
    public class OrdenTrabajoQueryService
    {
        public List<Vehiculo> LoadVehiculos(NpgsqlConnection connection, NpgsqlTransaction? transaction = null)
        {
            EnsureOpen(connection);

            const string sql = @"
SELECT
    v.vehiculoid, v.clienteid, v.placa, v.marcaid, v.modeloid,
    v.colorvehiculoid, v.anio, v.isdeleted, v.fechaactualizacion,
    TRIM(CONCAT(c.nombre, ' ', c.primerapellido, ' ', COALESCE(c.segundoapellido, ''))) AS cliente_nombre,
    c.ci::text AS cliente_ci,
    m.nombre AS marca_nombre,
    mo.nombre AS modelo_nombre,
    cv.nombre AS color_nombre
FROM vehiculo v
INNER JOIN cliente c ON c.clienteid = v.clienteid
INNER JOIN marca m ON m.marcaid = v.marcaid
INNER JOIN modelo mo ON mo.modeloid = v.modeloid
INNER JOIN colorvehiculo cv ON cv.colorvehiculoid = v.colorvehiculoid
WHERE NOT v.isdeleted AND NOT c.isdeleted
ORDER BY v.placa;";

            var vehiculos = new List<Vehiculo>();
            using var cmd = new NpgsqlCommand(sql, connection, transaction);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var v = Vehiculo.Reconstituir(
                    GetInt32(reader, "vehiculoid"),
                    GetInt32(reader, "clienteid"),
                    GetString(reader, "placa"),
                    GetInt32(reader, "marcaid"),
                    GetInt32(reader, "modeloid"),
                    GetInt32(reader, "colorvehiculoid"),
                    GetInt32(reader, "anio"),
                    GetNullableDateTime(reader, "fechaactualizacion"),
                    GetBoolean(reader, "isdeleted"));

                v.SetDisplayInfo(
                    GetString(reader, "cliente_nombre"),
                    GetString(reader, "cliente_ci"),
                    GetString(reader, "marca_nombre"),
                    GetString(reader, "modelo_nombre"),
                    GetString(reader, "color_nombre"));

                vehiculos.Add(v);
            }
            return vehiculos;
        }

        public List<OrdenTrabajo> LoadOrdenesTrabajo(NpgsqlConnection connection, NpgsqlTransaction? transaction = null)
        {
            EnsureOpen(connection);

            var vehiculos = LoadVehiculos(connection, transaction)
                .ToDictionary(v => v.VehiculoId);

            const string sql = @"
SELECT ordentrabajoid, vehiculoid, fechaingreso, fechaentrega,
       estadotrabajo, estadopago, estadovehiculo, total, isdeleted, fechaactualizacion
FROM ordentrabajo
ORDER BY fechaingreso DESC;";

            var ordenes = new List<OrdenTrabajo>();
            var porId = new Dictionary<int, OrdenTrabajo>();

            using (var cmd = new NpgsqlCommand(sql, connection, transaction))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var vehiculoId = GetInt32(reader, "vehiculoid");
                    vehiculos.TryGetValue(vehiculoId, out var vehiculo);

                    var orden = OrdenTrabajo.Reconstituir(
                        GetInt32(reader, "ordentrabajoid"),
                        vehiculoId,
                        GetDateTime(reader, "fechaingreso"),
                        GetNullableDateTime(reader, "fechaentrega"),
                        Enum.Parse<EstadoTrabajo>(GetString(reader, "estadotrabajo"), ignoreCase: true),
                        Enum.Parse<EstadoPago>(GetString(reader, "estadopago"), ignoreCase: true),
                        GetString(reader, "estadovehiculo"),
                        Convert.ToDouble(GetDecimal(reader, "total")),
                        GetBoolean(reader, "isdeleted"),
                        GetNullableDateTime(reader, "fechaactualizacion"),
                        vehiculo);

                    ordenes.Add(orden);
                    porId[orden.OrdenTrabajoId] = orden;
                }
            }

            if (ordenes.Count == 0) return ordenes;

            LoadProductosOrdenes(connection, transaction, porId);
            LoadServiciosOrdenes(connection, transaction, porId);

            return ordenes;
        }

        private static void LoadProductosOrdenes(
            NpgsqlConnection connection, NpgsqlTransaction? transaction,
            Dictionary<int, OrdenTrabajo> porId)
        {
            const string sql = @"
SELECT otp.ordentrabajoproductoid, otp.ordentrabajoid, otp.productoid,
       otp.cantidad, otp.preciounitario, otp.subtotal, p.nombre AS productonombre
FROM ordentrabajoproducto otp
INNER JOIN producto p ON p.productoid = otp.productoid
ORDER BY otp.ordentrabajoproductoid;";

            using var cmd = new NpgsqlCommand(sql, connection, transaction);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var ordenId = GetInt32(reader, "ordentrabajoid");
                if (porId.TryGetValue(ordenId, out var orden))
                {
                    orden.CargarProducto(OrdenTrabajoProducto.Reconstituir(
                        GetInt32(reader, "ordentrabajoproductoid"),
                        ordenId,
                        GetInt32(reader, "productoid"),
                        GetInt32(reader, "cantidad"),
                        Convert.ToDouble(GetDecimal(reader, "preciounitario")),
                        Convert.ToDouble(GetDecimal(reader, "subtotal")),
                        GetString(reader, "productonombre")));
                }
            }
        }

        private static void LoadServiciosOrdenes(
            NpgsqlConnection connection, NpgsqlTransaction? transaction,
            Dictionary<int, OrdenTrabajo> porId)
        {
            const string sql = @"
SELECT ots.ordentrabajoservicioid, ots.ordentrabajoid, ots.servicioid,
       ots.cantidad, ots.preciounitario, ots.subtotal, s.nombre AS servicionombre
FROM ordentrabajoservicio ots
INNER JOIN servicio s ON s.servicioid = ots.servicioid
ORDER BY ots.ordentrabajoservicioid;";

            using var cmd = new NpgsqlCommand(sql, connection, transaction);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var ordenId = GetInt32(reader, "ordentrabajoid");
                if (porId.TryGetValue(ordenId, out var orden))
                {
                    orden.CargarServicio(OrdenTrabajoServicio.Reconstituir(
                        GetInt32(reader, "ordentrabajoservicioid"),
                        ordenId,
                        GetInt32(reader, "servicioid"),
                        GetInt32(reader, "cantidad"),
                        Convert.ToDouble(GetDecimal(reader, "preciounitario")),
                        Convert.ToDouble(GetDecimal(reader, "subtotal")),
                        GetString(reader, "servicionombre")));
                }
            }
        }

        public List<Producto> LoadProductos(NpgsqlConnection connection, NpgsqlTransaction? transaction = null)
        {
            EnsureOpen(connection);
            const string sql = "SELECT productoid, nombre, precio, stock FROM producto WHERE NOT isdeleted ORDER BY nombre;";
            var lista = new List<Producto>();
            using var cmd = new NpgsqlCommand(sql, connection, transaction);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                lista.Add(Producto.Reconstituir(
                    GetInt32(reader, "productoid"),
                    GetString(reader, "nombre"),
                    Convert.ToDouble(GetDecimal(reader, "precio")),
                    GetInt32(reader, "stock")));
            return lista;
        }

        public List<Servicio> LoadServicios(NpgsqlConnection connection, NpgsqlTransaction? transaction = null)
        {
            EnsureOpen(connection);
            const string sql = "SELECT servicioid, nombre, precio FROM servicio WHERE NOT isdeleted ORDER BY nombre;";
            var lista = new List<Servicio>();
            using var cmd = new NpgsqlCommand(sql, connection, transaction);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                lista.Add(Servicio.Reconstituir(
                    GetInt32(reader, "servicioid"),
                    GetString(reader, "nombre"),
                    Convert.ToDouble(GetDecimal(reader, "precio"))));
            return lista;
        }

        public List<Marca> LoadMarcas(NpgsqlConnection connection, NpgsqlTransaction? transaction = null)
        {
            EnsureOpen(connection);
            const string sql = "SELECT marcaid, nombre FROM marca ORDER BY nombre;";
            var lista = new List<Marca>();
            using var cmd = new NpgsqlCommand(sql, connection, transaction);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                lista.Add(Marca.Reconstituir(GetInt32(reader, "marcaid"), GetString(reader, "nombre")));
            return lista;
        }

        public List<Modelo> LoadModelos(NpgsqlConnection connection, NpgsqlTransaction? transaction = null)
        {
            EnsureOpen(connection);
            const string sql = "SELECT modeloid, marcaid, nombre FROM modelo ORDER BY nombre;";
            var lista = new List<Modelo>();
            using var cmd = new NpgsqlCommand(sql, connection, transaction);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                lista.Add(Modelo.Reconstituir(GetInt32(reader, "modeloid"), GetInt32(reader, "marcaid"), GetString(reader, "nombre")));
            return lista;
        }

        public List<ColorVehiculo> LoadColores(NpgsqlConnection connection, NpgsqlTransaction? transaction = null)
        {
            EnsureOpen(connection);
            const string sql = "SELECT colorvehiculoid, nombre FROM colorvehiculo ORDER BY nombre;";
            var lista = new List<ColorVehiculo>();
            using var cmd = new NpgsqlCommand(sql, connection, transaction);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                lista.Add(ColorVehiculo.Reconstituir(GetInt32(reader, "colorvehiculoid"), GetString(reader, "nombre")));
            return lista;
        }

        public List<OrdenTrabajoCatalogo> LoadOrdenTrabajoCatalogos(NpgsqlConnection connection, NpgsqlTransaction? transaction = null)
        {
            EnsureOpen(connection);
            const string sql = "SELECT ordentrabajocatalogoid, ordentrabajoid, productoid, cantidadutilizada, preciounitario FROM ordentrabajocatalogo ORDER BY ordentrabajoid;";
            var lista = new List<OrdenTrabajoCatalogo>();
            using var cmd = new NpgsqlCommand(sql, connection, transaction);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                lista.Add(OrdenTrabajoCatalogo.Crear(
                    GetInt32(reader, "ordentrabajoid"),
                    GetInt32(reader, "productoid"),
                    GetInt32(reader, "cantidadutilizada"),
                    GetDecimal(reader, "preciounitario")));
            return lista;
        }

        private static void EnsureOpen(NpgsqlConnection connection)
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
        }

        private static int GetInt32(NpgsqlDataReader r, string col) => r.GetInt32(r.GetOrdinal(col));
        private static string GetString(NpgsqlDataReader r, string col) => r.GetString(r.GetOrdinal(col));
        private static bool GetBoolean(NpgsqlDataReader r, string col) => r.GetBoolean(r.GetOrdinal(col));
        private static decimal GetDecimal(NpgsqlDataReader r, string col) => r.GetDecimal(r.GetOrdinal(col));
        private static DateTime GetDateTime(NpgsqlDataReader r, string col) => r.GetDateTime(r.GetOrdinal(col));
        private static DateTime? GetNullableDateTime(NpgsqlDataReader r, string col)
        {
            var ord = r.GetOrdinal(col);
            return r.IsDBNull(ord) ? null : r.GetDateTime(ord);
        }
    }
}

