using API_Pedidos.Models;

namespace API_Pedidos.Services
{
    public interface IDraftService
    {
        Task<DraftFundingRequest?> GetDraftByUserIdAsync(string userId);
        Task<DraftFundingRequest> SaveDraftAsync(string userId, string draftData);
        Task<bool> DeleteDraftAsync(string userId);
    }
}
