using API_Pedidos.DTOs;

namespace API_Pedidos.Services
{
    public interface IFundingRequestService
    {
        Task<FundingRequestResponseDto> AddFundingRequestAsync(FundingRequestCreateDto newFundingRequest, string userId);
        Task<IEnumerable<FundingRequestResponseDto>> GetUserFundingRequestsAsync(string userId);
        Task<IEnumerable<FundingRequestAdminResponseDto>> GetAllActiveFundingRequestsAsync();
        Task<IEnumerable<FundingRequestAdminResponseDto>> GetAllInactiveFundingRequestsAsync();
        Task<FundingRequestAdminResponseDto?> UpdatePartialPaymentAsync(long id, double newPartialPayment);
        Task<FundingRequestAdminResponseDto?> ChangeIsActiveAsync(long id);
        Task<FundingRequestAdminResponseDto?> ChangeOnWorkAsync(long id);
        Task<FundingRequestAdminResponseDto?> AddCommentAsync(long id, string comment);
        Task<bool> UpdateFundingRequestAsync(FundingRequestUpdateDto dto, string userId);
    }
}