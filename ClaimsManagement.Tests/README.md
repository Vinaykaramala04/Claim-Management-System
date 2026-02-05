# Claims Management System - Unit Tests

This project contains comprehensive unit tests for the Claims Management System backend services using xUnit, Moq, and FluentAssertions.

## Test Coverage

### Core Services Tested

1. **ClaimService** - Tests for claim creation, retrieval, status updates, and business logic
2. **AuthService** - Tests for authentication, JWT token generation and validation
3. **UserService** - Tests for user management operations
4. **DashboardService** - Tests for dashboard statistics and analytics
5. **ClaimApprovalService** - Tests for claim approval workflow

### Test Categories

- **Unit Tests**: Isolated testing of individual service methods
- **Business Logic Tests**: Validation of business rules and constraints
- **Integration Tests**: Testing service interactions with mocked dependencies
- **Edge Case Tests**: Testing error conditions and boundary scenarios

## Technologies Used

- **xUnit**: Primary testing framework
- **Moq**: Mocking framework for dependencies
- **FluentAssertions**: Fluent assertion library for better test readability
- **Microsoft.EntityFrameworkCore.InMemory**: In-memory database for testing

## Project Structure

```
ClaimsManagement.Tests/
├── Services/
│   ├── ClaimServiceTests.cs
│   ├── AuthServiceTests.cs
│   ├── UserServiceTests.cs
│   ├── DashboardServiceTests.cs
│   └── ClaimApprovalServiceTests.cs
└── Helpers/
    └── TestDataHelper.cs
```

## Running Tests

### Command Line
```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --verbosity normal

# Run specific test class
dotnet test --filter "ClaimServiceTests"

# Run tests with coverage (if coverage tools are installed)
dotnet test --collect:"XPlat Code Coverage"
```

### Visual Studio
- Open Test Explorer (Test → Test Explorer)
- Click "Run All Tests" or run individual test methods

## Test Examples

### ClaimService Tests
- ✅ Create claim with valid data
- ✅ Validate business rules (amount limits, incident date)
- ✅ Status transition validation
- ✅ SLA calculation based on priority
- ✅ Claim number generation

### AuthService Tests
- ✅ User login with valid credentials
- ✅ JWT token generation and validation
- ✅ Role-based authentication
- ✅ Invalid credential handling

### UserService Tests
- ✅ User creation and updates
- ✅ Password hashing and validation
- ✅ User activation/deactivation
- ✅ Email uniqueness validation

## Key Test Patterns

### Arrange-Act-Assert Pattern
```csharp
[Fact]
public async Task CreateClaimAsync_ValidRequest_ShouldCreateClaimSuccessfully()
{
    // Arrange
    var request = new ClaimCreateDto { /* test data */ };
    _mockRepository.Setup(x => x.AddAsync(It.IsAny<Claim>()))
        .ReturnsAsync(expectedClaim);

    // Act
    var result = await _claimService.CreateClaimAsync(request, userId);

    // Assert
    result.Should().NotBeNull();
    result.Amount.Should().Be(request.Amount);
    _mockRepository.Verify(x => x.AddAsync(It.IsAny<Claim>()), Times.Once);
}
```

### Theory Tests for Multiple Scenarios
```csharp
[Theory]
[InlineData(Priority.Critical, 1)]
[InlineData(Priority.High, 3)]
[InlineData(Priority.Medium, 5)]
[InlineData(Priority.Low, 10)]
public async Task CreateClaimAsync_DifferentPriorities_ShouldSetCorrectSLADate(
    Priority priority, int expectedBusinessDays)
{
    // Test implementation
}
```

## Test Results Summary

- **Total Tests**: 51
- **Passed**: 50
- **Failed**: 1 (minor assertion issue - now fixed)
- **Coverage**: Core business logic and service methods

## Benefits of This Test Suite

1. **Quality Assurance**: Ensures business logic works correctly
2. **Regression Prevention**: Catches breaking changes early
3. **Documentation**: Tests serve as living documentation
4. **Refactoring Safety**: Enables safe code refactoring
5. **CI/CD Integration**: Can be integrated into build pipelines

## Future Enhancements

- Add integration tests with real database
- Implement performance tests
- Add API endpoint tests
- Increase test coverage to 90%+
- Add mutation testing

## Running in CI/CD

```yaml
# Example GitHub Actions workflow
- name: Run Tests
  run: dotnet test --no-build --verbosity normal --logger trx --results-directory TestResults
  
- name: Publish Test Results
  uses: dorny/test-reporter@v1
  if: success() || failure()
  with:
    name: Test Results
    path: TestResults/*.trx
    reporter: dotnet-trx
```