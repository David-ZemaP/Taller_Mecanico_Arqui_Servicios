using Microsoft.AspNetCore.Http;
using Taller_Mecanico_Arqui.Domain.Entities;

namespace Taller_Mecanico_Arqui.Infrastructure.Services
{
    public class OrdenTrabajoFotoService
    {
        private const long MaximoTamanoArchivo = 5 * 1024 * 1024;

        private static readonly Dictionary<string, string> ExtensionesPermitidas = new(StringComparer.OrdinalIgnoreCase)
        {
            { ".jpg",  "image/jpeg" },
            { ".jpeg", "image/jpeg" },
            { ".png",  "image/png"  },
            { ".webp", "image/webp" }
        };

        public string? ObtenerErrorValidacion(IFormFile archivo)
        {
            ArgumentNullException.ThrowIfNull(archivo);

            if (archivo.Length <= 0)
                return $"La foto '{archivo.FileName}' está vacía.";

            if (archivo.Length > MaximoTamanoArchivo)
                return $"La foto '{archivo.FileName}' supera el límite de 5 MB.";

            var extension = Path.GetExtension(archivo.FileName);
            if (!ExtensionesPermitidas.ContainsKey(extension))
                return $"La foto '{archivo.FileName}' debe ser JPG, PNG o WEBP.";

            return null;
        }

        public async Task<List<OrdenTrabajoFoto>> GuardarFotosAsync(
            IEnumerable<IFormFile> archivos,
            int ordenTrabajoId,
            CancellationToken cancellationToken = default)
        {
            var fotos = new List<OrdenTrabajoFoto>();

            foreach (var archivo in archivos.Where(a => a.Length > 0))
            {
                var extension = Path.GetExtension(archivo.FileName);
                var contentType = ExtensionesPermitidas.TryGetValue(extension, out var ct)
                    ? ct
                    : "application/octet-stream";

                using var ms = new MemoryStream((int)archivo.Length);
                await archivo.CopyToAsync(ms, cancellationToken);

                fotos.Add(OrdenTrabajoFoto.Crear(
                    ordenTrabajoId,
                    ms.ToArray(),
                    contentType,
                    Path.GetFileName(archivo.FileName)));
            }

            return fotos;
        }
    }
}
