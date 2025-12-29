using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.Data;
using TaskManagementAPI.Models;
using ModelsUser = TaskManagementAPI.Models.User;

namespace TaskManagementAPI.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async System.Threading.Tasks.Task<IEnumerable<ModelsUser>> GetAllAsync()
    {
        return await _context.Users.ToListAsync();
    }

    public async System.Threading.Tasks.Task<ModelsUser?> GetByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async System.Threading.Tasks.Task<ModelsUser?> GetByUsernameAsync(string username)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async System.Threading.Tasks.Task<ModelsUser> CreateAsync(ModelsUser user)
    {
        user.CreatedAt = DateTime.UtcNow;
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async System.Threading.Tasks.Task<ModelsUser?> UpdateAsync(int id, ModelsUser user)
    {
        var existingUser = await _context.Users.FindAsync(id);
        if (existingUser == null)
        {
            return null;
        }

        existingUser.Email = user.Email;
        existingUser.Role = user.Role;
        existingUser.FullName = user.FullName;
        existingUser.IsActive = user.IsActive;
        existingUser.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existingUser;
    }

    public async System.Threading.Tasks.Task<bool> DeleteAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return false;
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }
}


