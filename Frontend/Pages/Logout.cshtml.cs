using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Taller_Mecanico_Arqui.Pages
{
    [AllowAnonymous]
    public class LogoutModel : PageModel
    {
        private const string AuthenticationScheme = "FrontendScheme";

        public async Task<IActionResult> OnGetAsync()
        {
            await HttpContext.SignOutAsync(AuthenticationScheme);
            return RedirectToPage("/Login");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await HttpContext.SignOutAsync(AuthenticationScheme);
            return RedirectToPage("/Login");
        }
    }
}
