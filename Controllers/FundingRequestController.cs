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


        [HttpGet, Authorize(Roles = "employee")]
        public async Task<IActionResult> GetAllFundingRequest()
        {
            return Ok(await _context.Requests.ToListAsync());
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


    }
}
