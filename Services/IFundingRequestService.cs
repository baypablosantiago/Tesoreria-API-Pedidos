using API_Pedidos.DTOs;
using API_Pedidos.Models;

namespace API_Pedidos.Services
{
    public interface IFundingRequestService
    {
        Task<FundingRequest> AddFundingRequestAsync(FundingRequest newFundingRequest, string userId);
        Task<IEnumerable<FundingRequest>> GetUserFundingRequestsAsync(string userId);
        Task<IEnumerable<FundingRequest>> GetAllActiveFundingRequestsAsync();
        Task<IEnumerable<FundingRequest>> GetAllInactiveFundingRequestsAsync();
        Task<FundingRequest?> UpdatePartialPaymentAsync(long id, double newPartialPayment);
        Task<FundingRequest?> ChangeIsActiveAsync(long id);
        Task<FundingRequest?> ChangeOnWorkAsync(long id);
        Task<FundingRequest?> AddCommentAsync(long id, string comment);
        Task<bool> UpdateFundingRequestAsync(FundingRequestUpdateDto dto);
    }
}