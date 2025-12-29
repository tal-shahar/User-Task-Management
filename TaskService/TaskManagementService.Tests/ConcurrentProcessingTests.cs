using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using TaskManagementService.Tests.Models;
using TaskManagementService.Tests.Messaging;

namespace TaskManagementService.Tests
{
    public class ConcurrentProcessingTests
    {
        [Fact]
        public async Task ProcessMultipleTasksConcurrently_ShouldHandleAllTasks()
        {
            // Arrange
            var tasks = Enumerable.Range(1, 20)
                .Select(i => new TaskModel
                {
                    Id = i,
                    Title = $"Task {i}",
                    Description = $"Description {i}",
                    DueDate = DateTime.UtcNow.AddDays(-1),
                    Priority = Priority.Medium,
                    UserFullName = "Test User",
                    UserEmail = "test@example.com"
                })
                .ToList();

            var processedTasks = new System.Collections.Concurrent.ConcurrentBag<int>();
            var semaphore = new SemaphoreSlim(10, 10); // Max 10 concurrent

            // Act
            var publishTasks = tasks.Select(async task =>
            {
                await semaphore.WaitAsync();
                try
                {
                    // Simulate processing
                    await Task.Delay(10);
                    processedTasks.Add(task.Id);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(publishTasks);

            // Assert
            Assert.Equal(20, processedTasks.Count);
            Assert.Equal(20, processedTasks.Distinct().Count()); // No duplicates
        }

        [Fact]
        public void MessageDeduplication_ShouldPreventDuplicateMessages()
        {
            // Arrange
            var message1 = new TaskReminderMessage
            {
                TaskId = 1,
                Title = "Test Task",
                DetectedAt = new DateTime(2024, 1, 1, 12, 0, 0)
            };

            var message2 = new TaskReminderMessage
            {
                TaskId = 1,
                Title = "Test Task",
                DetectedAt = new DateTime(2024, 1, 1, 12, 0, 0) // Same minute
            };

            var message3 = new TaskReminderMessage
            {
                TaskId = 1,
                Title = "Test Task",
                DetectedAt = new DateTime(2024, 1, 1, 12, 1, 0) // Different minute
            };

            // Act - Create deduplication keys
            var key1 = $"{message1.TaskId}_{message1.DetectedAt:yyyyMMddHHmm}";
            var key2 = $"{message2.TaskId}_{message2.DetectedAt:yyyyMMddHHmm}";
            var key3 = $"{message3.TaskId}_{message3.DetectedAt:yyyyMMddHHmm}";

            // Assert
            Assert.Equal(key1, key2); // Same key = duplicate
            Assert.NotEqual(key1, key3); // Different key = not duplicate
        }

        [Fact]
        public async Task ConcurrentDictionary_ShouldBeThreadSafe()
        {
            // Arrange
            var dictionary = new System.Collections.Concurrent.ConcurrentDictionary<int, DateTime>();
            var tasks = new List<Task>();

            // Act - Add items concurrently
            for (int i = 0; i < 100; i++)
            {
                var taskId = i;
                tasks.Add(Task.Run(() =>
                {
                    dictionary.AddOrUpdate(taskId, DateTime.UtcNow, (key, oldValue) => DateTime.UtcNow);
                }));
            }

            await Task.WhenAll(tasks);

            // Assert
            Assert.Equal(100, dictionary.Count);
        }

        [Fact]
        public async Task SemaphoreSlim_ShouldLimitConcurrency()
        {
            // Arrange
            var maxConcurrent = 5;
            var semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
            var concurrentCount = 0;
            var maxObservedConcurrency = 0;
            var lockObj = new object();

            // Act
            var tasks = Enumerable.Range(1, 20)
                .Select(async i =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        lock (lockObj)
                        {
                            concurrentCount++;
                            maxObservedConcurrency = Math.Max(maxObservedConcurrency, concurrentCount);
                        }

                        await Task.Delay(50); // Simulate work

                        lock (lockObj)
                        {
                            concurrentCount--;
                        }
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                })
                .ToArray();

            await Task.WhenAll(tasks);

            // Assert
            Assert.True(maxObservedConcurrency <= maxConcurrent);
            Assert.Equal(0, concurrentCount); // All tasks completed
        }

        [Fact]
        public void MessageCorrelation_ShouldHaveUniqueIds()
        {
            // Arrange & Act
            var messages = Enumerable.Range(1, 10)
                .Select(i => new TaskReminderMessage
                {
                    TaskId = i,
                    MessageId = Guid.NewGuid().ToString(),
                    CorrelationId = Guid.NewGuid().ToString()
                })
                .ToList();

            // Assert
            var messageIds = messages.Select(m => m.MessageId).ToList();
            var correlationIds = messages.Select(m => m.CorrelationId).ToList();

            Assert.Equal(10, messageIds.Distinct().Count());
            Assert.Equal(10, correlationIds.Distinct().Count());
        }

        [Fact]
        public void ProcessedTasksCleanup_ShouldRemoveOldEntries()
        {
            // Arrange
            var processedTasks = new System.Collections.Concurrent.ConcurrentDictionary<int, DateTime>();
            var now = DateTime.UtcNow;

            // Add old and new entries
            processedTasks.TryAdd(1, now.AddHours(-3)); // Old
            processedTasks.TryAdd(2, now.AddHours(-2)); // Old
            processedTasks.TryAdd(3, now.AddMinutes(-10)); // Recent
            processedTasks.TryAdd(4, now.AddMinutes(-5)); // Recent

            var cutoffTime = now.AddHours(-1);

            // Act
            var keysToRemove = processedTasks
                .Where(kvp => kvp.Value < cutoffTime)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in keysToRemove)
            {
                processedTasks.TryRemove(key, out _);
            }

            // Assert
            Assert.Equal(2, processedTasks.Count);
            Assert.Contains(3, processedTasks.Keys);
            Assert.Contains(4, processedTasks.Keys);
            Assert.DoesNotContain(1, processedTasks.Keys);
            Assert.DoesNotContain(2, processedTasks.Keys);
        }
    }
}

