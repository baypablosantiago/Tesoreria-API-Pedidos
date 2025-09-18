using API_Pedidos.DTOs;

namespace API_Pedidos.Services
{
    public interface IPartialPaymentService
    {
        Task<List<PartialPaymentDto>> GetPartialPaymentHistoryAsync(long fundingRequestId);
        Task<decimal> GetTotalPartialPaymentAsync(long fundingRequestId);
        Task<PartialPaymentDto> CreatePartialPaymentAsync(long fundingRequestId, decimal amount, string userId, string userEmail);
        Task<bool> DeletePartialPaymentAsync(int paymentId);
    }
}