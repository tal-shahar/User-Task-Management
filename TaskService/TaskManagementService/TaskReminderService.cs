using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using TaskManagementService.Data;
using TaskManagementService.Messaging;

namespace TaskManagementService
{
    public partial class TaskReminderService : ServiceBase
    {
        private Timer _timer;
        private readonly string _connectionString;
        private readonly string _rabbitMqHost;
        private readonly string _rabbitMqUsername;
        private readonly string _rabbitMqPassword;
        private readonly string _queueName;
        private readonly int _checkIntervalMinutes;
        private readonly int _maxConcurrentPublishes;
        private volatile bool _isRunning = false;
        private readonly SemaphoreSlim _processingSemaphore;
        private readonly object _timerLock = new object();
        private RabbitMqPublisher _rabbitMqPublisher;
        private readonly ConcurrentDictionary<int, DateTime> _processedTasks = new ConcurrentDictionary<int, DateTime>();

        public TaskReminderService()
        {
            InitializeComponent();
            ServiceName = "TaskManagementReminderService";

            // Read configuration from App.config
            _connectionString = ConfigurationManager.ConnectionStrings["TaskManagementDb"]?.ConnectionString
                ?? throw new InvalidOperationException("TaskManagementDb connection string not found in App.config");

            var rabbitMqConnection = ConfigurationManager.ConnectionStrings["RabbitMQ"]?.ConnectionString
                ?? throw new InvalidOperationException("RabbitMQ connection string not found in App.config");

            // Parse RabbitMQ connection string (format: host=localhost;port=5672;username=guest;password=guest)
            var rabbitMqParts = rabbitMqConnection.Split(';');
            _rabbitMqHost = GetValue(rabbitMqParts, "host") ?? "localhost";
            var port = GetValue(rabbitMqParts, "port") ?? "5672";
            _rabbitMqUsername = GetValue(rabbitMqParts, "username") ?? "guest";
            _rabbitMqPassword = GetValue(rabbitMqParts, "password") ?? "guest";

            _queueName = ConfigurationManager.AppSettings["QueueName"] ?? "task-reminders";
            _checkIntervalMinutes = int.Parse(ConfigurationManager.AppSettings["CheckIntervalMinutes"] ?? "5");
            _maxConcurrentPublishes = int.Parse(ConfigurationManager.AppSettings["MaxConcurrentPublishes"] ?? "10");
            
            _processingSemaphore = new SemaphoreSlim(_maxConcurrentPublishes, _maxConcurrentPublishes);
        }

        private string GetValue(string[] parts, string key)
        {
            foreach (var part in parts)
            {
                var keyValue = part.Split('=');
                if (keyValue.Length == 2 && keyValue[0].Trim().Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    return keyValue[1].Trim();
                }
            }
            return null;
        }

        protected override void OnStart(string[] args)
        {
            WriteLog("Task Reminder Service is starting...");
            
            try
            {
                // Initialize RabbitMQ connection
                _rabbitMqPublisher = new RabbitMqPublisher(_rabbitMqHost, _rabbitMqUsername, _rabbitMqPassword, _queueName);
                WriteLog($"RabbitMQ publisher initialized. Host: {_rabbitMqHost}, Queue: {_queueName}");

                // Start timer to check for overdue tasks
                var interval = TimeSpan.FromMinutes(_checkIntervalMinutes);
                
                lock (_timerLock)
                {
                    _timer = new Timer(async _ => 
                    {
                        // Prevent overlapping timer callbacks
                        if (!_processingSemaphore.Wait(0))
                        {
                            WriteLog("Previous check still in progress, skipping this cycle.");
                            return;
                        }

                        try
                        {
                            await CheckOverdueTasksAsync(_rabbitMqPublisher);
                        }
                        catch (Exception ex)
                        {
                            WriteLog($"Unhandled exception in timer callback: {ex.Message}");
                        }
                        finally
                        {
                            _processingSemaphore.Release();
                        }
                    }, null, TimeSpan.Zero, interval);
                }
                
                _isRunning = true;
                WriteLog($"Service started. Checking for overdue tasks every {_checkIntervalMinutes} minutes. Max concurrent publishes: {_maxConcurrentPublishes}");
            }
            catch (Exception ex)
            {
                WriteLog($"Error starting service: {ex.Message}");
                throw;
            }
        }

