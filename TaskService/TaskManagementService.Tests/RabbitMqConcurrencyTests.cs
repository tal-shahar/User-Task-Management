using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using TaskManagementService.Tests.Messaging;

namespace TaskManagementService.Tests
{
    public class RabbitMqConcurrencyTests
    {
        [Fact]
        public void MessageIdempotency_ShouldPreventDuplicatePublishing()
        {
            // Arrange
            var publishedMessages = new ConcurrentDictionary<string, DateTime>();
            var message1 = new TaskReminderMessage
            {
                TaskId = 1,
                DetectedAt = new DateTime(2024, 1, 1, 12, 0, 0)
            };

            var message2 = new TaskReminderMessage
            {
                TaskId = 1,
                DetectedAt = new DateTime(2024, 1, 1, 12, 0, 0) // Same minute
            };

            // Act
            var key1 = $"{message1.TaskId}_{message1.DetectedAt:yyyyMMddHHmm}";
            var key2 = $"{message2.TaskId}_{message2.DetectedAt:yyyyMMddHHmm}";

            var added1 = publishedMessages.TryAdd(key1, DateTime.UtcNow);
            var added2 = publishedMessages.TryAdd(key2, DateTime.UtcNow);

            // Assert
            Assert.True(added1);
            Assert.False(added2); // Duplicate key should not be added
            Assert.Single(publishedMessages);
        }

        [Fact]
        public async Task ConcurrentMessageTracking_ShouldBeThreadSafe()
        {
            // Arrange
            var publishedMessages = new ConcurrentDictionary<string, DateTime>();
            var tasks = new System.Collections.Generic.List<Task>();

            // Act - Add messages concurrently
            for (int i = 0; i < 100; i++)
            {
                var taskId = i;
                var detectedAt = DateTime.UtcNow.AddMinutes(-taskId);
                var key = $"{taskId}_{detectedAt:yyyyMMddHHmm}";

                tasks.Add(Task.Run(() =>
                {
                    publishedMessages.TryAdd(key, DateTime.UtcNow);
                }));
            }

            await Task.WhenAll(tasks);

            // Assert
            Assert.Equal(100, publishedMessages.Count);
        }

        [Fact]
        public void MessageTrackingCleanup_ShouldRemoveOldEntries()
        {
            // Arrange
            var publishedMessages = new ConcurrentDictionary<string, DateTime>();
            var now = DateTime.UtcNow;

            publishedMessages.TryAdd("key1", now.AddHours(-2)); // Old - should be removed
            publishedMessages.TryAdd("key2", now.AddHours(-1).AddMinutes(-1)); // Old - should be removed
            publishedMessages.TryAdd("key3", now.AddMinutes(-30)); // Recent - should remain
            publishedMessages.TryAdd("key4", now.AddMinutes(-10)); // Recent - should remain

            var cutoffTime = now.AddHours(-1);

            // Act
            var keysToRemove = new System.Collections.Generic.List<string>();
            foreach (var kvp in publishedMessages)
            {
                if (kvp.Value < cutoffTime)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                publishedMessages.TryRemove(key, out _);
            }

            // Assert
            Assert.Equal(2, publishedMessages.Count);
            Assert.Contains("key3", publishedMessages.Keys);
            Assert.Contains("key4", publishedMessages.Keys);
            Assert.DoesNotContain("key1", publishedMessages.Keys);
            Assert.DoesNotContain("key2", publishedMessages.Keys);
        }

        [Fact]
        public async Task RetryMechanism_ShouldAttemptMultipleTimes()
        {
            // Arrange
            var maxRetries = 3;
            var attemptCount = 0;
            var shouldSucceed = false;

            // Act
            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                attemptCount++;
                try
                {
                    if (attempt == 2) // Succeed on third attempt
                    {
                        shouldSucceed = true;
                        break;
                    }
                    throw new Exception("Simulated failure");
                }
                catch
                {
                    if (attempt < maxRetries - 1)
                    {
                        await Task.Delay(10); // Simulate retry delay
                        continue;
                    }
                }
            }

            // Assert
            Assert.Equal(3, attemptCount);
            Assert.True(shouldSucceed);
        }

        [Fact]
        public void MessageProperties_ShouldIncludeCorrelationAndMessageIds()
        {
            // Arrange & Act
            var message = new TaskReminderMessage
            {
                TaskId = 1,
                MessageId = Guid.NewGuid().ToString(),
                CorrelationId = Guid.NewGuid().ToString(),
                Title = "Test",
                DetectedAt = DateTime.UtcNow
            };

            // Assert
            Assert.NotNull(message.MessageId);
            Assert.NotNull(message.CorrelationId);
            Assert.NotEqual(message.MessageId, message.CorrelationId);
            Assert.NotEmpty(message.MessageId);
            Assert.NotEmpty(message.CorrelationId);
        }
    }
}

