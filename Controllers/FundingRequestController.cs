using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API_Pedidos.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

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

        [HttpPost]
        public async Task<IActionResult> AddFundingRequest(FundingRequest newFundingRequest)
        {
            if (newFundingRequest is null)
            {
                return BadRequest();
            }
            else
            {
                _context.Add(newFundingRequest);
                await _context.SaveChangesAsync();
                return Ok(newFundingRequest);
            }
        }

    }   
}
