using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API_Pedidos.Models;
using Microsoft.AspNetCore.Mvc;

namespace API_Pedidos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FundingRequestController : ControllerBase
    {

        [HttpPost]
        public ActionResult<FundingRequest> AddFundingRequest(FundingRequest newFundingRequest)
        {
            if (newFundingRequest is null)
            {
                return BadRequest();
            }
            else
            {
                return CreatedAtAction("Done",newFundingRequest);
            }
        }

    }   
}
