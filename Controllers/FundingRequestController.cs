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

            _context.Requests.Update(fundingRequest);
            await _context.SaveChangesAsync();

            return Ok(fundingRequest);
        }


        //ELIMINAR O COMENTAR
        [HttpPost("test-insert-many-requests")]
        public async Task<IActionResult> InsertManyTestFundingRequests()
        {
            var random = new Random();
            var requests = new List<FundingRequest>();

            var start = new DateTime(2024, 5, 1);
            var end = new DateTime(2025, 7, 1);

            int requestNumber = 1000;
            int paymentOrderNumber = 500;

            while (start <= end)
            {
                int requestsThisMonth = random.Next(5, 11); // entre 5 y 10 solicitudes

                for (int i = 0; i < requestsThisMonth; i++)
                {
                    var da = random.Next(1, 4); // DA entre 1 y 3
                    var amount = random.Next(1000, 8001); // Monto entre 1000 y 8000

                    var receivedDay = random.Next(1, 28); // Día entre 1 y 27
                    var receivedAt = new DateTime(start.Year, start.Month, receivedDay);
                    var dueDate = receivedAt.AddDays(random.Next(10, 30)).ToString("yyyy-MM-dd");

                    // Variar conceptos y fuentes de financiamiento
                    var concepts = new[] { "Compra de insumos", "Servicios", "Reparaciones", "Equipamiento", "Mantenimiento" };
                    var sources = new[] { "Nación", "Provincia", "Tesorería", "Fondo especial" };

                    var concept = concepts[random.Next(concepts.Length)];
                    var fundingSource = sources[random.Next(sources.Length)];

                    requests.Add(new FundingRequest
                    {
                        ReceivedAt = receivedAt,
                        DA = da,
                        RequestNumber = requestNumber++,
                        FiscalYear = receivedAt.Year,
                        PaymentOrderNumber = paymentOrderNumber++,
                        Concept = $"{concept} #{requestNumber}",
                        DueDate = dueDate,
                        Amount = amount,
                        FundingSource = fundingSource,
                        CheckingAccount = $"CUENTA-{da}-{i + 1}",
                        Comments = "Carga automática de prueba",
                        PartialPayment = (i % 3 == 0) ? 0.5 : 0,
                        IsActive = false,
                        UserId = "admin"
                    });
                }

                start = start.AddMonths(1);
            }

            _context.Requests.AddRange(requests);
            await _context.SaveChangesAsync();

            return Ok(new { inserted = requests.Count });
        }




    }
}
