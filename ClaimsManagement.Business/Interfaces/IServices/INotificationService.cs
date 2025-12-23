using ClaimsManagement.DataAccess.Models;
using ClaimsManagement.DataAccess.Enum;

namespace ClaimsManagement.Business.Interfaces.IServices
{
    public interface INotificationService
    {
        Task<IEnumerable<UserNotification>> GetUserNotificationsAsync(int userId);
        Task<IEnumerable<UserNotification>> GetUnreadNotificationsAsync(int userId);
        Task MarkAsReadAsync(int notificationId, int userId);
        Task MarkAllAsReadAsync(int userId);
        Task DeleteNotificationAsync(int notificationId, int userId);
        Task CreateNotificationAsync(int userId, string title, string message, NotificationType type);
        Task CreateNotificationAsync(int userId, string title, string message, NotificationType type, int? relatedClaimId);
    }
}