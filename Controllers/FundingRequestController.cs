using API_Pedidos.DTOs;
using API_Pedidos.Models;
using API_Pedidos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API_Pedidos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FundingRequestController : ControllerBase
    {
        private readonly IFundingRequestService _fundingRequestService;

        public FundingRequestController(IFundingRequestService fundingRequestService)
        {
            _fundingRequestService = fundingRequestService;
        }

        [HttpPost, Authorize(Roles = "user,admin")]
        public async Task<IActionResult> AddFundingRequest(FundingRequestCreateDto newFundingRequest)
        {
            if (newFundingRequest is null)
                return BadRequest();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
                return Unauthorized();

            var result = await _fundingRequestService.AddFundingRequestAsync(newFundingRequest, userId);
            return Ok(result);
        }

        [HttpGet("user"), Authorize(Roles = "user")]
        public async Task<IActionResult> GetMyFundingRequests()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var myRequests = await _fundingRequestService.GetUserFundingRequestsAsync(userId);
            return Ok(myRequests);
        }

        [HttpGet("active-requests"), Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAllFundingRequest()
        {
            var requests = await _fundingRequestService.GetAllActiveFundingRequestsAsync();
            return Ok(requests);
        }

        [HttpGet("inactive-requests"), Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAllInactiveRequest()
        {
            var requests = await _fundingRequestService.GetAllInactiveFundingRequestsAsync();
            return Ok(requests);
        }

        [HttpPatch("partial-payment/{id}"), Authorize(Roles = "admin")]
        public async Task<IActionResult> PartialPayment(long id, [FromBody] PartialPaymentUpdateDto dto)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == null)
                return Unauthorized();

            var result = await _fundingRequestService.UpdatePartialPaymentAsync(id, dto.PartialPayment, currentUserId);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpPatch("is-active/{id}"), Authorize(Roles = "admin")]
        public async Task<IActionResult> ChangeIsActive(long id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == null)
                return Unauthorized();

            var result = await _fundingRequestService.ChangeIsActiveAsync(id, currentUserId);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpPost("batch/set-onwork"), Authorize(Roles = "admin")]
        public async Task<IActionResult> SetOnWorkBatch([FromBody] SetOnWorkBatchDto dto)
        {
            if (dto == null || dto.RequestIds == null || dto.RequestIds.Count == 0)
                return BadRequest("No se proporcionaron IDs de solicitudes");

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == null)
                return Unauthorized();

            var updatedCount = await _fundingRequestService.SetOnWorkBatchAsync(dto, currentUserId);

            return Ok(new { UpdatedCount = updatedCount, Message = $"{updatedCount} solicitudes actualizadas" });
        }

        [HttpPost("batch/mark-as-on-work"), Authorize(Roles = "admin")]
        public async Task<IActionResult> MarkAsOnWork([FromBody] List<long> requestIds)
        {
            if (requestIds == null || requestIds.Count == 0)
                return BadRequest("No se proporcionaron IDs de solicitudes");

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == null)
                return Unauthorized();

            var dto = new SetOnWorkBatchDto
            {
                RequestIds = requestIds,
                OnWork = true
            };

            var updatedCount = await _fundingRequestService.SetOnWorkBatchAsync(dto, currentUserId);

            return Ok(new { UpdatedCount = updatedCount, Message = $"{updatedCount} solicitudes marcadas como 'En revisión'" });
        }

        [HttpPost("batch/mark-as-pending"), Authorize(Roles = "admin")]
        public async Task<IActionResult> MarkAsPending([FromBody] List<long> requestIds)
        {
            if (requestIds == null || requestIds.Count == 0)
                return BadRequest("No se proporcionaron IDs de solicitudes");

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == null)
                return Unauthorized();

            var dto = new SetOnWorkBatchDto
            {
                RequestIds = requestIds,
                OnWork = false
            };

            var updatedCount = await _fundingRequestService.SetOnWorkBatchAsync(dto, currentUserId);

            return Ok(new { UpdatedCount = updatedCount, Message = $"{updatedCount} solicitudes marcadas como 'Pendiente'" });
        }

        [HttpPatch("add-comment/{id}"), Authorize(Roles = "admin")]
        public async Task<IActionResult> AddComment(long id, [FromBody] CommentsFromTesoDto dto)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == null)
                return Unauthorized();

            var result = await _fundingRequestService.AddCommentAsync(id, dto.Comment, currentUserId);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpPut("update-request"), Authorize(Roles = "user")]
        public async Task<IActionResult> UpdateFundingRequest([FromBody] FundingRequestUpdateDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var result = await _fundingRequestService.UpdateFundingRequestAsync(dto, userId);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

    }
}
