using System;

namespace TaskManagementService.Messaging
{
    public class TaskReminderMessage
    {
        public string MessageId { get; set; } = Guid.NewGuid().ToString();
        public int TaskId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public string Priority { get; set; }
        public string UserFullName { get; set; }
        public string UserEmail { get; set; }
        public DateTime DetectedAt { get; set; }
        public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
    }
}

