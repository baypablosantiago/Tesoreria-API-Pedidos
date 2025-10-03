using API_Pedidos.DTOs;
using API_Pedidos.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API_Pedidos.Services
{
    public class AdminNotificationService : IAdminNotificationService
    {
        private readonly FundingRequestContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AdminNotificationService(FundingRequestContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task CreateNotificationForAllAdminsAsync(FundingRequestChangeNotificationDto notificationData)
        {
            // Obtener todos los usuarios con rol "admin"
            var admins = await _userManager.GetUsersInRoleAsync("admin");

            if (!admins.Any())
                return;

            // Crear una notificación para cada admin
            var notifications = admins.Select(admin => new AdminNotification
            {
                AdminUserId = admin.Id,
                RequestId = notificationData.RequestId,
                RequestNumber = notificationData.RequestNumber,
                DA = notificationData.DA,
                ChangeType = notificationData.ChangeType,
                FieldChanged = notificationData.FieldChanged,
                OldValue = notificationData.OldValue,
                NewValue = notificationData.NewValue,
                UserEmail = notificationData.UserEmail,
                ChangeDate = notificationData.ChangeDate,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            await _context.AdminNotifications.AddRangeAsync(notifications);
            await _context.SaveChangesAsync();
        }

        public async Task<List<AdminNotification>> GetNotificationsForAdminAsync(string adminUserId, int limit = 50)
        {
            return await _context.AdminNotifications
                .Where(n => n.AdminUserId == adminUserId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountForAdminAsync(string adminUserId)
        {
            return await _context.AdminNotifications
                .Where(n => n.AdminUserId == adminUserId && !n.IsRead)
                .CountAsync();
        }

        public async Task MarkAsReadAsync(long notificationId, string adminUserId)
        {
            var notification = await _context.AdminNotifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.AdminUserId == adminUserId);

            if (notification != null && !notification.IsRead)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync(string adminUserId)
        {
            var unreadNotifications = await _context.AdminNotifications
                .Where(n => n.AdminUserId == adminUserId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }
    }
}
