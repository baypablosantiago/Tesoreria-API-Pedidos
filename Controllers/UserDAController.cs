using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using API_Pedidos.Services;

namespace API_Pedidos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserDAController : ControllerBase
    {
        private readonly IUserDAService _userDAService;

        public UserDAController(IUserDAService userDAService)
        {
            _userDAService = userDAService;
        }

        [HttpGet, Authorize(Roles = "user,admin")]
        public async Task<IActionResult> GetUserDAs()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
                return Unauthorized();

            var daNumbers = await _userDAService.GetUserDANumbersAsync(userId);
            return Ok(daNumbers);
        }
    }
}