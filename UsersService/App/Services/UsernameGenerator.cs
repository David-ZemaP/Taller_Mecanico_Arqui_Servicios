using System.Globalization;
using System.Text;
using Taller_Mecanico_Users.Domain.Ports;

namespace Taller_Mecanico_Users.App.Services;

/// <summary>
/// Servicio para generar usernames automáticamente a partir de nombres y apellidos.
/// Patrón: inicial_nombre + primer_apellido, todo lowercase, sin tildes
/// Resolución de duplicados: si existe, agrega número (jperez2, jperez3, etc)
/// </summary>
public class UsernameGenerator
{
    private readonly IUsuarioLoginRepository _usuarioRepository;

    public UsernameGenerator(IUsuarioLoginRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    /// <summary>
    /// Genera un username único basado en nombres y apellidos.
    /// </summary>
    public async Task<string> GenerateAsync(string nombres, string primerApellido)
    {
        // Validación
        if (string.IsNullOrWhiteSpace(nombres) || string.IsNullOrWhiteSpace(primerApellido))
            throw new ArgumentException("Nombres y apellido son requeridos");

        // Extraer inicial del nombre y apellido completo
        var inicial = RemoveDiacritics(nombres.Trim()[0].ToString().ToLower());
        var apellido = RemoveDiacritics(primerApellido.Trim().ToLower());

        var baseUsername = $"{inicial}{apellido}";

        // Verificar unicidad
        var username = baseUsername;
        var counter = 2;

        while (await _usuarioRepository.GetByUsernameAsync(username) != null)
        {
            username = $"{baseUsername}{counter}";
            counter++;
        }

        return username;
    }

    /// <summary>
    /// Remueve diacríticos (tildes, ñ, etc) de un string.
    /// Ej: "Pérez" → "Perez", "José" → "Jose"
    /// </summary>
    private static string RemoveDiacritics(string text)
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new System.Text.StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }
}
