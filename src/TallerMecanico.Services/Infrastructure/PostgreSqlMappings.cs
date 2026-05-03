using Npgsql;
using TallerMecanico.Core.Models;

namespace TallerMecanico.Services.Infrastructure;

internal static class PostgreSqlMappings
{
    public static Cliente MapCliente(NpgsqlDataReader reader) => new()
    {
        Id = reader.GetInt32(reader.GetOrdinal("id")),
        Nombre = reader.GetString(reader.GetOrdinal("nombre")),
        Apellido = reader.GetString(reader.GetOrdinal("apellido")),
        Telefono = reader.IsDBNull(reader.GetOrdinal("telefono")) ? null : reader.GetString(reader.GetOrdinal("telefono")),
        Email = reader.IsDBNull(reader.GetOrdinal("email")) ? null : reader.GetString(reader.GetOrdinal("email")),
        IsDeleted = reader.GetBoolean(reader.GetOrdinal("is_deleted")),
        CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
        CreatedByUserId = reader.IsDBNull(reader.GetOrdinal("created_by_user_id")) ? null : reader.GetInt32(reader.GetOrdinal("created_by_user_id")),
        DeletedAt = reader.IsDBNull(reader.GetOrdinal("deleted_at")) ? null : reader.GetDateTime(reader.GetOrdinal("deleted_at")),
        DeletedByUserId = reader.IsDBNull(reader.GetOrdinal("deleted_by_user_id")) ? null : reader.GetInt32(reader.GetOrdinal("deleted_by_user_id"))
    };

    public static Vehiculo MapVehiculo(NpgsqlDataReader reader) => new()
    {
        Id = reader.GetInt32(reader.GetOrdinal("id")),
        ClienteId = reader.GetInt32(reader.GetOrdinal("cliente_id")),
        Marca = reader.GetString(reader.GetOrdinal("marca")),
        Modelo = reader.GetString(reader.GetOrdinal("modelo")),
        Anio = reader.GetInt32(reader.GetOrdinal("anio")),
        Placa = reader.GetString(reader.GetOrdinal("placa")),
        Color = reader.IsDBNull(reader.GetOrdinal("color")) ? null : reader.GetString(reader.GetOrdinal("color")),
        IsDeleted = reader.GetBoolean(reader.GetOrdinal("is_deleted")),
        CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
        CreatedByUserId = reader.IsDBNull(reader.GetOrdinal("created_by_user_id")) ? null : reader.GetInt32(reader.GetOrdinal("created_by_user_id")),
        DeletedAt = reader.IsDBNull(reader.GetOrdinal("deleted_at")) ? null : reader.GetDateTime(reader.GetOrdinal("deleted_at")),
        DeletedByUserId = reader.IsDBNull(reader.GetOrdinal("deleted_by_user_id")) ? null : reader.GetInt32(reader.GetOrdinal("deleted_by_user_id"))
    };

    public static Producto MapProducto(NpgsqlDataReader reader) => new()
    {
        Id = reader.GetInt32(reader.GetOrdinal("id")),
        Nombre = reader.GetString(reader.GetOrdinal("nombre")),
        Descripcion = reader.IsDBNull(reader.GetOrdinal("descripcion")) ? null : reader.GetString(reader.GetOrdinal("descripcion")),
        Precio = reader.GetDecimal(reader.GetOrdinal("precio")),
        Stock = reader.GetInt32(reader.GetOrdinal("stock")),
        IsDeleted = reader.GetBoolean(reader.GetOrdinal("is_deleted"))
    };

    public static OrdenTrabajo MapOrden(NpgsqlDataReader reader) => new()
    {
        Id = reader.GetInt32(reader.GetOrdinal("id")),
        ClienteId = reader.GetInt32(reader.GetOrdinal("cliente_id")),
        VehiculoId = reader.GetInt32(reader.GetOrdinal("vehiculo_id")),
        Descripcion = reader.IsDBNull(reader.GetOrdinal("descripcion")) ? null : reader.GetString(reader.GetOrdinal("descripcion")),
        Estado = (EstadoOrden)reader.GetInt32(reader.GetOrdinal("estado")),
        FechaCreacion = reader.GetDateTime(reader.GetOrdinal("fecha_creacion")),
        FechaCompletado = reader.IsDBNull(reader.GetOrdinal("fecha_completado")) ? null : reader.GetDateTime(reader.GetOrdinal("fecha_completado")),
        FechaAnulacion = reader.IsDBNull(reader.GetOrdinal("fecha_anulacion")) ? null : reader.GetDateTime(reader.GetOrdinal("fecha_anulacion")),
        UsuarioCreacionId = reader.IsDBNull(reader.GetOrdinal("usuario_creacion_id")) ? null : reader.GetInt32(reader.GetOrdinal("usuario_creacion_id")),
        UsuarioAnulacionId = reader.IsDBNull(reader.GetOrdinal("usuario_anulacion_id")) ? null : reader.GetInt32(reader.GetOrdinal("usuario_anulacion_id")),
        IsDeleted = reader.GetBoolean(reader.GetOrdinal("is_deleted"))
    };

    public static DetalleProducto MapDetalleProducto(NpgsqlDataReader reader) => new()
    {
        ProductoId = reader.GetInt32(reader.GetOrdinal("producto_id")),
        NombreProducto = reader.GetString(reader.GetOrdinal("nombre_producto")),
        Cantidad = reader.GetInt32(reader.GetOrdinal("cantidad")),
        PrecioUnitario = reader.GetDecimal(reader.GetOrdinal("precio_unitario"))
    };

    public static Servicio MapDetalleServicio(NpgsqlDataReader reader) => new()
    {
        Id = reader.GetInt32(reader.GetOrdinal("servicio_id")),
        Nombre = reader.GetString(reader.GetOrdinal("nombre_servicio")),
        Descripcion = reader.IsDBNull(reader.GetOrdinal("descripcion")) ? null : reader.GetString(reader.GetOrdinal("descripcion")),
        Precio = reader.GetDecimal(reader.GetOrdinal("precio"))
    };
}