using API_Pedidos.Models;
using Microsoft.EntityFrameworkCore;

namespace API_Pedidos.Services
{
    public class DraftService : IDraftService
    {
        private readonly FundingRequestContext _context;

        public DraftService(FundingRequestContext context)
        {
            _context = context;
        }

        public async Task<DraftFundingRequest?> GetDraftByUserIdAsync(string userId)
        {
            return await _context.DraftFundingRequests
                .Where(d => d.UserId == userId)
                .FirstOrDefaultAsync();
        }

        public async Task<DraftFundingRequest> SaveDraftAsync(string userId, string draftData)
        {
            var existingDraft = await GetDraftByUserIdAsync(userId);

            if (existingDraft != null)
            {
                // Update existing draft
                existingDraft.DraftData = draftData;
                existingDraft.LastSaved = DateTime.UtcNow;
                _context.DraftFundingRequests.Update(existingDraft);
            }
            else
            {
                // Create new draft
                var newDraft = new DraftFundingRequest
                {
                    UserId = userId,
                    DraftData = draftData,
                    LastSaved = DateTime.UtcNow
                };
                await _context.DraftFundingRequests.AddAsync(newDraft);
                existingDraft = newDraft;
            }

            await _context.SaveChangesAsync();
            return existingDraft;
        }

        public async Task<bool> DeleteDraftAsync(string userId)
        {
            var draft = await GetDraftByUserIdAsync(userId);
            if (draft == null) return false;

            _context.DraftFundingRequests.Remove(draft);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
