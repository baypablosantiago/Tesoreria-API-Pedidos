using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API_Pedidos.Services;

namespace API_Pedidos.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RolesController : Controller
    {
        private readonly IRolesService _rolesService;

        public RolesController(IRolesService rolesService)
        {
            _rolesService = rolesService;
        }

        [HttpGet, Authorize]
        public async Task<IActionResult> GetRole()
        {
            var userName = User.Identity?.Name;

            if (string.IsNullOrEmpty(userName))
                return Unauthorized("Usuario no autenticado");

            var role = await _rolesService.GetUserRoleAsync(userName);

            if (role == null)
                return NotFound("Usuario no encontrado");

            return Ok(new { role });
        }
    }
}