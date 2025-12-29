using System;

namespace TaskManagementService.Tests.Messaging
{
    public class TaskReminderMessage
    {
        public string MessageId { get; set; } = Guid.NewGuid().ToString();
        public int TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string UserFullName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public DateTime DetectedAt { get; set; }
        public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
    }
}

