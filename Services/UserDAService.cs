using Microsoft.EntityFrameworkCore;
using API_Pedidos.Models;

namespace API_Pedidos.Services
{
    public class UserDAService : IUserDAService
    {
        private readonly FundingRequestContext _context;

        public UserDAService(FundingRequestContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<int>> GetUserDANumbersAsync(string userId)
        {
            return await _context.UserDAs
                .Where(u => u.UserId == userId && u.IsActive)
                .Select(u => u.DANumber)
                .OrderDescending()
                .ToListAsync();
        }
    }
}