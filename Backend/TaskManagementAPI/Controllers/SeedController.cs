using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.Data;
using TaskManagementAPI.Models;
using BCrypt.Net;

namespace TaskManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeedController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<SeedController> _logger;

    public SeedController(AppDbContext context, ILogger<SeedController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost("admin")]
    public async System.Threading.Tasks.Task<IActionResult> SeedAdmin()
    {
        try
        {
            var existingAdmin = await _context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
            if (existingAdmin != null)
            {
                return Ok(new { message = "Admin user already exists", username = "admin" });
            }

            var admin = new User
            {
                Username = "admin",
                Email = "admin@taskmanagement.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Role = Role.Admin,
                FullName = "Administrator",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(admin);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Admin user created successfully", username = "admin", password = "admin123" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding admin user");
            return StatusCode(500, "An error occurred while seeding admin user");
        }
    }
}
