using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.Data;
using TaskManagementAPI.DTOs;
using TaskManagementAPI.Models;
using TaskManagementAPI.Repositories;
using TaskManagementAPI.Services;
using TaskManagementAPI.Tests.Helpers;
using BCrypt.Net;
using ModelsTask = TaskManagementAPI.Models.Task;

namespace TaskManagementAPI.Tests.Services;

public class TaskServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly TaskService _taskService;
    private readonly TaskRepository _taskRepository;
    private readonly User _testUser;

    public TaskServiceTests()
    {
        _context = TestDbContextFactory.CreateInMemoryDbContext();
        _taskRepository = new TaskRepository(_context);
        _taskService = new TaskService(_taskRepository, _context);

        // Create a test user
        _testUser = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            FullName = "Test User",
            Role = Role.User,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(_testUser);
        _context.SaveChanges();
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateTaskAsync_ShouldSetAllRequiredDatabaseFields()
    {
        // Arrange
        var createTaskDto = new CreateTaskDto
        {
            Title = "Test Task",
            Description = "Test Description",
            DueDate = DateTime.UtcNow.AddDays(7),
            Priority = PriorityDto.Medium
        };

        // Act
        var result = await _taskService.CreateTaskAsync(createTaskDto, _testUser.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Task", result.Title);
        Assert.Equal("Test Description", result.Description);
        Assert.Equal(_testUser.Email, result.UserEmail);
        Assert.Equal(_testUser.FullName, result.UserFullName);

        // Verify the task was saved to database with all required fields
        var savedTask = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == result.Id);
        Assert.NotNull(savedTask);
        Assert.NotNull(savedTask.Title);
        Assert.NotNull(savedTask.Description);
        Assert.NotNull(savedTask.UserEmail);
        Assert.NotNull(savedTask.UserFullName);
        Assert.NotNull(savedTask.UserTelephone); // This was the missing field that caused the error
        Assert.NotEqual(default(DateTime), savedTask.CreatedAt);
        Assert.NotEqual(default(DateTime), savedTask.UpdatedAt);
        Assert.True(savedTask.UserId > 0);
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateTaskAsync_ShouldNotAllowNullRequiredFields()
    {
        // Arrange
        var createTaskDto = new CreateTaskDto
        {
            Title = "Test Task",
            Description = "Test Description",
            DueDate = DateTime.UtcNow.AddDays(7),
            Priority = PriorityDto.Medium
        };

        // Act
        var result = await _taskService.CreateTaskAsync(createTaskDto, _testUser.Id);

        // Assert - Verify no null values in required fields
        var savedTask = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == result.Id);
        Assert.NotNull(savedTask);
        
        // Check all required string fields are not null
        Assert.NotNull(savedTask.Title);
        Assert.NotNull(savedTask.Description);
        Assert.NotNull(savedTask.UserEmail);
        Assert.NotNull(savedTask.UserFullName);
        Assert.NotNull(savedTask.UserTelephone); // Critical: This field must not be null
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateTaskAsync_ShouldHandleUserWithoutFullName()
    {
        // Arrange
        var userWithoutFullName = new User
        {
            Username = "nofullname",
            Email = "nofullname@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            FullName = string.Empty, // Empty full name
            Role = Role.User,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(userWithoutFullName);
        await _context.SaveChangesAsync();

        var createTaskDto = new CreateTaskDto
        {
            Title = "Test Task",
            Description = "Test Description",
            DueDate = DateTime.UtcNow.AddDays(7),
            Priority = PriorityDto.Low
        };

        // Act
        var result = await _taskService.CreateTaskAsync(createTaskDto, userWithoutFullName.Id);

        // Assert - Should use username as fallback
        var savedTask = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == result.Id);
        Assert.NotNull(savedTask);
        Assert.Equal(userWithoutFullName.Username, savedTask.UserFullName);
        Assert.NotNull(savedTask.UserTelephone); // Must still be set
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateTaskAsync_ShouldThrowExceptionWhenUserNotFound()
    {
        // Arrange
        var createTaskDto = new CreateTaskDto
        {
            Title = "Test Task",
            Description = "Test Description",
            DueDate = DateTime.UtcNow.AddDays(7),
            Priority = PriorityDto.Medium
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _taskService.CreateTaskAsync(createTaskDto, 99999)); // Non-existent user ID
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateTaskAsync_ShouldSetUserTelephoneField()
    {
        // Arrange
        var createTaskDto = new CreateTaskDto
        {
            Title = "Test Task",
            Description = "Test Description",
            DueDate = DateTime.UtcNow.AddDays(7),
            Priority = PriorityDto.High
        };

        // Act
        var result = await _taskService.CreateTaskAsync(createTaskDto, _testUser.Id);

        // Assert - Verify UserTelephone is set (even if empty string)
        var savedTask = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == result.Id);
        Assert.NotNull(savedTask);
        Assert.NotNull(savedTask.UserTelephone); // Must not be null
        // Can be empty string, but must not be null to satisfy database constraint
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}

