using API_Pedidos.DTOs;
using API_Pedidos.Models;

namespace API_Pedidos.Services
{
    public interface IAdminNotificationService
    {
        Task CreateNotificationForAllAdminsAsync(FundingRequestChangeNotificationDto notificationData);
        Task<List<AdminNotification>> GetNotificationsForAdminAsync(string adminUserId, int limit = 50);
        Task<int> GetUnreadCountForAdminAsync(string adminUserId);
        Task MarkAsReadAsync(long notificationId, string adminUserId);
        Task MarkAllAsReadAsync(string adminUserId);
    }
}
