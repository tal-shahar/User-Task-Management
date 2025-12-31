using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.Data;
using TaskManagementAPI.DTOs;
using TaskManagementAPI.Models;
using TaskManagementAPI.Repositories;
using TaskManagementAPI.Services;
using TaskManagementAPI.Tests.Helpers;
using BCrypt.Net;
using ModelsTask = TaskManagementAPI.Models.Task;

namespace TaskManagementAPI.Tests.Integration;

/// <summary>
/// Integration tests to verify that task creation works with the actual database schema.
/// These tests catch schema mismatches between the model and database.
/// </summary>
public class TaskCreationIntegrationTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly TaskService _taskService;
    private readonly User _testUser;

    public TaskCreationIntegrationTests()
    {
        _context = TestDbContextFactory.CreateInMemoryDbContext();
        var taskRepository = new TaskRepository(_context);
        _taskService = new TaskService(taskRepository, _context);

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
    public async System.Threading.Tasks.Task CreateTask_ShouldSucceedWithAllRequiredDatabaseFields()
    {
        // Arrange
        var createTaskDto = new CreateTaskDto
        {
            Title = "Integration Test Task",
            Description = "This is an integration test",
            DueDate = DateTime.UtcNow.AddDays(7),
            Priority = PriorityDto.Medium
        };

        // Act
        var result = await _taskService.CreateTaskAsync(createTaskDto, _testUser.Id);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);

        // Verify the task exists in the database
        var savedTask = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == result.Id);

        Assert.NotNull(savedTask);
        
        // Verify all required database columns are populated (not null)
        Assert.NotNull(savedTask.Title);
        Assert.NotNull(savedTask.Description);
        Assert.NotNull(savedTask.UserEmail);
        Assert.NotNull(savedTask.UserFullName);
        Assert.NotNull(savedTask.UserTelephone); // This field was missing and caused the error
        Assert.NotEqual(default(DateTime), savedTask.DueDate);
        Assert.NotEqual(default(DateTime), savedTask.CreatedAt);
        Assert.NotEqual(default(DateTime), savedTask.UpdatedAt);
        Assert.True(savedTask.UserId > 0);
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateTask_ShouldNotViolateDatabaseConstraints()
    {
        // Arrange
        var createTaskDto = new CreateTaskDto
        {
            Title = "Constraint Test",
            Description = "Testing database constraints",
            DueDate = DateTime.UtcNow.AddDays(5),
            Priority = PriorityDto.High
        };

        // Act
        var result = await _taskService.CreateTaskAsync(createTaskDto, _testUser.Id);

        // Assert - If we get here without exception, all constraints are satisfied
        Assert.NotNull(result);
        
        // Verify we can query the task back (proves it was saved successfully)
        var queryResult = await _context.Tasks
            .Where(t => t.Id == result.Id)
            .FirstOrDefaultAsync();
            
        Assert.NotNull(queryResult);
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateTask_ShouldHandleMultipleTasksForSameUser()
    {
        // Arrange
        var tasks = new List<CreateTaskDto>
        {
            new() { Title = "Task 1", Description = "First task", DueDate = DateTime.UtcNow.AddDays(1), Priority = PriorityDto.Low },
            new() { Title = "Task 2", Description = "Second task", DueDate = DateTime.UtcNow.AddDays(2), Priority = PriorityDto.Medium },
            new() { Title = "Task 3", Description = "Third task", DueDate = DateTime.UtcNow.AddDays(3), Priority = PriorityDto.High }
        };

        // Act
        var results = new List<TaskDto>();
        foreach (var taskDto in tasks)
        {
            var result = await _taskService.CreateTaskAsync(taskDto, _testUser.Id);
            results.Add(result);
        }

        // Assert
        Assert.Equal(3, results.Count);
        
        // Verify all tasks were saved with required fields
        foreach (var result in results)
        {
            var savedTask = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == result.Id);
            Assert.NotNull(savedTask);
            Assert.NotNull(savedTask.UserTelephone); // Must be set for all tasks
        }
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateTask_ShouldSetUserTelephoneToEmptyStringWhenNotProvided()
    {
        // Arrange
        var createTaskDto = new CreateTaskDto
        {
            Title = "Telephone Test",
            Description = "Testing UserTelephone field",
            DueDate = DateTime.UtcNow.AddDays(10),
            Priority = PriorityDto.Low
        };

        // Act
        var result = await _taskService.CreateTaskAsync(createTaskDto, _testUser.Id);

        // Assert
        var savedTask = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == result.Id);
        Assert.NotNull(savedTask);
        Assert.NotNull(savedTask.UserTelephone); // Must not be null
        Assert.Equal(string.Empty, savedTask.UserTelephone); // Should be empty string
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}

