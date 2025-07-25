using API_Pedidos.DTOs;
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

        [HttpPost, Authorize(Roles = "user,admin")]
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

        [HttpGet("active-requests"), Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAllFundingRequest()
        {
            var requests = await _context.Requests
             .Where(fr => fr.IsActive)
             .OrderByDescending(fr => fr.ReceivedAt)
             .ToListAsync();

            return Ok(requests);
        }

        [HttpGet("inactive-requests"), Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAllInactiveRequest()
        {
            var requests = await _context.Requests
            .Where(fr => !fr.IsActive)
            .OrderByDescending(fr => fr.ReceivedAt)
            .ToListAsync();

            return Ok(requests);
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
            if (fundingRequest.IsActive == false)
            {
                fundingRequest.OnWork = true;
            }

            _context.Requests.Update(fundingRequest);
            await _context.SaveChangesAsync();

            return Ok(fundingRequest);
        }

        [HttpPatch("on-work/{id}"), Authorize(Roles = "admin")]
        public async Task<IActionResult> ChangeInWork(long id)
        {
            var fundingRequest = await _context.Requests.FindAsync(id);
            if (fundingRequest == null)
            {
                return NotFound();
            }

            var onWork = fundingRequest.OnWork;
            fundingRequest.OnWork = !onWork;

            _context.Requests.Update(fundingRequest);
            await _context.SaveChangesAsync();

            return Ok(fundingRequest);
        }

        [HttpPatch("add-comment/{id}"), Authorize(Roles = "admin")]
        public async Task<IActionResult> AddComment(long id, [FromBody] CommentsFromTesoDto dto)
        {
            var fundingRequest = await _context.Requests.FindAsync(id);
            if (fundingRequest == null)
            {
                return NotFound();
            }

            fundingRequest.CommentsFromTeso = dto.Comment;

            _context.Requests.Update(fundingRequest);
            await _context.SaveChangesAsync();

            return Ok(fundingRequest);
        }

        [HttpPut("update-request"), Authorize(Roles = "user")]
        public async Task<IActionResult> UpdateFundingRequest([FromBody] FundingRequestUpdateDto dto)
        {
            var request = await _context.Requests.FindAsync(dto.Id);

            if (request == null)
            {
                return NotFound(new { message = "La solicitud no fue encontrada." });
            }

            request.RequestNumber = dto.RequestNumber;
            request.FiscalYear = dto.FiscalYear;
            request.PaymentOrderNumber = dto.PaymentOrderNumber;
            request.Concept = dto.Concept;
            request.Amount = dto.Amount;
            request.FundingSource = dto.FundingSource;
            request.CheckingAccount = dto.CheckingAccount;
            request.DueDate = dto.DueDate;
            request.Comments = dto.Comments;

            _context.Requests.Update(request);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Solicitud actualizada correctamente." });
        }

    }
}
