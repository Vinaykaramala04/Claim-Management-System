using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ClaimsManagement.DataAccess.Interfaces.IRepositories;
using ClaimsManagement.DataAccess.Enum;
using ClaimsManagement.Business.Interfaces.IServices;

namespace ClaimsManagement.Business.Services
{
    public class ClaimEscalationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ClaimEscalationService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1); // Check every hour

        public ClaimEscalationService(IServiceProvider serviceProvider, ILogger<ClaimEscalationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Claim Escalation Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessEscalationsAsync();
                    await Task.Delay(_checkInterval, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing claim escalations");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Wait 5 minutes before retry
                }
            }

            _logger.LogInformation("Claim Escalation Service stopped");
        }

        private async Task ProcessEscalationsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var claimRepository = scope.ServiceProvider.GetRequiredService<IClaimRepository>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            try
            {
                // Get claims that have breached SLA
                var currentTime = DateTime.UtcNow;
                var overdueClaims = await claimRepository.GetClaimsBySLAAsync(currentTime);

                foreach (var claim in overdueClaims)
                {
                    if (!claim.IsEscalated)
                    {
                        // Mark claim as escalated
                        claim.IsEscalated = true;
                        await claimRepository.UpdateAsync(claim);

                        // Send escalation notification
                        await notificationService.CreateNotificationAsync(
                            claim.UserId,
                            "SLA Breach",
                            $"Your claim {claim.ClaimNumber} has breached SLA deadline",
                            NotificationType.SLABreach);

                        _logger.LogInformation($"Escalated claim {claim.ClaimNumber} due to SLA breach");
                    }
                }

                // Check for claims approaching SLA deadline (within 24 hours)
                var approachingDeadline = DateTime.UtcNow.AddHours(24);
                var warningClaims = await claimRepository.FindAsync(c => 
                    c.SLADueDate <= approachingDeadline && 
                    c.SLADueDate > currentTime &&
                    !c.IsEscalated &&
                    c.Status != ClaimStatus.Paid &&
                    c.Status != ClaimStatus.Rejected);

                foreach (var claim in warningClaims)
                {
                    // Send warning notification
                    await notificationService.CreateNotificationAsync(
                        claim.UserId,
                        "SLA Warning",
                        $"Your claim {claim.ClaimNumber} is approaching SLA deadline",
                        NotificationType.SLABreach);

                    _logger.LogInformation($"Sent SLA warning for claim {claim.ClaimNumber}");
                }

                _logger.LogInformation($"Processed escalations: {overdueClaims.Count()} escalated, {warningClaims.Count()} warnings sent");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing claim escalations");
                throw;
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Claim Escalation Service is stopping");
            await base.StopAsync(stoppingToken);
        }
    }
}