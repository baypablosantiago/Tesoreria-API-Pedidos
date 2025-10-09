using API_Pedidos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace API_Pedidos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DraftController : ControllerBase
    {
        private readonly IDraftService _draftService;

        public DraftController(IDraftService draftService)
        {
            _draftService = draftService;
        }

        [HttpGet, Authorize(Roles = "user,admin")]
        public async Task<IActionResult> GetDraft()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var draft = await _draftService.GetDraftByUserIdAsync(userId);
            if (draft == null)
                return NotFound();

            return Ok(draft);
        }

        [HttpPost, Authorize(Roles = "user,admin")]
        public async Task<IActionResult> SaveDraft([FromBody] JsonElement draftData)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var draftDataString = draftData.GetRawText();
            var savedDraft = await _draftService.SaveDraftAsync(userId, draftDataString);

            return Ok(savedDraft);
        }

        [HttpDelete, Authorize(Roles = "user,admin")]
        public async Task<IActionResult> DeleteDraft()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var deleted = await _draftService.DeleteDraftAsync(userId);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}
