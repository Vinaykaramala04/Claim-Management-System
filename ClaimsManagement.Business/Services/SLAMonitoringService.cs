using ClaimsManagement.Business.Interfaces.IServices;
using ClaimsManagement.DataAccess.Interfaces.IRepositories;
using ClaimsManagement.DataAccess.Enum;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ClaimsManagement.Business.Services
{
    public class SLAMonitoringService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SLAMonitoringService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1); // Check every hour

        public SLAMonitoringService(IServiceProvider serviceProvider, ILogger<SLAMonitoringService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckSLABreaches();
                    await Task.Delay(_checkInterval, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while checking SLA breaches");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Wait 5 minutes before retry
                }
            }
        }

        private async Task CheckSLABreaches()
        {
            using var scope = _serviceProvider.CreateScope();
            var claimRepository = scope.ServiceProvider.GetRequiredService<IClaimRepository>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            try
            {
                var currentTime = DateTime.UtcNow;
                var allClaims = await claimRepository.GetAllAsync();
                
                var overdueClaims = allClaims.Where(c => 
                    c.SLADueDate.HasValue && 
                    c.SLADueDate < currentTime && 
                    !c.IsEscalated &&
                    c.Status != ClaimStatus.Paid && 
                    c.Status != ClaimStatus.Rejected &&
                    c.Status != ClaimStatus.Cancelled
                ).ToList();

                foreach (var claim in overdueClaims)
                {
                    // Mark as escalated to avoid duplicate notifications
                    claim.IsEscalated = true;
                    await claimRepository.UpdateAsync(claim);

                    // Notify claim owner
                    await notificationService.CreateNotificationAsync(
                        claim.UserId,
                        "SLA Breach Alert",
                        $"Your claim {claim.ClaimNumber} has exceeded the expected processing time. We are working to resolve this as soon as possible.",
                        NotificationType.SLABreach,
                        claim.ClaimId
                    );

                    _logger.LogWarning("SLA breach detected for claim {ClaimNumber}, due date was {SLADueDate}", 
                        claim.ClaimNumber, claim.SLADueDate);
                }

                if (overdueClaims.Any())
                {
                    _logger.LogInformation("Processed {Count} SLA breach notifications", overdueClaims.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check SLA breaches");
            }
        }
    }
}