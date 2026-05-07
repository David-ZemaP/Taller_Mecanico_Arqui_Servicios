namespace Taller_Mecanico_Arqui.Frontend.Enums;

/// <summary>
/// Niveles de acceso para el sistema.
/// </summary>
public enum NivelAcceso
{
    Completo = 3,   // Administrador con acceso total
    Gerente = 2,    // Gerente - acceso a empleados, usuarios, reportes
    Parcial = 1,   // Empleado - acceso limitado
    Cliente = 0    // Cliente - solo puede ver su perfil
}

/// <summary>
/// Estado laboral de un empleado.
/// </summary>
public enum EstadoLaboral
{
    Activo = 0,
    Inactivo = 1,
    Vacaciones = 2,
    Licencia = 3
}