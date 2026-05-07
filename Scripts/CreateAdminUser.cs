using Npgsql;
using BC = BCrypt.Net.BCrypt;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Crear Usuario Administrador ===\n");
        
        var connectionString = "Host=127.0.0.1;Port=5432;Database=taller_mecanico;Username=postgres;Password=postgres";
        
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        
        // Check if user already exists
        await using var checkCmd = new NpgsqlCommand(
            "SELECT UsuarioLoginId FROM UsuarioLogin WHERE Email = @email", connection);
        checkCmd.Parameters.AddWithValue("email", "administrador.principal@taller.com");
        
        var existingId = await checkCmd.ExecuteScalarAsync();
        if (existingId != null)
        {
            Console.WriteLine("El usuario administrador.principal@taller.com ya existe.");
            return;
        }
        
        // Check if there's an admin empleado
        int? empleadoId = null;
        await using var checkEmpCmd = new NpgsqlCommand(
            "SELECT EmpleadoId FROM Empleado WHERE email = @email OR tipoempleado = 'Gerente' LIMIT 1", connection);
        checkEmpCmd.Parameters.AddWithValue("email", "administrador.principal@taller.com");
        
        var empResult = await checkEmpCmd.ExecuteScalarAsync();
        if (empResult != null)
        {
            empleadoId = (int?)empResult;
        }
        
        // If no empleado exists, create one
        if (empleadoId == null)
        {
            Console.WriteLine("Creando empleado administrador...");
            await using var insertEmpCmd = new NpgsqlCommand(@"
                INSERT INTO Empleado (Nombre, PrimerApellido, Ci, Telefono, Email, TipoEmpleado, EstadoLaboral)
                VALUES (@nombre, @apellido, @ci, @telefono, @email, @tipo, 'Activo')
                RETURNING EmpleadoId", connection);
            
            insertEmpCmd.Parameters.AddWithValue("nombre", "Administrador");
            insertEmpCmd.Parameters.AddWithValue("apellido", "Principal");
            insertEmpCmd.Parameters.AddWithValue("ci", 100000);
            insertEmpCmd.Parameters.AddWithValue("telefono", 70000000);
            insertEmpCmd.Parameters.AddWithValue("email", "administrador.principal@taller.com");
            insertEmpCmd.Parameters.AddWithValue("tipo", "Gerente");
            
            empleadoId = (int?)await insertEmpCmd.ExecuteScalarAsync();
            Console.WriteLine($"Empleado creado con ID: {empleadoId}");
        }
        
        // Hash password with BCrypt cost 12
        string passwordHash = BC.HashPassword("ap100000", BC.GenerateSalt(12));
        Console.WriteLine($"Hash generado: {passwordHash}");
        
        // Create the admin user
        await using var insertCmd = new NpgsqlCommand(@"
            INSERT INTO UsuarioLogin (EmpleadoId, Email, PasswordHash, Activo, RequiereCambioPassword, EsCliente)
            VALUES (@empleadoId, @email, @passwordHash, true, false, false)
            RETURNING UsuarioLoginId", connection);
        
        insertCmd.Parameters.AddWithValue("empleadoId", empleadoId!);
        insertCmd.Parameters.AddWithValue("email", "administrador.principal@taller.com");
        insertCmd.Parameters.AddWithValue("passwordHash", passwordHash);
        
        var userId = (int?)await insertCmd.ExecuteScalarAsync();
        
        Console.WriteLine($"\n✅ Usuario creado exitosamente!");
        Console.WriteLine($"   ID: {userId}");
        Console.WriteLine($"   Email: administrador.principal@taller.com");
        Console.WriteLine($"   Password: ap100000");
        Console.WriteLine($"   RequiereCambioPassword: false");
    }
}
