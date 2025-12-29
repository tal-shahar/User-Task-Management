using System;
using Xunit;
using Newtonsoft.Json;
using TaskManagementService.Tests.Messaging;

namespace TaskManagementService.Tests
{
    public class MessageSerializationTests
    {
        [Fact]
        public void SerializeTaskReminderMessage_ShouldProduceValidJson()
        {
            // Arrange
            var message = new TaskReminderMessage
            {
                TaskId = 1,
                Title = "Test Task",
                Description = "Test Description",
                DueDate = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc),
                Priority = "High",
                UserFullName = "John Doe",
                UserEmail = "john@example.com",
                DetectedAt = DateTime.UtcNow
            };

            // Act
            var json = JsonConvert.SerializeObject(message);
            var deserialized = JsonConvert.DeserializeObject<TaskReminderMessage>(json);

            // Assert
            Assert.NotNull(json);
            Assert.NotNull(deserialized);
            Assert.Equal(message.TaskId, deserialized.TaskId);
            Assert.Equal(message.Title, deserialized.Title);
            Assert.Equal(message.Description, deserialized.Description);
            Assert.Equal(message.DueDate, deserialized.DueDate);
            Assert.Equal(message.Priority, deserialized.Priority);
            Assert.Equal(message.UserFullName, deserialized.UserFullName);
            Assert.Equal(message.UserEmail, deserialized.UserEmail);
        }

        [Fact]
        public void SerializeTaskReminderMessage_ShouldIncludeAllProperties()
        {
            // Arrange
            var message = new TaskReminderMessage
            {
                TaskId = 123,
                Title = "Complete Project",
                Description = "Finish the task management application",
                DueDate = DateTime.UtcNow.AddDays(-1),
                Priority = "Medium",
                UserFullName = "Jane Smith",
                UserEmail = "jane@example.com",
                DetectedAt = DateTime.UtcNow
            };

            // Act
            var json = JsonConvert.SerializeObject(message);

            // Assert
            Assert.Contains("\"TaskId\":123", json);
            Assert.Contains("\"Title\":\"Complete Project\"", json);
            Assert.Contains("\"Description\":\"Finish the task management application\"", json);
            Assert.Contains("\"Priority\":\"Medium\"", json);
            Assert.Contains("\"UserFullName\":\"Jane Smith\"", json);
            Assert.Contains("\"UserEmail\":\"jane@example.com\"", json);
        }

        [Fact]
        public void DeserializeTaskReminderMessage_FromValidJson_ShouldCreateObject()
        {
            // Arrange
            var json = @"{
                ""TaskId"": 1,
                ""Title"": ""Test"",
                ""Description"": ""Test Desc"",
                ""DueDate"": ""2024-01-01T12:00:00Z"",
                ""Priority"": ""Low"",
                ""UserFullName"": ""Test User"",
                ""UserEmail"": ""test@example.com"",
                ""DetectedAt"": ""2024-01-01T12:00:00Z""
            }";

            // Act
            var message = JsonConvert.DeserializeObject<TaskReminderMessage>(json);

            // Assert
            Assert.NotNull(message);
            Assert.Equal(1, message.TaskId);
            Assert.Equal("Test", message.Title);
            Assert.Equal("Test Desc", message.Description);
            Assert.Equal("Low", message.Priority);
            Assert.Equal("Test User", message.UserFullName);
            Assert.Equal("test@example.com", message.UserEmail);
        }
    }
}

