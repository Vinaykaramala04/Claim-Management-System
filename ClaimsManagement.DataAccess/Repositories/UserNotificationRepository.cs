using Microsoft.EntityFrameworkCore;
using ClaimsManagement.DataAccess.Data;
using ClaimsManagement.DataAccess.Models;
using ClaimsManagement.DataAccess.Interfaces.IRepositories;

namespace ClaimsManagement.DataAccess.Repositories
{
    public class UserNotificationRepository : BaseRepository<UserNotification>, IUserNotificationRepository
    {
        public UserNotificationRepository(ClaimsManagementDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<UserNotification>> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                .Include(un => un.RelatedClaim)
                .Where(un => un.UserId == userId)
                .OrderByDescending(un => un.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserNotification>> GetUnreadByUserIdAsync(int userId)
        {
            return await _dbSet
                .Include(un => un.RelatedClaim)
                .Where(un => un.UserId == userId && !un.IsRead)
                .OrderByDescending(un => un.CreatedAt)
                .ToListAsync();
        }

        public async Task MarkAsReadAsync(int notificationId, int userId)
        {
            var notification = await _dbSet
                .FirstOrDefaultAsync(un => un.NotificationId == notificationId && un.UserId == userId);
            
            if (notification != null)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            var notifications = await _dbSet
                .Where(un => un.UserId == userId && !un.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
    }
}