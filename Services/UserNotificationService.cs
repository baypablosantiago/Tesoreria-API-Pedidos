using API_Pedidos.Models;
using Microsoft.EntityFrameworkCore;

namespace API_Pedidos.Services
{
    public class UserNotificationService : IUserNotificationService
    {
        private readonly FundingRequestContext _context;

        public UserNotificationService(FundingRequestContext context)
        {
            _context = context;
        }

        public async Task CreateNotificationForUserAsync(string userId, long requestId, string changeType, string message)
        {
            var notification = new UserNotification
            {
                UserId = userId,
                RequestId = requestId,
                ChangeType = changeType,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _context.UserNotifications.AddAsync(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<List<UserNotification>> GetNotificationsForUserAsync(string userId, int limit = 50, DateTime? since = null)
        {
            var query = _context.UserNotifications
                .Where(n => n.UserId == userId);

            // Si se proporciona 'since', filtrar solo las nuevas
            if (since.HasValue)
            {
                query = query.Where(n => n.CreatedAt > since.Value);
            }

            return await query
                .OrderByDescending(n => n.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountForUserAsync(string userId)
        {
            return await _context.UserNotifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .CountAsync();
        }

        public async Task MarkAsReadAsync(long notificationId, string userId)
        {
            var notification = await _context.UserNotifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification != null && !notification.IsRead)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            await _context.UserNotifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
        }
    }
}
