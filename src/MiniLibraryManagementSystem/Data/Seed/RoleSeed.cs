using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MiniLibraryManagementSystem.Data.Seed;

public static class RoleSeed
{
    public const string Admin = "Admin";
    public const string Librarian = "Librarian";
    public const string Member = "Member";

    public static async Task SeedAsync(RoleManager<IdentityRole> roleManager, CancellationToken ct = default)
    {
        foreach (var roleName in new[] { Admin, Librarian, Member })
        {
            if (await roleManager.RoleExistsAsync(roleName)) continue;
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}