        protected override void OnStop()
        {
            WriteLog("Task Reminder Service is stopping...");
            _isRunning = false;
            
            // Wait for any ongoing processing to complete (with timeout)
            if (!_processingSemaphore.Wait(TimeSpan.FromSeconds(30)))
            {
                WriteLog("Warning: Some tasks may still be processing during shutdown.");
            }
            
            lock (_timerLock)
            {
                _timer?.Dispose();
                _timer = null;
            }
            
            _rabbitMqPublisher?.Dispose();
            _processingSemaphore?.Dispose();
            _processedTasks.Clear();
            
            WriteLog("Service stopped.");
        }

        private async Task CheckOverdueTasksAsync(RabbitMqPublisher rabbitMqPublisher)
        {
            if (!_isRunning)
                return;

            try
            {
                WriteLog("Checking for overdue tasks...");
                
                var taskRepository = new TaskRepository(_connectionString);
                var overdueTasks = await taskRepository.GetOverdueTasksAsync(DateTime.UtcNow);

                WriteLog($"Found {overdueTasks.Count} overdue task(s).");

                if (overdueTasks.Count == 0)
                    return;

                // Process tasks in parallel with concurrency control
                var publishTasks = overdueTasks.Select(async task =>
                {
                    // Check if we've already processed this task recently (within last check interval)
                    var lastProcessed = _processedTasks.GetOrAdd(task.Id, DateTime.MinValue);
                    var timeSinceLastProcess = DateTime.UtcNow - lastProcessed;
                    
                    if (timeSinceLastProcess.TotalMinutes < _checkIntervalMinutes)
                    {
                        WriteLog($"Skipping task ID {task.Id} - already processed {timeSinceLastProcess.TotalMinutes:F1} minutes ago");
                        return;
                    }

                    // Acquire semaphore for concurrent publishing
                    await _processingSemaphore.WaitAsync();
                    
                    try
                    {
                        var reminderMessage = new TaskReminderMessage
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

                        var published = await rabbitMqPublisher.PublishReminderAsync(reminderMessage, CancellationToken.None);
                        
                        if (published)
                        {
                            _processedTasks.AddOrUpdate(task.Id, DateTime.UtcNow, (key, oldValue) => DateTime.UtcNow);
                            WriteLog($"Published reminder for task ID {task.Id} - {task.Title}");
                        }
                        else
                        {
                            WriteLog($"Skipped duplicate reminder for task ID {task.Id}");
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog($"Error publishing reminder for task ID {task.Id}: {ex.Message}");
                    }
                    finally
                    {
                        _processingSemaphore.Release();
                    }
                });

                // Wait for all publish operations to complete
                await Task.WhenAll(publishTasks);
                
                // Cleanup old processed task entries (older than 2 check intervals)
                CleanupProcessedTasks();
                
                WriteLog($"Completed processing {overdueTasks.Count} overdue task(s).");
            }
            catch (Exception ex)
            {
                WriteLog($"Error checking overdue tasks: {ex.Message}");
            }
        }

        private void CleanupProcessedTasks()
        {
            var cutoffTime = DateTime.UtcNow.AddMinutes(-(_checkIntervalMinutes * 2));
            var keysToRemove = _processedTasks
                .Where(kvp => kvp.Value < cutoffTime)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in keysToRemove)
            {
                _processedTasks.TryRemove(key, out _);
            }
        }

        private void WriteLog(string message)
        {
            // In a production environment, use proper logging (NLog, Serilog, etc.)
            // For now, we'll use EventLog or a simple file log
            try
            {
                System.Diagnostics.EventLog.WriteEntry(ServiceName, $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {message}", 
                    System.Diagnostics.EventLogEntryType.Information);
            }
            catch
            {
                // If EventLog fails, try writing to a file
                try
                {
                    var logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "service.log");
                    System.IO.File.AppendAllText(logPath, $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {message}\r\n");
                }
                catch
                {
                    // If all logging fails, ignore (service should continue running)
                }
            }
        }
    }
}

