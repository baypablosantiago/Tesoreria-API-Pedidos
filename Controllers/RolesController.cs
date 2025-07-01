using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API_Pedidos.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RolesController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        public RolesController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }


        [HttpGet, Authorize]
        public async Task<IActionResult> GetRole()
        {
            var userName = User.Identity?.Name;

            if (string.IsNullOrEmpty(userName))
                return Unauthorized("Usuario no autenticado");

            var user = await _userManager.FindByNameAsync(userName);

            if (user == null)
                return NotFound("Usuario no encontrado");

            var roles = await _userManager.GetRolesAsync(user);

            var role = roles.FirstOrDefault();

            return Ok(new { role });
        }

    }
}