using ClaimsManagement.Business.Interfaces.IServices;
using ClaimsManagement.Business.Services;
using ClaimsManagement.DataAccess.Interfaces.IRepositories;
using ClaimsManagement.DataAccess.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace ClaimsManagement.Business.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBusinessServices(this IServiceCollection services)
        {
            // Register repositories
            services.AddScoped<IClaimRepository, ClaimRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IExpenseCategoryRepository, ExpenseCategoryRepository>();
            services.AddScoped<IClaimApprovalRepository, ClaimApprovalRepository>();
            services.AddScoped<IClaimCommentRepository, ClaimCommentRepository>();
            services.AddScoped<IClaimDocumentRepository, ClaimDocumentRepository>();
            services.AddScoped<IUserNotificationRepository, UserNotificationRepository>();
            services.AddScoped<IDepartmentRepository, DepartmentRepository>();
            
            // Register business services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IClaimService, ClaimService>();
            services.AddScoped<IClaimApprovalService, ClaimApprovalService>();
            services.AddScoped<IClaimCommentService, ClaimCommentService>();
            services.AddScoped<IClaimDocumentService, ClaimDocumentService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IExpenseCategoryService, ExpenseCategoryService>();
            services.AddScoped<INotificationService, UserNotificationService>();
            services.AddScoped<IEmailNotificationService, EmailNotificationService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IAuditService, AuditService>();
            services.AddScoped<IDocumentValidationService, DocumentValidationService>();
            services.AddScoped<IChatbotService, ChatbotService>();
            
            return services;
        }
    }
}