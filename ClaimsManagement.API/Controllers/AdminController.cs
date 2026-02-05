using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ClaimsManagement.Business.Interfaces.IServices;
using ClaimsManagement.DataAccess.Models;
using ClaimsManagement.DataAccess.Enum;
using ClaimsManagement.DataAccess.Interfaces.IRepositories;

namespace ClaimsManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IExpenseCategoryService _categoryService;
        private readonly IUserService _userService;
        private readonly IClaimRepository _claimRepository;
        private readonly IUserRepository _userRepository;

        public AdminController(IExpenseCategoryService categoryService, IUserService userService, IClaimRepository claimRepository, IUserRepository userRepository)
        {
            _categoryService = categoryService;
            _userService = userService;
            _claimRepository = claimRepository;
            _userRepository = userRepository;
        }

        [HttpPost("seed-categories")]
        public async Task<IActionResult> SeedCategories()
        {
            try
            {
                var categories = new[]
                {
                    new { Name = "Health Insurance", Description = "Medical expenses, doctor visits, prescriptions", MaxAmount = 10000m, RequiresApproval = true },
                    new { Name = "Auto Insurance", Description = "Vehicle accidents, repairs, towing", MaxAmount = 25000m, RequiresApproval = true },
                    new { Name = "Property Insurance", Description = "Home damage, theft, fire damage", MaxAmount = 50000m, RequiresApproval = true },
                    new { Name = "Travel Insurance", Description = "Trip cancellations, lost luggage, delays", MaxAmount = 5000m, RequiresApproval = false },
                    new { Name = "Life Insurance", Description = "Life insurance claims", MaxAmount = 100000m, RequiresApproval = true },
                    new { Name = "Disability Insurance", Description = "Disability and injury claims", MaxAmount = 15000m, RequiresApproval = true },
                    new { Name = "Office Supplies", Description = "General office supplies and equipment", MaxAmount = 500m, RequiresApproval = false },
                    new { Name = "Business Travel", Description = "Business travel expenses", MaxAmount = 2000m, RequiresApproval = false }
                };

                foreach (var cat in categories)
                {
                    await _categoryService.CreateCategoryAsync(cat.Name, cat.Description, cat.MaxAmount, cat.RequiresApproval);
                }

                return Ok(new { message = "Categories seeded successfully", count = categories.Length });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("seed-users")]
        public async Task<IActionResult> SeedUsers()
        {
            try
            {
                var users = new[]
                {
                    new { FirstName = "John", LastName = "Doe", Email = "john.doe@company.com", Role = UserRole.Employee, Phone = "555-0101" },
                    new { FirstName = "Jane", LastName = "Smith", Email = "jane.smith@company.com", Role = UserRole.Agent, Phone = "555-0102" },
                    new { FirstName = "Mike", LastName = "Johnson", Email = "mike.johnson@company.com", Role = UserRole.Manager, Phone = "555-0103" },
                    new { FirstName = "Sarah", LastName = "Wilson", Email = "sarah.wilson@company.com", Role = UserRole.Employee, Phone = "555-0104" },
                    new { FirstName = "David", LastName = "Brown", Email = "david.brown@company.com", Role = UserRole.Agent, Phone = "555-0105" },
                    new { FirstName = "Lisa", LastName = "Davis", Email = "lisa.davis@company.com", Role = UserRole.Employee, Phone = "555-0106" },
                    new { FirstName = "Tom", LastName = "Miller", Email = "tom.miller@company.com", Role = UserRole.Manager, Phone = "555-0107" },
                    new { FirstName = "Amy", LastName = "Garcia", Email = "amy.garcia@company.com", Role = UserRole.Employee, Phone = "555-0108" }
                };

                int createdCount = 0;
                foreach (var user in users)
                {
                    try
                    {
                        var createDto = new ClaimsManagement.Business.DTOs.User.UserCreateDto
                        {
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            Email = user.Email,
                            Password = "TempPass123!", // Default password
                            Role = user.Role,
                            PhoneNumber = user.Phone
                        };
                        
                        await _userService.CreateUserAsync(createDto);
                        createdCount++;
                    }
                    catch
                    {
                        
                        continue;
                    }
                }

                return Ok(new { message = "Users seeded successfully", created = createdCount, total = users.Length });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("system-stats")]
        public async Task<IActionResult> GetSystemStats()
        {
            try
            {
                
                var allClaims = await _claimRepository.GetAllAsync();
                var allUsers = await _userRepository.GetAllAsync();

                var totalClaims = allClaims.Count();
                var approvedClaims = allClaims.Count(c => c.Status == ClaimsManagement.DataAccess.Enum.ClaimStatus.Approved);
                var pendingClaims = allClaims.Count(c => c.Status == ClaimsManagement.DataAccess.Enum.ClaimStatus.Submitted || c.Status == ClaimsManagement.DataAccess.Enum.ClaimStatus.UnderReview);
                var rejectedClaims = allClaims.Count(c => c.Status == ClaimsManagement.DataAccess.Enum.ClaimStatus.Rejected);

                var stats = new
                {
                    totalClaims = totalClaims,
                    approvedClaims = approvedClaims,
                    pendingClaims = pendingClaims,
                    rejectedClaims = rejectedClaims,
                    totalAmount = allClaims.Sum(c => c.Amount),
                    approvedAmount = allClaims.Where(c => c.Status == ClaimsManagement.DataAccess.Enum.ClaimStatus.Approved).Sum(c => c.Amount),
                    totalUsers = allUsers.Count(),
                    claimsByCategory = allClaims.GroupBy(c => c.Category?.Name ?? "Unknown")
                        .Select(g => new {
                            category = g.Key,
                            count = g.Count(),
                            percentage = totalClaims > 0 ? Math.Round((double)g.Count() / totalClaims * 100, 1) : 0
                        }).ToList(),
                    claimsByStatus = new[] {
                        new { status = "Approved", count = approvedClaims, percentage = totalClaims > 0 ? Math.Round((double)approvedClaims / totalClaims * 100, 1) : 0 },
                        new { status = "Pending", count = pendingClaims, percentage = totalClaims > 0 ? Math.Round((double)pendingClaims / totalClaims * 100, 1) : 0 },
                        new { status = "Rejected", count = rejectedClaims, percentage = totalClaims > 0 ? Math.Round((double)rejectedClaims / totalClaims * 100, 1) : 0 }
                    },
                    monthlyTrends = allClaims.GroupBy(c => c.CreatedAt.ToString("yyyy-MM"))
                        .Select(g => new {
                            month = g.Key,
                            claims = g.Count(),
                            amount = g.Sum(c => c.Amount)
                        }).OrderBy(x => x.month).ToList(),
                    topUsers = allClaims.GroupBy(c => c.User)
                        .Where(g => g.Key != null)
                        .Select(g => new {
                            userName = $"{g.Key!.FirstName} {g.Key.LastName}",
                            claimCount = g.Count(),
                            totalAmount = g.Sum(c => c.Amount)
                        })
                        .OrderByDescending(x => x.totalAmount)
                        .Take(5)
                        .ToList()
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}