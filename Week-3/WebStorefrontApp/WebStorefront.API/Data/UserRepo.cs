using Microsoft.EntityFrameworkCore;
using ProductCatalog.Models;

namespace ProductCatalog.Data;

public class UserRepo : IUserRepo
{
    private readonly AppDbContext _context;

    public UserRepo(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        // AsNoTracking: we're only reading, never updating from this query.
        // Skipping change-tracking is a tiny perf win and makes the intent clear.
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == username);
    }

}