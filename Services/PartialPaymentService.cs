using API_Pedidos.DTOs;
using API_Pedidos.Models;
using Microsoft.EntityFrameworkCore;

namespace API_Pedidos.Services
{
    public class PartialPaymentService : IPartialPaymentService
    {
        private readonly FundingRequestContext _context;

        public PartialPaymentService(FundingRequestContext context)
        {
            _context = context;
        }

        public async Task<List<PartialPaymentDto>> GetPartialPaymentHistoryAsync(long fundingRequestId)
        {
            var payments = await _context.PartialPayments
                .Where(p => p.FundingRequestId == fundingRequestId)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PartialPaymentDto
                {
                    Id = p.Id,
                    FundingRequestId = p.FundingRequestId,
                    Amount = p.Amount,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            return payments;
        }

        public async Task<decimal> GetTotalPartialPaymentAsync(long fundingRequestId)
        {
            return await _context.PartialPayments
                .Where(p => p.FundingRequestId == fundingRequestId)
                .SumAsync(p => p.Amount);
        }

        public async Task<PartialPaymentDto> CreatePartialPaymentAsync(long fundingRequestId, decimal amount, string userId, string userEmail)
        {
            var partialPayment = new PartialPayment
            {
                FundingRequestId = fundingRequestId,
                Amount = amount,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = userId,
                CreatedByUserEmail = userEmail
            };

            _context.PartialPayments.Add(partialPayment);
            await _context.SaveChangesAsync();

            return new PartialPaymentDto
            {
                Id = partialPayment.Id,
                FundingRequestId = partialPayment.FundingRequestId,
                Amount = partialPayment.Amount,
                CreatedAt = partialPayment.CreatedAt
                // CreatedByUserId y CreatedByUserEmail NO se exponen por seguridad
            };
        }

        public async Task<long?> DeletePartialPaymentAsync(int paymentId)
        {
            var partialPayment = await _context.PartialPayments.FindAsync(paymentId);
            if (partialPayment == null)
                return null;

            var fundingRequestId = partialPayment.FundingRequestId;

            // Eliminar el pago parcial
            _context.PartialPayments.Remove(partialPayment);
            await _context.SaveChangesAsync();

            // Recalcular y actualizar el total en FundingRequest
            var newTotal = await GetTotalPartialPaymentAsync(fundingRequestId);
            var fundingRequest = await _context.Requests.FindAsync(fundingRequestId);
            if (fundingRequest != null)
            {
                fundingRequest.PartialPayment = (double)newTotal;
                _context.Requests.Update(fundingRequest);
                await _context.SaveChangesAsync();
            }

            return fundingRequestId;
        }
    }
}