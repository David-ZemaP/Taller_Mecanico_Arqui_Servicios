using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Taller_Mecanico_Arqui.Frontend.Adapters;
using Taller_Mecanico_Arqui.Frontend.Authorization;
using Taller_Mecanico_Arqui.Frontend.DTOs;

namespace Taller_Mecanico_Arqui.Pages.Clientes
{
    [RequireAccessLevel(NivelAcceso.Parcial)]
    public class IndexModel : PageModel
    {
        private readonly IClienteAdapter _clienteAdapter;

        public IndexModel(IClienteAdapter clienteAdapter)
        {
            _clienteAdapter = clienteAdapter;
        }

        public IList<ClienteListDto> Clientes { get; set; } = new List<ClienteListDto>();

        [BindProperty]
        public ClienteFormDto FormDto { get; set; } = new();

        public async Task OnGetAsync()
        {
            Clientes = await _clienteAdapter.GetAllAsync();
        }

        public async Task<JsonResult> OnGetClienteAsync(int id)
        {
            var cliente = await _clienteAdapter.GetByIdAsync(id);
            if (cliente == null)
            {
                return new JsonResult(new { error = "Cliente no encontrado." }) { StatusCode = 404 };
            }

            return new JsonResult(new
            {
                clienteId = cliente.ClienteId,
                nombres = cliente.Nombres,
                primerApellido = cliente.PrimerApellido,
                segundoApellido = cliente.SegundoApellido,
                ciNumero = int.TryParse(cliente.Ci, out var ci) ? ci : 0,
                telefono = cliente.Telefono,
                email = cliente.Email
            });
        }

        public async Task<IActionResult> OnPostSaveAjaxAsync([FromBody] ClienteFormDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values
                                    .SelectMany(v => v.Errors)
                                    .Select(e => e.ErrorMessage));
                return new JsonResult(new { success = false, message = "Datos inválidos: " + errors }) { StatusCode = 400 };
            }

            bool success;
            string? error = null;

            if (dto.ClienteId == 0)
            {
                var createDto = new CreateClienteDto
                {
                    Nombres = dto.Nombres,
                    PrimerApellido = dto.PrimerApellido,
                    SegundoApellido = dto.SegundoApellido,
                    CiNumero = dto.CiNumero,
                    CiComplemento = dto.CiComplemento,
                    Telefono = dto.Telefono,
                    Email = dto.Email,
                    TipoCliente = "Regular"
                };
                (success, _, error) = await _clienteAdapter.CreateAsync(createDto);
            }
            else
            {
                var updateDto = new UpdateClienteDto
                {
                    ClienteId = dto.ClienteId,
                    Nombres = dto.Nombres,
                    PrimerApellido = dto.PrimerApellido,
                    SegundoApellido = dto.SegundoApellido,
                    CiNumero = dto.CiNumero,
                    CiComplemento = dto.CiComplemento,
                    Telefono = dto.Telefono,
                    Email = dto.Email,
                    TipoCliente = "Regular"
                };
                (success, error) = await _clienteAdapter.UpdateAsync(updateDto);
            }

            if (!success)
            {
                return new JsonResult(new { success = false, message = error ?? "Error al guardar el cliente" }) { StatusCode = 500 };
            }

            return new JsonResult(new { success = true, cliente = dto });
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (!ModelState.IsValid)
            {
                Clientes = await _clienteAdapter.GetAllAsync();
                return Page();
            }

            bool success;
            string? error = null;

            if (FormDto.ClienteId == 0)
            {
                var createDto = new CreateClienteDto
                {
                    Nombres = FormDto.Nombres,
                    PrimerApellido = FormDto.PrimerApellido,
                    SegundoApellido = FormDto.SegundoApellido,
                    CiNumero = FormDto.CiNumero,
                    CiComplemento = FormDto.CiComplemento,
                    Telefono = FormDto.Telefono,
                    Email = FormDto.Email,
                    TipoCliente = "Regular"
                };
                (success, _, error) = await _clienteAdapter.CreateAsync(createDto);
            }
            else
            {
                var updateDto = new UpdateClienteDto
                {
                    ClienteId = FormDto.ClienteId,
                    Nombres = FormDto.Nombres,
                    PrimerApellido = FormDto.PrimerApellido,
                    SegundoApellido = FormDto.SegundoApellido,
                    CiNumero = FormDto.CiNumero,
                    CiComplemento = FormDto.CiComplemento,
                    Telefono = FormDto.Telefono,
                    Email = FormDto.Email,
                    TipoCliente = "Regular"
                };
                (success, error) = await _clienteAdapter.UpdateAsync(updateDto);
            }

            if (!success)
            {
                ModelState.AddModelError(string.Empty, error ?? "No se pudo guardar el cliente.");
                Clientes = await _clienteAdapter.GetAllAsync();
                return Page();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var (success, error) = await _clienteAdapter.DeleteAsync(id);
            if (!success)
            {
                TempData["ErrorMessage"] = error ?? "No se pudo eliminar el cliente.";
            }

            return RedirectToPage();
        }

        public async Task<JsonResult> OnGetBuscarClientesAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return new JsonResult(new List<object>());

            var clientes = await _clienteAdapter.GetAllAsync();
            
            term = term.ToLower(CultureInfo.InvariantCulture);
            var resultados = clientes
                .Where(c => c.NombreCompleto.ToLower(CultureInfo.InvariantCulture).Contains(term, StringComparison.OrdinalIgnoreCase) ||
                           c.Ci.Contains(term))
                .OrderBy(c => c.NombreCompleto)
                .Select(c => new { id = c.ClienteId, text = c.NombreCompleto + " - CI: " + c.Ci })
                .Take(15)
                .ToList();

            return new JsonResult(resultados);
        }
    }
}