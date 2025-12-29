using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using TaskManagementService.Tests.Models;
using TaskManagementService.Tests.Messaging;

namespace TaskManagementService.Tests
{
    public class OverdueTaskDetectionTests
    {
        [Fact]
        public void IsTaskOverdue_WithPastDueDate_ShouldReturnTrue()
        {
            // Arrange
            var task = new TaskModel
            {
                Id = 1,
                Title = "Overdue Task",
                DueDate = DateTime.UtcNow.AddDays(-1), // Due yesterday
                Priority = Priority.High
            };

            // Act
            var isOverdue = task.DueDate < DateTime.UtcNow;

            // Assert
            Assert.True(isOverdue);
        }

        [Fact]
        public void IsTaskOverdue_WithFutureDueDate_ShouldReturnFalse()
        {
            // Arrange
            var task = new TaskModel
            {
                Id = 1,
                Title = "Future Task",
                DueDate = DateTime.UtcNow.AddDays(1), // Due tomorrow
                Priority = Priority.Medium
            };

            // Act
            var isOverdue = task.DueDate < DateTime.UtcNow;

            // Assert
            Assert.False(isOverdue);
        }

        [Fact]
        public void CreateReminderMessage_FromTask_ShouldMapAllProperties()
        {
            // Arrange
            var task = new TaskModel
            {
                Id = 1,
                Title = "Test Task",
                Description = "Test Description",
                DueDate = DateTime.UtcNow.AddDays(-1),
                Priority = Priority.High,
                UserFullName = "John Doe",
                UserEmail = "john@example.com"
            };

            // Act
            var reminder = new TaskReminderMessage
            {
                TaskId = task.Id,
                Title = task.Title,
                Description = task.Description,
                DueDate = task.DueDate,
                Priority = task.Priority.ToString(),
                UserFullName = task.UserFullName,
                UserEmail = task.UserEmail,
                DetectedAt = DateTime.UtcNow
            };

            // Assert
            Assert.Equal(task.Id, reminder.TaskId);
            Assert.Equal(task.Title, reminder.Title);
            Assert.Equal(task.Description, reminder.Description);
            Assert.Equal(task.DueDate, reminder.DueDate);
            Assert.Equal(task.Priority.ToString(), reminder.Priority);
            Assert.Equal(task.UserFullName, reminder.UserFullName);
            Assert.Equal(task.UserEmail, reminder.UserEmail);
        }

        [Fact]
        public void FilterOverdueTasks_ShouldOnlyReturnPastDueTasks()
        {
            // Arrange
            var tasks = new List<TaskModel>
            {
                new TaskModel { Id = 1, DueDate = DateTime.UtcNow.AddDays(-1) }, // Overdue
                new TaskModel { Id = 2, DueDate = DateTime.UtcNow.AddDays(1) },  // Not overdue
                new TaskModel { Id = 3, DueDate = DateTime.UtcNow.AddDays(-2) },  // Overdue
                new TaskModel { Id = 4, DueDate = DateTime.UtcNow.AddHours(1) }    // Not overdue
            };

            var currentDate = DateTime.UtcNow;

            // Act
            var overdueTasks = tasks.FindAll(t => t.DueDate < currentDate);

            // Assert
            Assert.Equal(2, overdueTasks.Count);
            Assert.Contains(overdueTasks, t => t.Id == 1);
            Assert.Contains(overdueTasks, t => t.Id == 3);
            Assert.DoesNotContain(overdueTasks, t => t.Id == 2);
            Assert.DoesNotContain(overdueTasks, t => t.Id == 4);
        }

        [Fact]
        public void PriorityEnum_ShouldHaveCorrectValues()
        {
            // Assert
            Assert.Equal(1, (int)Priority.Low);
            Assert.Equal(2, (int)Priority.Medium);
            Assert.Equal(3, (int)Priority.High);
        }
    }
}

