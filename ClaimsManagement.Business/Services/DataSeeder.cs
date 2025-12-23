using ClaimsManagement.DataAccess.Data;
using ClaimsManagement.DataAccess.Models;
using ClaimsManagement.DataAccess.Enum;
using Microsoft.EntityFrameworkCore;

namespace ClaimsManagement.Business.Services
{
    public class DataSeeder
    {
        private readonly ClaimsManagementDbContext _context;

        public DataSeeder(ClaimsManagementDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            // Ensure database is created
            await _context.Database.EnsureCreatedAsync();

            // Seed Departments
            if (!await _context.Departments.AnyAsync())
            {
                var departments = new[]
                {
                    new Department { Name = "Human Resources", Description = "HR Department", IsActive = true },
                    new Department { Name = "Finance", Description = "Finance Department", IsActive = true },
                    new Department { Name = "IT", Description = "Information Technology", IsActive = true },
                    new Department { Name = "Operations", Description = "Operations Department", IsActive = true }
                };

                await _context.Departments.AddRangeAsync(departments);
                await _context.SaveChangesAsync();
            }

            // Seed Expense Categories
            if (!await _context.ExpenseCategories.AnyAsync())
            {
                var categories = new[]
                {
                    new ExpenseCategory { Name = "Medical", Description = "Medical expenses", MaxAmount = 5000, RequiresApproval = true, IsActive = true },
                    new ExpenseCategory { Name = "Travel", Description = "Travel expenses", MaxAmount = 2000, RequiresApproval = true, IsActive = true },
                    new ExpenseCategory { Name = "Equipment", Description = "Equipment purchases", MaxAmount = 1000, RequiresApproval = true, IsActive = true },
                    new ExpenseCategory { Name = "Training", Description = "Training and development", MaxAmount = 3000, RequiresApproval = true, IsActive = true },
                    new ExpenseCategory { Name = "Miscellaneous", Description = "Other expenses", MaxAmount = 500, RequiresApproval = false, IsActive = true }
                };

                await _context.ExpenseCategories.AddRangeAsync(categories);
                await _context.SaveChangesAsync();
            }

            // Seed Users
            if (!await _context.Users.AnyAsync())
            {
                var hrDept = await _context.Departments.FirstAsync(d => d.Name == "Human Resources");
                var financeDept = await _context.Departments.FirstAsync(d => d.Name == "Finance");
                var itDept = await _context.Departments.FirstAsync(d => d.Name == "IT");

                var users = new[]
                {
                    new User 
                    { 
                        FirstName = "Admin", 
                        LastName = "User", 
                        Email = "admin@company.com", 
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"), 
                        Role = UserRole.Admin, 
                        DepartmentId = hrDept.DepartmentId,
                        IsActive = true 
                    },
                    new User 
                    { 
                        FirstName = "Manager", 
                        LastName = "Smith", 
                        Email = "manager@company.com", 
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("manager123"), 
                        Role = UserRole.Manager, 
                        DepartmentId = financeDept.DepartmentId,
                        IsActive = true 
                    },
                    new User 
                    { 
                        FirstName = "Agent", 
                        LastName = "Johnson", 
                        Email = "agent@company.com", 
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("agent123"), 
                        Role = UserRole.Agent, 
                        DepartmentId = hrDept.DepartmentId,
                        IsActive = true 
                    },
                    new User 
                    { 
                        FirstName = "John", 
                        LastName = "Doe", 
                        Email = "john.doe@company.com", 
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("employee123"), 
                        Role = UserRole.Employee, 
                        DepartmentId = itDept.DepartmentId,
                        IsActive = true 
                    }
                };

                await _context.Users.AddRangeAsync(users);
                await _context.SaveChangesAsync();
            }

            // Seed Sample Claims
            if (!await _context.Claims.AnyAsync())
            {
                var employee = await _context.Users.FirstAsync(u => u.Role == UserRole.Employee);
                var medicalCategory = await _context.ExpenseCategories.FirstAsync(c => c.Name == "Medical");
                var travelCategory = await _context.ExpenseCategories.FirstAsync(c => c.Name == "Travel");

                var claims = new[]
                {
                    new Claim
                    {
                        ClaimNumber = "CLM202412000001",
                        UserId = employee.UserId,
                        ClaimType = ClaimType.Medical,
                        CategoryId = medicalCategory.CategoryId,
                        Title = "Medical Checkup",
                        Description = "Annual medical checkup expenses",
                        Amount = 500.00m,
                        Status = ClaimStatus.Submitted,
                        Priority = Priority.Medium,
                        IncidentDate = DateTime.UtcNow.AddDays(-7),
                        SubmittedAt = DateTime.UtcNow.AddDays(-5),
                        SLADueDate = DateTime.UtcNow.AddDays(3)
                    },
                    new Claim
                    {
                        ClaimNumber = "CLM202412000002",
                        UserId = employee.UserId,
                        ClaimType = ClaimType.Travel,
                        CategoryId = travelCategory.CategoryId,
                        Title = "Business Trip",
                        Description = "Travel expenses for client meeting",
                        Amount = 1200.00m,
                        Status = ClaimStatus.UnderReview,
                        Priority = Priority.High,
                        IncidentDate = DateTime.UtcNow.AddDays(-10),
                        SubmittedAt = DateTime.UtcNow.AddDays(-8),
                        SLADueDate = DateTime.UtcNow.AddDays(1)
                    }
                };

                await _context.Claims.AddRangeAsync(claims);
                await _context.SaveChangesAsync();
            }
        }
    }
}