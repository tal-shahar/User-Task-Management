using System.ComponentModel.DataAnnotations;

namespace TaskManagementAPI.Models;

public class Task
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public DateTime DueDate { get; set; }

    [Required]
    public Priority Priority { get; set; }

    [Required]
    public int UserId { get; set; }

    public User? User { get; set; }

    [Required]
    [MaxLength(200)]
    public string UserFullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    [EmailAddress]
    public string UserEmail { get; set; } = string.Empty;

    [MaxLength(50)]
    public string UserTelephone { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum Priority
{
    Low = 1,
    Medium = 2,
    High = 3
}

