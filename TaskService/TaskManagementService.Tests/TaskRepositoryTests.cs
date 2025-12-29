using System;
using Xunit;
using TaskManagementService.Tests.Models;

namespace TaskManagementService.Tests
{
    public class TaskRepositoryTests
    {
        [Fact]
        public void TaskModel_ShouldHaveAllRequiredProperties()
        {
            // Arrange & Act
            var task = new TaskModel
            {
                Id = 1,
                Title = "Test Task",
                Description = "Test Description",
                DueDate = DateTime.UtcNow,
                Priority = Priority.High,
                UserId = 1,
                UserFullName = "John Doe",
                UserEmail = "john@example.com",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Assert
            Assert.Equal(1, task.Id);
            Assert.Equal("Test Task", task.Title);
            Assert.Equal("Test Description", task.Description);
            Assert.Equal(Priority.High, task.Priority);
            Assert.Equal(1, task.UserId);
            Assert.Equal("John Doe", task.UserFullName);
            Assert.Equal("john@example.com", task.UserEmail);
        }

        [Fact]
        public void TaskModel_WithNullUpdatedAt_ShouldBeValid()
        {
            // Arrange & Act
            var task = new TaskModel
            {
                Id = 1,
                Title = "Test Task",
                DueDate = DateTime.UtcNow,
                UpdatedAt = null
            };

            // Assert
            Assert.Null(task.UpdatedAt);
        }
    }
}

