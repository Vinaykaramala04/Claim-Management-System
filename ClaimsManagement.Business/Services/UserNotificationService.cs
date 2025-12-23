using ClaimsManagement.Business.Interfaces.IServices;
using ClaimsManagement.DataAccess.Interfaces.IRepositories;
using ClaimsManagement.DataAccess.Models;
using ClaimsManagement.DataAccess.Enum;
using Microsoft.Extensions.Logging;

namespace ClaimsManagement.Business.Services
{
    public class UserNotificationService : INotificationService
    {
        private readonly IUserNotificationRepository _notificationRepository;
        private readonly ILogger<UserNotificationService> _logger;

        public UserNotificationService(IUserNotificationRepository notificationRepository, ILogger<UserNotificationService> logger)
        {
            _notificationRepository = notificationRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<UserNotification>> GetUserNotificationsAsync(int userId)
        {
            try
            {
                return await _notificationRepository.GetByUserIdAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get notifications for user {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<UserNotification>> GetUnreadNotificationsAsync(int userId)
        {
            try
            {
                var notifications = await _notificationRepository.GetByUserIdAsync(userId);
                return notifications.Where(n => !n.IsRead);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get unread notifications for user {UserId}", userId);
                throw;
            }
        }

        public async Task MarkAsReadAsync(int notificationId, int userId)
        {
            try
            {
                var notification = await _notificationRepository.GetByIdAsync(notificationId);
                if (notification != null && notification.UserId == userId)
                {
                    notification.IsRead = true;
                    notification.ReadAt = DateTime.UtcNow;
                    await _notificationRepository.UpdateAsync(notification);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to mark notification {NotificationId} as read", notificationId);
                throw;
            }
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            try
            {
                var notifications = await _notificationRepository.GetByUserIdAsync(userId);
                var unreadNotifications = notifications.Where(n => !n.IsRead);
                
                foreach (var notification in unreadNotifications)
                {
                    notification.IsRead = true;
                    notification.ReadAt = DateTime.UtcNow;
                    await _notificationRepository.UpdateAsync(notification);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to mark all notifications as read for user {UserId}", userId);
                throw;
            }
        }

        public async Task DeleteNotificationAsync(int notificationId, int userId)
        {
            try
            {
                var notification = await _notificationRepository.GetByIdAsync(notificationId);
                if (notification != null && notification.UserId == userId)
                {
                    await _notificationRepository.DeleteAsync(notification);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete notification {NotificationId}", notificationId);
                throw;
            }
        }

        public async Task CreateNotificationAsync(int userId, string title, string message, NotificationType type)
        {
            await CreateNotificationAsync(userId, title, message, type, null);
        }

        public async Task CreateNotificationAsync(int userId, string title, string message, NotificationType type, int? relatedClaimId)
        {
            try
            {
                var notification = new UserNotification
                {
                    UserId = userId,
                    Title = title,
                    Message = message,
                    Type = type,
                    RelatedClaimId = relatedClaimId,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _notificationRepository.AddAsync(notification);
                _logger.LogInformation("Created notification for user {UserId}: {Title}", userId, title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create notification for user {UserId}", userId);
                throw;
            }
        }
    }
}