using API_Pedidos.Hubs;
using API_Pedidos.Models;
using API_Pedidos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace API_Pedidos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PartialPaymentController : ControllerBase
    {
        private readonly IPartialPaymentService _partialPaymentService;
        private readonly IHubContext<FundingRequestHub> _hubContext;
        private readonly FundingRequestContext _context;

        public PartialPaymentController(
            IPartialPaymentService partialPaymentService,
            IHubContext<FundingRequestHub> hubContext,
            FundingRequestContext context)
        {
            _partialPaymentService = partialPaymentService;
            _hubContext = hubContext;
            _context = context;
        }

        [HttpGet("{fundingRequestId}/history")]
        public async Task<IActionResult> GetPartialPaymentHistory(long fundingRequestId)
        {
            var history = await _partialPaymentService.GetPartialPaymentHistoryAsync(fundingRequestId);
            return Ok(history);
        }

        [HttpGet("{fundingRequestId}/total")]
        public async Task<IActionResult> GetTotalPartialPayment(long fundingRequestId)
        {
            var total = await _partialPaymentService.GetTotalPartialPaymentAsync(fundingRequestId);
            return Ok(new { total });
        }

        [HttpDelete("{paymentId}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeletePartialPayment(int paymentId)
        {
            var fundingRequestId = await _partialPaymentService.DeletePartialPaymentAsync(paymentId);
            if (fundingRequestId == null)
                return NotFound();

            // Obtener el funding request actualizado y enviarlo por SignalR
            var fundingRequest = await _context.Requests.FindAsync(fundingRequestId.Value);
            if (fundingRequest != null)
            {
                var dto = FundingRequestMapper.ToAdminResponseDto(fundingRequest);
                await _hubContext.Clients.Group("admins").SendAsync("FundingRequestChanged", dto);
            }

            return Ok();
        }
    }
}