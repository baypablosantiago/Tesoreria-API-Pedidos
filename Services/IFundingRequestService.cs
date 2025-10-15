using API_Pedidos.DTOs;

namespace API_Pedidos.Services
{
    public interface IFundingRequestService
    {
        Task<FundingRequestResponseDto> AddFundingRequestAsync(FundingRequestCreateDto newFundingRequest, string userId);
        Task<IEnumerable<FundingRequestResponseDto>> GetUserFundingRequestsAsync(string userId);
        Task<IEnumerable<FundingRequestAdminResponseDto>> GetAllActiveFundingRequestsAsync();
        Task<IEnumerable<FundingRequestAdminResponseDto>> GetAllInactiveFundingRequestsAsync();
        Task<FundingRequestAdminResponseDto?> UpdatePartialPaymentAsync(long id, double newPartialPayment, string currentUserId);
        Task<FundingRequestAdminResponseDto?> ChangeIsActiveAsync(long id, string currentUserId);
        Task<int> SetOnWorkBatchAsync(SetOnWorkBatchDto dto, string currentUserId);
        Task<FundingRequestAdminResponseDto?> AddCommentAsync(long id, string comment, string currentUserId);
        Task<FundingRequestResponseDto?> UpdateFundingRequestAsync(FundingRequestUpdateDto dto, string userId);
    }
}