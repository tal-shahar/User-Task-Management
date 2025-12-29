using System;
using Xunit;
using TaskManagementService.Tests.Messaging;

namespace TaskManagementService.Tests
{
    public class RabbitMqPublisherTests
    {
        [Fact]
        public void TaskReminderMessage_ShouldHaveAllRequiredProperties()
        {
            // Arrange & Act
            var message = new TaskReminderMessage
            {
                TaskId = 1,
                Title = "Test",
                Description = "Desc",
                DueDate = DateTime.UtcNow,
                Priority = "High",
                UserFullName = "User",
                UserEmail = "user@test.com",
                DetectedAt = DateTime.UtcNow
            };

            // Assert
            Assert.Equal(1, message.TaskId);
            Assert.Equal("Test", message.Title);
            Assert.Equal("Desc", message.Description);
            Assert.Equal("High", message.Priority);
            Assert.Equal("User", message.UserFullName);
            Assert.Equal("user@test.com", message.UserEmail);
        }

        [Fact]
        public void TaskReminderMessage_ShouldSerializeToJson()
        {
            // Arrange
            var message = new TaskReminderMessage
            {
                TaskId = 1,
                Title = "Test Task",
                Description = "Test Description",
                DueDate = DateTime.UtcNow,
                Priority = "High",
                UserFullName = "John Doe",
                UserEmail = "john@example.com",
                DetectedAt = DateTime.UtcNow
            };

            // Act - Test that message can be serialized (this is what RabbitMQ publisher does)
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(message);

            // Assert
            Assert.NotNull(json);
            Assert.Contains("Test Task", json);
            Assert.Contains("john@example.com", json);
            Assert.Contains("\"TaskId\":1", json);
        }
    }
}

