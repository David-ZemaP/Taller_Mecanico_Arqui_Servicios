using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Taller_Mecanico_Users.Domain.Ports;

namespace Taller_Mecanico_Users.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly IRolRepository _rolRepository;

        public RolesController(IRolRepository rolRepository)
        {
            _rolRepository = rolRepository;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var roles = await _rolRepository.GetAllAsync();
            
            var result = roles.Select(r => new
            {
                rolId = r.RolId,
                nombre = r.Nombre,
                descripcion = r.Descripcion
            });

            return Ok(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var rol = await _rolRepository.GetByIdAsync(id);
            
            if (rol == null)
            {
                return NotFound(new { message = "Rol no encontrado" });
            }

            return Ok(new
            {
                rolId = rol.RolId,
                nombre = rol.Nombre,
                descripcion = rol.Descripcion
            });
        }
    }
}