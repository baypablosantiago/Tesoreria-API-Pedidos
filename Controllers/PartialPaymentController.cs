using API_Pedidos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_Pedidos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PartialPaymentController : ControllerBase
    {
        private readonly IPartialPaymentService _partialPaymentService;

        public PartialPaymentController(IPartialPaymentService partialPaymentService)
        {
            _partialPaymentService = partialPaymentService;
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
    }
}