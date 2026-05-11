using WebService.Models;

namespace WebService.Mappers
{
    /// <summary>
    /// Mapper para convertir entre Roles de BD y NivelAcceso del frontend.
    /// 
    /// Mapeo BD -> Frontend:
    /// - "Gerente" -> NivelAcceso.Gerente
    /// - "Administrador" -> NivelAcceso.Completo
    /// - "Mecanico" -> NivelAcceso.Parcial
    /// - "Cliente" -> NivelAcceso.Cliente
    /// 
    /// Mapeo Frontend -> BD:
    /// - NivelAcceso.Gerente -> "Gerente"
    /// - NivelAcceso.Completo -> "Administrador"
    /// - NivelAcceso.Parcial -> "Mecanico"
    /// - NivelAcceso.Cliente -> "Cliente"
    /// </summary>
    public static class RoleMapper
    {
        /// <summary>
        /// Convierte el nombre del rol de la BD al enum NivelAcceso del frontend.
        /// </summary>
        public static NivelAcceso ToNivelAcceso(string? rolNombre)
        {
            if (string.IsNullOrEmpty(rolNombre))
                return NivelAcceso.Parcial;

            return rolNombre switch
            {
                "Gerente" => NivelAcceso.Gerente,
                "Administrador" => NivelAcceso.Completo,
                "Mecanico" => NivelAcceso.Parcial,
                "Cliente" => NivelAcceso.Cliente,
                _ => NivelAcceso.Parcial
            };
        }

        /// <summary>
        /// Convierte el enum NivelAcceso del frontend al nombre del rol en la BD.
        /// </summary>
        public static string ToRolNombre(NivelAcceso nivelAcceso)
        {
            return nivelAcceso switch
            {
                NivelAcceso.Gerente => "Gerente",
                NivelAcceso.Completo => "Administrador",
                NivelAcceso.Parcial => "Mecanico",
                NivelAcceso.Cliente => "Cliente",
                _ => "Mecanico"
            };
        }

        /// <summary>
        /// Obtiene el texto a mostrar según el nivel de acceso.
        /// </summary>
        public static string GetDisplayName(NivelAcceso nivelAcceso)
        {
            return nivelAcceso switch
            {
                NivelAcceso.Gerente => "Gerente",
                NivelAcceso.Completo => "Administrador",
                NivelAcceso.Parcial => "Mecánico",
                NivelAcceso.Cliente => "Cliente",
                _ => "Desconocido"
            };
        }
    }
}