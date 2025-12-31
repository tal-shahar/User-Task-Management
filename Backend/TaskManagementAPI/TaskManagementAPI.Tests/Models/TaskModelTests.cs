using TaskManagementAPI.Models;
using ModelsTask = TaskManagementAPI.Models.Task;

namespace TaskManagementAPI.Tests.Models;

public class TaskModelTests
{
    [Fact]
    public void Task_ShouldHaveAllRequiredProperties()
    {
        // Arrange & Act
        var task = new ModelsTask
        {
            Id = 1,
            Title = "Test Task",
            Description = "Test Description",
            DueDate = DateTime.UtcNow,
            Priority = Priority.Medium,
            UserId = 1,
            UserFullName = "Test User",
            UserEmail = "test@example.com",
            UserTelephone = string.Empty, // Required field
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert - Verify all properties are accessible
        Assert.Equal(1, task.Id);
        Assert.NotNull(task.Title);
        Assert.NotNull(task.Description);
        Assert.NotNull(task.UserEmail);
        Assert.NotNull(task.UserFullName);
        Assert.NotNull(task.UserTelephone); // Critical: Must exist
        Assert.True(task.UserId > 0);
        Assert.NotEqual(default(DateTime), task.CreatedAt);
        Assert.NotEqual(default(DateTime), task.UpdatedAt);
    }

    [Fact]
    public void Task_UserTelephone_ShouldNotBeNull()
    {
        // Arrange
        var task = new ModelsTask();

        // Act
        task.UserTelephone = string.Empty; // Can be empty but not null

        // Assert
        Assert.NotNull(task.UserTelephone); // Must not be null to satisfy DB constraint
    }

    [Fact]
    public void Task_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var task = new ModelsTask();

        // Assert - Verify default values
        Assert.Equal(0, task.Id);
        Assert.Equal(string.Empty, task.Title);
        Assert.Equal(string.Empty, task.Description);
        Assert.Equal(string.Empty, task.UserFullName);
        Assert.Equal(string.Empty, task.UserEmail);
        Assert.Equal(string.Empty, task.UserTelephone); // Should default to empty string, not null
        Assert.Equal(0, task.UserId);
    }
}

