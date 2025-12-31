using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.Data;
using TaskManagementAPI.Models;
using TaskManagementAPI.Repositories;
using TaskManagementAPI.Tests.Helpers;
using ModelsTask = TaskManagementAPI.Models.Task;

namespace TaskManagementAPI.Tests.Repositories;

public class TaskRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly TaskRepository _repository;
    private readonly User _testUser;

    public TaskRepositoryTests()
    {
        _context = TestDbContextFactory.CreateInMemoryDbContext();
        _repository = new TaskRepository(_context);

        // Create a test user
        _testUser = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            FullName = "Test User",
            Role = Role.User,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(_testUser);
        _context.SaveChanges();
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateAsync_ShouldSaveTaskWithAllRequiredFields()
    {
        // Arrange
        var task = new ModelsTask
        {
            Title = "Test Task",
            Description = "Test Description",
            DueDate = DateTime.UtcNow.AddDays(7),
            Priority = Priority.Medium,
            UserId = _testUser.Id,
            UserFullName = _testUser.FullName,
            UserEmail = _testUser.Email,
            UserTelephone = string.Empty // Required field
        };

        // Act
        var result = await _repository.CreateAsync(task);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        
        // Verify all required fields are saved
        var savedTask = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == result.Id);
        Assert.NotNull(savedTask);
        Assert.NotNull(savedTask.Title);
        Assert.NotNull(savedTask.Description);
        Assert.NotNull(savedTask.UserEmail);
        Assert.NotNull(savedTask.UserFullName);
        Assert.NotNull(savedTask.UserTelephone); // Critical field
        Assert.NotEqual(default(DateTime), savedTask.CreatedAt);
        Assert.NotEqual(default(DateTime), savedTask.UpdatedAt);
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateAsync_ShouldSetCreatedAtAndUpdatedAt()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;
        var task = new ModelsTask
        {
            Title = "Test Task",
            Description = "Test Description",
            DueDate = DateTime.UtcNow.AddDays(7),
            Priority = Priority.Low,
            UserId = _testUser.Id,
            UserFullName = _testUser.FullName,
            UserEmail = _testUser.Email,
            UserTelephone = string.Empty
        };

        // Act
        var result = await _repository.CreateAsync(task);
        var afterCreation = DateTime.UtcNow;

        // Assert
        Assert.True(result.CreatedAt >= beforeCreation && result.CreatedAt <= afterCreation);
        Assert.True(result.UpdatedAt >= beforeCreation && result.UpdatedAt <= afterCreation);
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateAsync_ShouldNotAllowNullRequiredFields()
    {
        // Arrange
        var task = new ModelsTask
        {
            Title = "Test Task",
            Description = "Test Description",
            DueDate = DateTime.UtcNow.AddDays(7),
            Priority = Priority.Medium,
            UserId = _testUser.Id,
            UserFullName = _testUser.FullName,
            UserEmail = _testUser.Email,
            UserTelephone = string.Empty // Must be set, even if empty
        };

        // Act
        var result = await _repository.CreateAsync(task);

        // Assert - Verify no null values
        var savedTask = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == result.Id);
        Assert.NotNull(savedTask);
        
        // All required string fields must not be null
        Assert.NotNull(savedTask.Title);
        Assert.NotNull(savedTask.Description);
        Assert.NotNull(savedTask.UserEmail);
        Assert.NotNull(savedTask.UserFullName);
        Assert.NotNull(savedTask.UserTelephone); // This was the issue
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}

