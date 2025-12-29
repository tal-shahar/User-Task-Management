namespace TaskManagementAPI.DTOs;

public class LoginDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public RoleDto Role { get; set; }
    public string FullName { get; set; } = string.Empty;
}

public enum RoleDto
{
    User = 1,
    Admin = 2
}


