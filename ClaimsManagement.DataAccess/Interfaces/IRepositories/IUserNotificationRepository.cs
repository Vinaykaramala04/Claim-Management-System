using ClaimsManagement.DataAccess.Models;

namespace ClaimsManagement.DataAccess.Interfaces.IRepositories
{
    public interface IUserNotificationRepository : IBaseRepository<UserNotification>
    {
        Task<IEnumerable<UserNotification>> GetByUserIdAsync(int userId);
        Task<IEnumerable<UserNotification>> GetUnreadByUserIdAsync(int userId);
        Task MarkAsReadAsync(int notificationId, int userId);
        Task MarkAllAsReadAsync(int userId);
    }
}