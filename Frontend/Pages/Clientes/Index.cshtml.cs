#nullable disable
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Taller_Mecanico_Arqui.Application.UseCases.Clientes;
using Taller_Mecanico_Arqui.Application.DTOs.Clientes;
using Taller_Mecanico_Arqui.Domain.Common;
using Taller_Mecanico_Arqui.Infrastructure.Authorization;
using Taller_Mecanico_Arqui.Domain.Enums;
using Taller_Mecanico_Arqui.Infrastructure.Services;
using Taller_Mecanico_Arqui.Domain.Ports;

namespace Taller_Mecanico_Arqui.Pages.Clientes
{
    [RequireAccessLevel(NivelAcceso.Parcial)]
    public class IndexModel : PageModel
    {
        private readonly GetAllClientesUseCase _getAllClientesUseCase;
        private readonly GetClienteByIdUseCase _getClienteByIdUseCase;
        private readonly CreateClienteUseCase _createClienteUseCase;
        private readonly UpdateClienteUseCase _updateClienteUseCase;
        private readonly DeleteClienteUseCase _deleteClienteUseCase;
        private readonly IClienteRepository _clienteRepository;
        private readonly AuthenticationHelper _authHelper;

        public IndexModel(
            GetAllClientesUseCase getAllClientesUseCase,
            GetClienteByIdUseCase getClienteByIdUseCase,
            CreateClienteUseCase createClienteUseCase,
            UpdateClienteUseCase updateClienteUseCase,
            DeleteClienteUseCase deleteClienteUseCase,
            IClienteRepository clienteRepository,
            AuthenticationHelper authHelper)
        {
            _getAllClientesUseCase = getAllClientesUseCase;
            _getClienteByIdUseCase = getClienteByIdUseCase;
            _createClienteUseCase = createClienteUseCase;
            _updateClienteUseCase = updateClienteUseCase;
            _deleteClienteUseCase = deleteClienteUseCase;
            _clienteRepository = clienteRepository;
            _authHelper = authHelper;
        }

        public IList<ClienteListDto> Clientes { get; set; } = new List<ClienteListDto>();
        public bool CanModify => _authHelper.GetCurrentUserAccessLevel() == NivelAcceso.Completo;

        [BindProperty]
        public ClienteFormDto FormDto { get; set; } = new();

        public async Task OnGetAsync()
        {
            Clientes = (await _getAllClientesUseCase.ExecuteAsync())
                .Select(c => new ClienteListDto
                {
                    ClienteId = c.ClienteId,
                    NombreCompleto = c.NombreCompleto?.ToString() ?? string.Empty,
                    Telefono = c.Telefono,
                    Email = c.Email,
                    FechaRegistro = c.FechaRegistro
                })
                .ToList();
        }

        public async Task<JsonResult> OnGetClienteAsync(int id)
        {
            var clienteResult = await _getClienteByIdUseCase.ExecuteAsync(id);
            if (clienteResult.IsFailure)
            {
                if (clienteResult.ErrorCode == ErrorCodes.ClienteNotFound)
                    return new JsonResult(new { error = "Cliente no encontrado." }) { StatusCode = 404 };

                return new JsonResult(new { error = clienteResult.ErrorMessage ?? "Error al consultar cliente." }) { StatusCode = 500 };
            }

            var cliente = clienteResult.Value!;
            return new JsonResult(new
            {
                clienteId = cliente.ClienteId,
                nombres = cliente.NombreCompleto!.Nombres,
                primerApellido = cliente.NombreCompleto!.PrimerApellido,
                segundoApellido = cliente.NombreCompleto!.SegundoApellido,
                ciNumero = cliente.Ci.Numero,
                ciComplemento = cliente.Ci.Complemento,
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

            Result saveResult;
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
                saveResult = await _createClienteUseCase.ExecuteAsync(createDto);
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
                saveResult = await _updateClienteUseCase.ExecuteAsync(updateDto);
            }

            if (saveResult.IsFailure)
            {
                return new JsonResult(new { success = false, message = saveResult.ErrorMessage ?? "Error al guardar el cliente" }) { StatusCode = 500 };
            }

            return new JsonResult(new { success = true, cliente = dto });
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (!CanModify)
            {
                TempData["ErrorMessage"] = "No tienes permisos para crear o modificar clientes.";
                return RedirectToPage();
            }

            if (!ModelState.IsValid)
            {
                Clientes = (await _getAllClientesUseCase.ExecuteAsync())
                    .Select(c => new ClienteListDto
                    {
                        ClienteId = c.ClienteId,
                        NombreCompleto = c.NombreCompleto?.ToString() ?? string.Empty,
                        Telefono = c.Telefono,
                        Email = c.Email,
                        FechaRegistro = c.FechaRegistro
                    })
                    .ToList();
                return Page();
            }

            Result saveResult;
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
                saveResult = await _createClienteUseCase.ExecuteAsync(createDto);
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
                saveResult = await _updateClienteUseCase.ExecuteAsync(updateDto);
            }

            if (saveResult.IsFailure)
            {
                ModelState.AddModelError(string.Empty, saveResult.ErrorMessage ?? "No se pudo guardar el cliente.");
                Clientes = (await _getAllClientesUseCase.ExecuteAsync())
                    .Select(c => new ClienteListDto
                    {
                        ClienteId = c.ClienteId,
                        NombreCompleto = c.NombreCompleto?.ToString() ?? string.Empty,
                        Telefono = c.Telefono,
                        Email = c.Email,
                        FechaRegistro = c.FechaRegistro
                    })
                    .ToList();
                return Page();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            if (!CanModify)
            {
                TempData["ErrorMessage"] = "No tienes permisos para eliminar clientes.";
                return RedirectToPage();
            }

            var deleteResult = await _deleteClienteUseCase.ExecuteAsync(id);
            if (deleteResult.IsFailure)
            {
                TempData["ErrorMessage"] = deleteResult.ErrorMessage ?? "No se pudo eliminar el cliente.";
            }

            return RedirectToPage();
        }

        public async Task<JsonResult> OnGetBuscarClientesAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return new JsonResult(new List<object>());

            term = term.ToLower(CultureInfo.InvariantCulture);
            var clientes = (await _clienteRepository.GetAllAsync())
                .Where(c => !c.IsDeleted &&
                    ((c.NombreCompleto?.ToString() ?? string.Empty).ToLower(CultureInfo.InvariantCulture).Contains(term, StringComparison.OrdinalIgnoreCase) ||
                     c.Ci.Numero.ToString().Contains(term)))
                .OrderBy(c => c.NombreCompleto?.ToString() ?? string.Empty)
                .Select(c => new { id = c.ClienteId, text = (c.NombreCompleto?.ToString() ?? string.Empty) + " - CI: " + c.Ci.Numero })
                .Take(15)
                .ToList();

            return new JsonResult(clientes);
        }
    }
}
