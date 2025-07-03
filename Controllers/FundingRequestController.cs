using API_Pedidos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API_Pedidos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FundingRequestController : ControllerBase
    {
        private readonly FundingRequestContext _context;
        public FundingRequestController(FundingRequestContext context)
        {
            _context = context;
        }

        [HttpPost, Authorize(Roles = "user")]
        public async Task<IActionResult> AddFundingRequest(FundingRequest newFundingRequest)
        {
            if (newFundingRequest is null)
            {
                return BadRequest();
            }
            else
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId is null)
                    return Unauthorized();

                newFundingRequest.UserId = userId;

                _context.Add(newFundingRequest);
                await _context.SaveChangesAsync();
                return Ok(newFundingRequest);
            }
        }

        [HttpGet("user"), Authorize(Roles = "user")]
        public async Task<IActionResult> GetMyFundingRequests()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var myRequests = await _context.Requests
                .Where(r => r.UserId == userId)
                .ToListAsync();

            return Ok(myRequests);
        }

        [HttpGet("all"), Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAllFundingRequest()
        {
            return Ok(await _context.Requests.ToListAsync());
        }

        [HttpPatch("partial-payment/{id}"), Authorize(Roles = "admin")]
        public async Task<IActionResult> PartialPayment(long id, [FromBody] double newPartialPayment)
        {
            var fundingRequest = await _context.Requests.FindAsync(id);
            if (fundingRequest == null)
            {
                return NotFound();
            }

            fundingRequest.PartialPayment = newPartialPayment;

            _context.Requests.Update(fundingRequest);
            await _context.SaveChangesAsync();

            return Ok(fundingRequest);
        }

        [HttpPatch("is-active/{id}"), Authorize(Roles = "admin")]
        public async Task<IActionResult> ChangeIsActive(long id)
        {
            var fundingRequest = await _context.Requests.FindAsync(id);
            if (fundingRequest == null)
            {
                return NotFound();
            }

            var state = fundingRequest.IsActive;
            fundingRequest.IsActive = !state;

            _context.Requests.Update(fundingRequest);
            await _context.SaveChangesAsync();

            return Ok(fundingRequest);
        }



    }
}
