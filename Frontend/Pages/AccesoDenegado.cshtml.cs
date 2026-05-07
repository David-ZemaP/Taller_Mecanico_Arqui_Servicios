using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Taller_Mecanico_Arqui.Pages;

public class AccesoDenegadoModel : PageModel
{
    public string UserLevel { get; set; } = "Desconocido";

    public void OnGet()
    {
        // Get user's access level from claims
        var nivelAccesoClaim = User.FindFirst("NivelAcceso");
        
        if (nivelAccesoClaim != null)
        {
            UserLevel = nivelAccesoClaim.Value;
        }
        else if (User.Identity?.IsAuthenticated == true)
        {
            UserLevel = "Sin nivel asignado";
        }
        else
        {
            UserLevel = "No autenticado";
        }
    }
}
