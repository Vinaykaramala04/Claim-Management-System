using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using ClaimsManagement.Business.Services;
using ClaimsManagement.Business.Interfaces.IServices;
using ClaimsManagement.Business.DTOs.Auth;
using ClaimsManagement.DataAccess.Models;
using ClaimsManagement.DataAccess.Enum;

namespace ClaimsManagement.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _mockUserService = new Mock<IUserService>();
            _mockConfiguration = new Mock<IConfiguration>();
            
            // Setup configuration mock
            var jwtSection = new Mock<IConfigurationSection>();
            jwtSection.Setup(x => x["SecretKey"]).Returns("ThisIsAVeryLongSecretKeyForJWTTokenGeneration123456789");
            jwtSection.Setup(x => x["Issuer"]).Returns("ClaimsManagementSystem");
            jwtSection.Setup(x => x["Audience"]).Returns("ClaimsManagementUsers");
            jwtSection.Setup(x => x["ExpiryInHours"]).Returns("24");
            
            _mockConfiguration.Setup(x => x.GetSection("JwtSettings")).Returns(jwtSection.Object);

            _authService = new AuthService(_mockUserService.Object, _mockConfiguration.Object);
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ShouldReturnLoginResponse()
        {
            // Arrange
            var request = new LoginRequestDto
            {
                Email = "test@example.com",
                Password = "password123"
            };

            var user = new User
            {
                UserId = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = request.Email,
                Role = UserRole.Employee,
                IsActive = true,
                PasswordHash = "hashedPassword"
            };

            _mockUserService.Setup(x => x.GetUserByEmailAsync(request.Email))
                .ReturnsAsync(user);
            _mockUserService.Setup(x => x.ValidatePasswordAsync(request.Password, user.PasswordHash))
                .ReturnsAsync(true);

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            result.Should().NotBeNull();
            result!.UserId.Should().Be(user.UserId);
            result.FirstName.Should().Be(user.FirstName);
            result.LastName.Should().Be(user.LastName);
            result.Email.Should().Be(user.Email);
            result.Role.Should().Be(user.Role);
            result.Token.Should().NotBeNullOrEmpty();
            result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        }

        [Fact]
        public async Task LoginAsync_InvalidEmail_ShouldReturnNull()
        {
            // Arrange
            var request = new LoginRequestDto
            {
                Email = "nonexistent@example.com",
                Password = "password123"
            };

            _mockUserService.Setup(x => x.GetUserByEmailAsync(request.Email))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task LoginAsync_InvalidPassword_ShouldReturnNull()
        {
            // Arrange
            var request = new LoginRequestDto
            {
                Email = "test@example.com",
                Password = "wrongpassword"
            };

            var user = new User
            {
                UserId = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = request.Email,
                Role = UserRole.Employee,
                IsActive = true,
                PasswordHash = "hashedPassword"
            };

            _mockUserService.Setup(x => x.GetUserByEmailAsync(request.Email))
                .ReturnsAsync(user);
            _mockUserService.Setup(x => x.ValidatePasswordAsync(request.Password, user.PasswordHash))
                .ReturnsAsync(false);

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task LoginAsync_InactiveUser_ShouldReturnNull()
        {
            // Arrange
            var request = new LoginRequestDto
            {
                Email = "test@example.com",
                Password = "password123"
            };

            var user = new User
            {
                UserId = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = request.Email,
                Role = UserRole.Employee,
                IsActive = false,
                PasswordHash = "hashedPassword"
            };

            _mockUserService.Setup(x => x.GetUserByEmailAsync(request.Email))
                .ReturnsAsync(user);

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GenerateJwtTokenAsync_ValidParameters_ShouldReturnToken()
        {
            // Arrange
            var userId = 1;
            var email = "test@example.com";
            var role = "Employee";

            // Act
            var result = await _authService.GenerateJwtTokenAsync(userId, email, role);

            // Assert
            result.Should().NotBeNullOrEmpty();
            result.Split('.').Should().HaveCount(3);
        }

        [Theory]
        [InlineData(UserRole.Employee)]
        [InlineData(UserRole.Agent)]
        [InlineData(UserRole.Manager)]
        [InlineData(UserRole.Admin)]
        public async Task LoginAsync_DifferentRoles_ShouldMapRoleCorrectly(UserRole userRole)
        {
            // Arrange
            var request = new LoginRequestDto
            {
                Email = "test@example.com",
                Password = "password123"
            };

            var user = new User
            {
                UserId = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = request.Email,
                Role = userRole,
                IsActive = true,
                PasswordHash = "hashedPassword"
            };

            _mockUserService.Setup(x => x.GetUserByEmailAsync(request.Email))
                .ReturnsAsync(user);
            _mockUserService.Setup(x => x.ValidatePasswordAsync(request.Password, user.PasswordHash))
                .ReturnsAsync(true);

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            result.Should().NotBeNull();
            result!.Role.Should().Be(userRole);
            result.Token.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task ValidateTokenAsync_ValidToken_ShouldReturnTrue()
        {
            // Arrange
            var userId = 1;
            var email = "test@example.com";
            var role = "Employee";
            var token = await _authService.GenerateJwtTokenAsync(userId, email, role);

            // Act
            var result = await _authService.ValidateTokenAsync(token);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ValidateTokenAsync_InvalidToken_ShouldReturnFalse()
        {
            // Arrange
            var invalidToken = "invalid.token.here";

            // Act
            var result = await _authService.ValidateTokenAsync(invalidToken);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ValidateTokenAsync_EmptyToken_ShouldReturnFalse()
        {
            // Arrange
            var emptyToken = "";

            // Act
            var result = await _authService.ValidateTokenAsync(emptyToken);

            // Assert
            result.Should().BeFalse();
        }
    }
}