using ClaimsManagement.Business.Services;
using ClaimsManagement.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace ClaimsManagement.API.Extensions
{
    public static class DatabaseExtensions
    {
        public static async Task<IApplicationBuilder> UseDatabaseInitializationAsync(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                var context = services.GetRequiredService<ClaimsManagementDbContext>();
                var logger = services.GetRequiredService<ILogger<Program>>();

                // Ensure database is created
                await context.Database.EnsureCreatedAsync();
                logger.LogInformation("Database ensured created");

                // Run migrations if any
                if (context.Database.GetPendingMigrations().Any())
                {
                    await context.Database.MigrateAsync();
                    logger.LogInformation("Database migrations applied");
                }

                // Seed data
                var seeder = new DataSeeder(context);
                await seeder.SeedAsync();
                logger.LogInformation("Database seeding completed");
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while initializing the database");
                throw;
            }

            return app;
        }
    }
}