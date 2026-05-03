using Npgsql;

namespace TallerMecanico.Services.Infrastructure;

internal sealed class PostgreSqlDatabase
{
    private readonly string _connectionString;
    private readonly object _schemaLock = new();
    private bool _schemaCreated;

    public PostgreSqlDatabase(string connectionString)
    {
        _connectionString = connectionString;
    }

    public NpgsqlConnection OpenConnection() => new(_connectionString);

    public void EnsureSchema()
    {
        if (_schemaCreated)
        {
            return;
        }

        lock (_schemaLock)
        {
            if (_schemaCreated)
            {
                return;
            }

            using var connection = OpenConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
CREATE TABLE IF NOT EXISTS clientes (
    id SERIAL PRIMARY KEY,
    nombre TEXT NOT NULL,
    apellido TEXT NOT NULL,
    telefono TEXT NULL,
    email TEXT NULL,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by_user_id INT NULL,
    deleted_at TIMESTAMPTZ NULL,
    deleted_by_user_id INT NULL
);

CREATE TABLE IF NOT EXISTS vehiculos (
    id SERIAL PRIMARY KEY,
    cliente_id INT NOT NULL,
    marca TEXT NOT NULL,
    modelo TEXT NOT NULL,
    anio INT NOT NULL,
    placa TEXT NOT NULL,
    color TEXT NULL,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by_user_id INT NULL,
    deleted_at TIMESTAMPTZ NULL,
    deleted_by_user_id INT NULL
);

CREATE TABLE IF NOT EXISTS productos (
    id SERIAL PRIMARY KEY,
    nombre TEXT NOT NULL,
    descripcion TEXT NULL,
    precio NUMERIC(18,2) NOT NULL,
    stock INT NOT NULL DEFAULT 0,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE TABLE IF NOT EXISTS ordenes_trabajo (
    id SERIAL PRIMARY KEY,
    cliente_id INT NOT NULL,
    vehiculo_id INT NOT NULL,
    descripcion TEXT NULL,
    estado INT NOT NULL,
    fecha_creacion TIMESTAMPTZ NOT NULL,
    fecha_completado TIMESTAMPTZ NULL,
    fecha_anulacion TIMESTAMPTZ NULL,
    usuario_creacion_id INT NULL,
    usuario_anulacion_id INT NULL,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE TABLE IF NOT EXISTS ordenes_trabajo_productos (
    orden_trabajo_id INT NOT NULL,
    producto_id INT NOT NULL,
    nombre_producto TEXT NOT NULL,
    cantidad INT NOT NULL,
    precio_unitario NUMERIC(18,2) NOT NULL,
    PRIMARY KEY (orden_trabajo_id, producto_id, nombre_producto)
);

CREATE TABLE IF NOT EXISTS ordenes_trabajo_servicios (
    orden_trabajo_id INT NOT NULL,
    servicio_id INT NOT NULL,
    nombre_servicio TEXT NOT NULL,
    descripcion TEXT NULL,
    precio NUMERIC(18,2) NOT NULL,
    PRIMARY KEY (orden_trabajo_id, servicio_id, nombre_servicio)
);
";
            command.ExecuteNonQuery();

            _schemaCreated = true;
        }
    }
}