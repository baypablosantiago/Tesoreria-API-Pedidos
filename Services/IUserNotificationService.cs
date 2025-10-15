using API_Pedidos.Models;

namespace API_Pedidos.Services
{
    public interface IUserNotificationService
    {
        Task CreateNotificationForUserAsync(string userId, long requestId, string changeType, int requestNumber, bool? onWork = null);
        Task<List<UserNotification>> GetNotificationsForUserAsync(string userId, int limit = 50, DateTime? since = null);
        Task<int> GetUnreadCountForUserAsync(string userId);
        Task MarkAsReadAsync(long notificationId, string userId);
        Task MarkAllAsReadAsync(string userId);
    }
}
