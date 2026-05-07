using Npgsql;

var connStr = "Host=127.0.0.1;Port=5433;Database=TallerMecanico;Username=admin;Password=tallermecanico2026";
using var conn = new NpgsqlConnection(connStr);

try
{
    await conn.OpenAsync();
    Console.WriteLine("Conexión abierta");

    using var cmd = new NpgsqlCommand(@"
            DROP TABLE IF EXISTS audit_logs;
        CREATE TABLE IF NOT EXISTS audit_logs (
            id SERIAL PRIMARY KEY,
            tabla_afectada VARCHAR(255),
            registro_id INT,
            accion VARCHAR(100),
            realizado_por VARCHAR(255),
            fecha_hora TIMESTAMP DEFAULT CURRENT_TIMESTAMP
        );
    ", conn);

    await cmd.ExecuteNonQueryAsync();
    Console.WriteLine("Tabla audit_logs creada exitosamente");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
