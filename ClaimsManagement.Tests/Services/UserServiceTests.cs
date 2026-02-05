using Xunit;
using Moq;
using FluentAssertions;
using ClaimsManagement.Business.Services;
using ClaimsManagement.DataAccess.Interfaces.IRepositories;
using ClaimsManagement.DataAccess.Models;
using ClaimsManagement.DataAccess.Enum;
using ClaimsManagement.Business.DTOs.User;

namespace ClaimsManagement.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _userService = new UserService(_mockUserRepository.Object);
        }

        [Fact]
        public async Task CreateUserAsync_ValidRequest_ShouldCreateUserSuccessfully()
        {
            // Arrange
            var request = new UserCreateDto
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Password = "password123",
                Role = UserRole.Employee,
                DepartmentId = 1,
                PhoneNumber = "1234567890"
            };

            var expectedUser = new User
            {
                UserId = 1,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Role = request.Role,
                DepartmentId = request.DepartmentId,
                PhoneNumber = request.PhoneNumber,
                IsActive = true
            };

            _mockUserRepository.Setup(x => x.AddAsync(It.IsAny<User>()))
                .ReturnsAsync(expectedUser);

            // Act
            var result = await _userService.CreateUserAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.FirstName.Should().Be(request.FirstName);
            result.LastName.Should().Be(request.LastName);
            result.Email.Should().Be(request.Email);
            result.Role.Should().Be(request.Role);
            result.IsActive.Should().BeTrue();
            
            _mockUserRepository.Verify(x => x.AddAsync(It.Is<User>(u => 
                u.FirstName == request.FirstName &&
                u.LastName == request.LastName &&
                u.Email == request.Email &&
                u.Role == request.Role &&
                u.IsActive == true &&
                !string.IsNullOrEmpty(u.PasswordHash))), Times.Once);
        }

        [Fact]
        public async Task GetUserByIdAsync_ValidId_ShouldReturnUser()
        {
            // Arrange
            var userId = 1;
            var expectedUser = new User
            {
                UserId = userId,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Role = UserRole.Employee,
                IsActive = true
            };

            _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(expectedUser);

            // Act
            var result = await _userService.GetUserByIdAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result!.UserId.Should().Be(userId);
            result.FirstName.Should().Be(expectedUser.FirstName);
            result.LastName.Should().Be(expectedUser.LastName);
            result.Email.Should().Be(expectedUser.Email);
        }

        [Fact]
        public async Task GetUserByIdAsync_NonExistentId_ShouldReturnNull()
        {
            // Arrange
            var userId = 999;
            _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _userService.GetUserByIdAsync(userId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetUserByEmailAsync_ValidEmail_ShouldReturnUser()
        {
            // Arrange
            var email = "john.doe@example.com";
            var expectedUser = new User
            {
                UserId = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = email,
                Role = UserRole.Employee,
                IsActive = true
            };

            _mockUserRepository.Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync(expectedUser);

            // Act
            var result = await _userService.GetUserByEmailAsync(email);

            // Assert
            result.Should().NotBeNull();
            result!.Email.Should().Be(email);
            result.FirstName.Should().Be(expectedUser.FirstName);
            result.LastName.Should().Be(expectedUser.LastName);
        }

        [Fact]
        public async Task GetUserByEmailAsync_NonExistentEmail_ShouldReturnNull()
        {
            // Arrange
            var email = "nonexistent@example.com";
            _mockUserRepository.Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _userService.GetUserByEmailAsync(email);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateUserAsync_ValidRequest_ShouldUpdateUserSuccessfully()
        {
            // Arrange
            var userId = 1;
            var existingUser = new User
            {
                UserId = userId,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Role = UserRole.Employee,
                DepartmentId = 1,
                PhoneNumber = "1234567890",
                IsActive = true
            };

            var updateRequest = new UserUpdateDto
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@example.com",
                Role = UserRole.Manager,
                DepartmentId = 2,
                PhoneNumber = "0987654321",
                IsActive = true
            };

            var updatedUser = new User
            {
                UserId = userId,
                FirstName = updateRequest.FirstName,
                LastName = updateRequest.LastName,
                Email = updateRequest.Email,
                Role = updateRequest.Role,
                DepartmentId = updateRequest.DepartmentId,
                PhoneNumber = updateRequest.PhoneNumber,
                IsActive = updateRequest.IsActive
            };

            _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(existingUser);
            _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(updatedUser);

            // Act
            var result = await _userService.UpdateUserAsync(userId, updateRequest);

            // Assert
            result.Should().NotBeNull();
            result.FirstName.Should().Be(updateRequest.FirstName);
            result.LastName.Should().Be(updateRequest.LastName);
            result.Email.Should().Be(updateRequest.Email);
            result.Role.Should().Be(updateRequest.Role);
            result.DepartmentId.Should().Be(updateRequest.DepartmentId);
            result.PhoneNumber.Should().Be(updateRequest.PhoneNumber);
            
            _mockUserRepository.Verify(x => x.UpdateAsync(It.Is<User>(u => 
                u.FirstName == updateRequest.FirstName &&
                u.LastName == updateRequest.LastName &&
                u.Email == updateRequest.Email &&
                u.Role == updateRequest.Role)), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAsync_NonExistentUser_ShouldThrowArgumentException()
        {
            // Arrange
            var userId = 999;
            var updateRequest = new UserUpdateDto
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@example.com",
                Role = UserRole.Manager,
                DepartmentId = 2,
                PhoneNumber = "0987654321",
                IsActive = true
            };

            _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync((User?)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _userService.UpdateUserAsync(userId, updateRequest));
        }

        [Fact]
        public async Task DeactivateUserAsync_ValidUser_ShouldDeactivateSuccessfully()
        {
            // Arrange
            var userId = 1;
            var existingUser = new User
            {
                UserId = userId,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                IsActive = true
            };

            _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(existingUser);
            _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(existingUser);

            // Act
            await _userService.DeactivateUserAsync(userId);

            // Assert
            _mockUserRepository.Verify(x => x.UpdateAsync(It.Is<User>(u => 
                u.UserId == userId && 
                u.IsActive == false)), Times.Once);
        }

        [Fact]
        public async Task DeactivateUserAsync_NonExistentUser_ShouldThrowArgumentException()
        {
            // Arrange
            var userId = 999;
            _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync((User?)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _userService.DeactivateUserAsync(userId));
        }

        [Fact]
        public async Task ValidatePasswordAsync_CorrectPassword_ShouldReturnTrue()
        {
            // Arrange
            var password = "password123";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            // Act
            var result = await _userService.ValidatePasswordAsync(password, hashedPassword);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ValidatePasswordAsync_IncorrectPassword_ShouldReturnFalse()
        {
            // Arrange
            var password = "password123";
            var wrongPassword = "wrongpassword";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            // Act
            var result = await _userService.ValidatePasswordAsync(wrongPassword, hashedPassword);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task HashPasswordAsync_ValidPassword_ShouldReturnHashedPassword()
        {
            // Arrange
            var password = "password123";

            // Act
            var result = await _userService.HashPasswordAsync(password);

            // Assert
            result.Should().NotBeNullOrEmpty();
            result.Should().NotBe(password); // Should be hashed, not plain text
            result.Length.Should().BeGreaterThan(password.Length); // Hashed password is longer
        }

        [Fact]
        public async Task GetAllUsersAsync_ShouldReturnAllUsers()
        {
            // Arrange
            var users = new List<User>
            {
                new User { UserId = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com" },
                new User { UserId = 2, FirstName = "Jane", LastName = "Smith", Email = "jane@example.com" }
            };

            _mockUserRepository.Setup(x => x.GetAllWithDepartmentAsync())
                .ReturnsAsync(users);

            // Act
            var result = await _userService.GetAllUsersAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().Contain(u => u.FirstName == "John");
            result.Should().Contain(u => u.FirstName == "Jane");
        }

        [Theory]
        [InlineData(UserRole.Employee)]
        [InlineData(UserRole.Agent)]
        [InlineData(UserRole.Manager)]
        [InlineData(UserRole.Admin)]
        public async Task CreateUserAsync_DifferentRoles_ShouldCreateWithCorrectRole(UserRole role)
        {
            // Arrange
            var request = new UserCreateDto
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
                Password = "password123",
                Role = role,
                DepartmentId = 1,
                PhoneNumber = "1234567890"
            };

            var expectedUser = new User
            {
                UserId = 1,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Role = role,
                IsActive = true
            };

            _mockUserRepository.Setup(x => x.AddAsync(It.IsAny<User>()))
                .ReturnsAsync(expectedUser);

            // Act
            var result = await _userService.CreateUserAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Role.Should().Be(role);
        }
    }
}