using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using Newtonsoft.Json;

namespace TaskManagementService.Messaging
{
    public class RabbitMqPublisher : IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _queueName;
        private readonly object _publishLock = new object();
        private readonly ConcurrentDictionary<string, DateTime> _publishedMessages = new ConcurrentDictionary<string, DateTime>();
        private readonly int _maxRetries = 3;
        private readonly TimeSpan _retryDelay = TimeSpan.FromMilliseconds(500);
        private bool _disposed = false;

        public RabbitMqPublisher(string host, string username, string password, string queueName)
        {
            _queueName = queueName ?? throw new ArgumentNullException(nameof(queueName));

            var factory = new ConnectionFactory
            {
                HostName = host,
                UserName = username,
                Password = password,
                Port = 5672,
                AutomaticRecoveryEnabled = true, // Enable automatic connection recovery
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
                RequestedHeartbeat = TimeSpan.FromSeconds(60)
            };

            try
            {
                _connection = factory.CreateConnection();
                _connection.ConnectionShutdown += OnConnectionShutdown;
                
                _channel = _connection.CreateModel();
                _channel.ModelShutdown += OnChannelShutdown;

                // Declare queue with proper settings for concurrent processing
                _channel.QueueDeclare(
                    queue: _queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                // Enable publisher confirms for reliability
                _channel.ConfirmSelect();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to connect to RabbitMQ at {host}: {ex.Message}", ex);
            }
        }

        public async Task<bool> PublishReminderAsync(TaskReminderMessage message, CancellationToken cancellationToken = default)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (_disposed)
                throw new ObjectDisposedException(nameof(RabbitMqPublisher));

            // Check for duplicate messages (idempotency)
            var messageKey = $"{message.TaskId}_{message.DetectedAt:yyyyMMddHHmm}";
            if (_publishedMessages.ContainsKey(messageKey))
            {
                // Message already published recently (within same minute)
                return false;
            }

            return await PublishWithRetryAsync(message, cancellationToken);
        }

        private async Task<bool> PublishWithRetryAsync(TaskReminderMessage message, CancellationToken cancellationToken)
        {
            var messageKey = $"{message.TaskId}_{message.DetectedAt:yyyyMMddHHmm}";
            
            for (int attempt = 0; attempt < _maxRetries; attempt++)
            {
                try
                {
                    if (cancellationToken.IsCancellationRequested)
                        return false;

                    lock (_publishLock)
                    {
                        if (!_channel.IsOpen)
                        {
                            throw new InvalidOperationException("Channel is closed");
                        }

                        var json = JsonConvert.SerializeObject(message);
                        var body = Encoding.UTF8.GetBytes(json);

                        var properties = _channel.CreateBasicProperties();
                        properties.Persistent = true; // Make messages persistent
                        properties.MessageId = message.MessageId;
                        properties.CorrelationId = message.CorrelationId;
                        properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                        properties.Headers = new System.Collections.Generic.Dictionary<string, object>
                        {
                            { "TaskId", message.TaskId },
                            { "Priority", message.Priority }
                        };

                        _channel.BasicPublish(
                            exchange: "",
                            routingKey: _queueName,
                            mandatory: true,
                            basicProperties: properties,
                            body: body);

                        // Wait for confirmation (with timeout)
                        if (_channel.WaitForConfirms(TimeSpan.FromSeconds(5)))
                        {
                            // Successfully published - track it
                            _publishedMessages.TryAdd(messageKey, DateTime.UtcNow);
                            
                            // Clean up old entries (older than 1 hour)
                            CleanupOldMessageTracking();
                            
                            return true;
                        }
                        else
                        {
                            throw new InvalidOperationException("Publisher confirmation timeout");
                        }
                    }
                }
                catch (AlreadyClosedException)
                {
                    // Connection closed, will retry
                    if (attempt < _maxRetries - 1)
                    {
                        await Task.Delay(_retryDelay, cancellationToken);
                        continue;
                    }
                    throw;
                }
                catch (BrokerUnreachableException)
                {
                    // Broker unreachable, will retry
                    if (attempt < _maxRetries - 1)
                    {
                        await Task.Delay(_retryDelay, cancellationToken);
                        continue;
                    }
                    throw;
                }
                catch (Exception ex)
                {
                    if (attempt < _maxRetries - 1)
                    {
                        await Task.Delay(_retryDelay, cancellationToken);
                        continue;
                    }
                    throw new InvalidOperationException($"Failed to publish message to RabbitMQ after {_maxRetries} attempts: {ex.Message}", ex);
                }
            }

            return false;
        }

        private void CleanupOldMessageTracking()
        {
            var cutoffTime = DateTime.UtcNow.AddHours(-1);
            var keysToRemove = new System.Collections.Generic.List<string>();

            foreach (var kvp in _publishedMessages)
            {
                if (kvp.Value < cutoffTime)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                _publishedMessages.TryRemove(key, out _);
            }
        }

        private void OnConnectionShutdown(object sender, ShutdownEventArgs reason)
        {
            // Connection shutdown - will be handled by automatic recovery
        }

        private void OnChannelShutdown(object sender, ShutdownEventArgs reason)
        {
            // Channel shutdown - connection recovery will recreate it
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            try
            {
                _channel?.ModelShutdown -= OnChannelShutdown;
                _connection?.ConnectionShutdown -= OnConnectionShutdown;

                if (_channel?.IsOpen == true)
                {
                    _channel.Close();
                }
                _channel?.Dispose();

                if (_connection?.IsOpen == true)
                {
                    _connection.Close();
                }
                _connection?.Dispose();

                _publishedMessages.Clear();
            }
            catch
            {
                // Ignore disposal errors
            }
        }
    }
}

