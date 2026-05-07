using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Taller_Mecanico_Arqui.Frontend.Adapters;
using Taller_Mecanico_Arqui.Frontend.Authorization;

namespace Taller_Mecanico_Arqui.Pages
{
    [RequireAccessLevel(NivelAcceso.Completo)]
    public class UsuariosModel : PageModel
    {
        private readonly IUsersServiceAdapter _usersServiceAdapter;

        public List<UserListDto> Usuarios { get; set; } = new();

        public UsuariosModel(IUsersServiceAdapter usersServiceAdapter)
        {
            _usersServiceAdapter = usersServiceAdapter;
        }

        public async Task OnGetAsync()
        {
            Usuarios = await _usersServiceAdapter.GetAllUsersAsync();
        }

        public IActionResult OnPostSaveAsync()
        {
            TempData["ErrorMessage"] = "La gestión de usuarios via web no está disponible actualmente. Esta funcionalidad requiere endpoints API en UsersService.";
            return RedirectToPage("/Index");
        }

        public IActionResult OnPostToggleActivoAsync(int id)
        {
            TempData["ErrorMessage"] = "La gestión de usuarios via web no está disponible actualmente. Esta funcionalidad requiere endpoints API en UsersService.";
            return RedirectToPage("/Index");
        }

        public IActionResult OnPostResetPasswordAsync(int id)
        {
            TempData["ErrorMessage"] = "La gestión de usuarios via web no está disponible actualmente. Esta funcionalidad requiere endpoints API en UsersService.";
            return RedirectToPage("/Index");
        }
    }
}
