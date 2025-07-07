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

            var das = new[] { 959, 965, 953 };

            var conceptTemplates = new[]
            {
        "Adquisición de insumos médicos para hospitales provinciales",
        "Servicio de mantenimiento integral de equipos informáticos",
        "Reparación de infraestructura edilicia en escuelas rurales",
        "Compra de mobiliario para oficinas administrativas",
        "Servicio de limpieza y mantenimiento en dependencias públicas",
        "Contratación de transporte para programas sociales",
        "Actualización de software y licencias gubernamentales",
        "Adquisición de materiales didácticos para instituciones educativas",
        "Mantenimiento preventivo de flota vehicular oficial",
        "Servicios de consultoría para planificación presupuestaria"
            };

            var sources = new[] { "Nación", "Provincia", "Tesorería", "Fondo especial" };

            while (start <= end)
            {
                int requestsThisMonth = random.Next(2, 5); // entre 2 y 5 solicitudes

                for (int i = 0; i < requestsThisMonth; i++)
                {
                    var da = das[random.Next(das.Length)];
                    var amount = random.Next(800_000, 8_000_001); // entre 800.000 y 8.000.000

                    var receivedDay = random.Next(1, 28); // Día entre 1 y 27
                    var receivedAt = new DateTime(start.Year, start.Month, receivedDay);
                    var dueDate = receivedAt.AddDays(random.Next(10, 30)).ToString("yyyy-MM-dd");

                    var concept = conceptTemplates[random.Next(conceptTemplates.Length)];
                    var fundingSource = sources[random.Next(sources.Length)];

                    // 50% de probabilidad de tener comentarios
                    var hasComments = random.NextDouble() < 0.5;
                    var comments = hasComments ? "Carga automática de prueba" : null;

                    bool hasPartialPayment = random.NextDouble() < 0.2; // 20% chance
                    double partialPayment = 0;
                    if (hasPartialPayment)
                    {
                        partialPayment = (double)random.Next(1, (int)(amount / 3) + 1);
                    }


                    requests.Add(new FundingRequest
                    {
                        ReceivedAt = receivedAt,
                        DA = da,
                        RequestNumber = requestNumber++,
                        FiscalYear = receivedAt.Year,
                        PaymentOrderNumber = paymentOrderNumber++,
                        Concept = $"{concept} (Ref. #{requestNumber})",
                        DueDate = dueDate,
                        Amount = amount,
                        FundingSource = fundingSource,
                        CheckingAccount = $"CUENTA-{da}-{i + 1}",
                        Comments = comments,
                        PartialPayment = partialPayment,
                        IsActive = true,
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
