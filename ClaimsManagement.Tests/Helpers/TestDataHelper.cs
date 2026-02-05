using ClaimsManagement.DataAccess.Models;
using ClaimsManagement.DataAccess.Enum;

namespace ClaimsManagement.Tests.Helpers
{
    public static class TestDataHelper
    {
        public static User CreateTestUser(int userId = 1, string email = "test@example.com", UserRole role = UserRole.Employee)
        {
            return new User
            {
                UserId = userId,
                FirstName = "Test",
                LastName = "User",
                Email = email,
                Role = role,
                IsActive = true,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                CreatedAt = DateTime.UtcNow,
                DepartmentId = 1,
                PhoneNumber = "1234567890"
            };
        }

        public static Claim CreateTestClaim(int claimId = 1, int userId = 1, ClaimStatus status = ClaimStatus.Submitted)
        {
            return new Claim
            {
                ClaimId = claimId,
                ClaimNumber = $"CLM202412240{claimId:D3}",
                UserId = userId,
                ClaimType = ClaimType.Medical,
                CategoryId = 1,
                Title = "Test Claim",
                Description = "Test Description",
                Amount = 100.00m,
                Status = status,
                Priority = Priority.Medium,
                IncidentDate = DateTime.UtcNow.AddDays(-1),
                SubmittedAt = DateTime.UtcNow,
                SLADueDate = DateTime.UtcNow.AddDays(5),
                CreatedAt = DateTime.UtcNow,
                Category = new ExpenseCategory { CategoryId = 1, Name = "Medical" },
                User = CreateTestUser(userId),
                Documents = new List<ClaimDocument>(),
                StatusHistory = new List<ClaimStatusHistory>(),
                Approvals = new List<ClaimApproval>()
            };
        }

        public static ExpenseCategory CreateTestCategory(int categoryId = 1, string name = "Medical")
        {
            return new ExpenseCategory
            {
                CategoryId = categoryId,
                Name = name,
                Description = $"{name} expenses",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
        }

        public static User CreateTestUserEntity(int userId = 1, string email = "test@example.com", UserRole role = UserRole.Employee)
        {
            return new User
            {
                UserId = userId,
                FirstName = "Test",
                LastName = "User",
                Email = email,
                Role = role,
                IsActive = true,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                CreatedAt = DateTime.UtcNow,
                DepartmentId = 1,
                PhoneNumber = "1234567890"
            };
        }
    }
}