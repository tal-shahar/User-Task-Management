using System.ComponentModel.DataAnnotations;

namespace TaskManagementAPI.Models;

public class Reminder
{
    public int Id { get; set; }

    [Required]
    public int TaskId { get; set; }

    public Task? Task { get; set; }

    [Required]
    [MaxLength(500)]
    public string Message { get; set; } = string.Empty;

    public DateTime? ProcessedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}


