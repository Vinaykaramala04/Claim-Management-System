using Microsoft.Extensions.DependencyInjection;
using ClaimsManagement.Business.Interfaces.IServices;
using ClaimsManagement.Business.Services;
using ClaimsManagement.DataAccess.Interfaces.IRepositories;
using ClaimsManagement.DataAccess.Repositories;

namespace ClaimsManagement.Business.Services
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddBusinessServices(this IServiceCollection services)
        {
            // Register Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IClaimRepository, ClaimRepository>();
            services.AddScoped<IClaimDocumentRepository, ClaimDocumentRepository>();
            services.AddScoped<IClaimApprovalRepository, ClaimApprovalRepository>();
            services.AddScoped<IClaimCommentRepository, ClaimCommentRepository>();
            services.AddScoped<IExpenseCategoryRepository, ExpenseCategoryRepository>();
            services.AddScoped<IDepartmentRepository, DepartmentRepository>();
            services.AddScoped<IUserNotificationRepository, UserNotificationRepository>();

            // Register Services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IClaimService, ClaimService>();
            services.AddScoped<IClaimDocumentService, ClaimDocumentService>();
            services.AddScoped<IClaimApprovalService, ClaimApprovalService>();
            services.AddScoped<IClaimCommentService, ClaimCommentService>();
            services.AddScoped<IExpenseCategoryService, ExpenseCategoryService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<INotificationService, UserNotificationService>();
            services.AddScoped<IEmailNotificationService, EmailNotificationService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IAuditService, AuditService>();
            services.AddScoped<IDocumentValidationService, DocumentValidationService>();
            
            // Register Background Services
            services.AddHostedService<SLAMonitoringService>();

            return services;
        }
    }
}