using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MiniLibraryManagementSystem.Data.Seed;
using MiniLibraryManagementSystem.DTOs;

namespace MiniLibraryManagementSystem.Services;

public class UserManagementService : IUserManagementService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserManagementService(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IReadOnlyList<UserWithRolesDto>> GetUsersWithRolesAsync(CancellationToken ct = default)
    {
        var users = _userManager.Users.ToList();
        var result = new List<UserWithRolesDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            result.Add(new UserWithRolesDto(user.Id, user.UserName ?? user.Id, user.Email, roles.ToList()));
        }
        return result;
    }

    public async Task<IReadOnlyList<string>> GetRoleNamesAsync(CancellationToken ct = default)
    {
        var roles = await _roleManager.Roles.Select(r => r.Name!).Where(n => n != null).ToListAsync(ct);
        return roles.OrderBy(n => n == RoleSeed.Admin ? 0 : n == RoleSeed.Librarian ? 1 : 2).ToList();
    }

    public async Task<(bool Success, string? Error)> UpdateUserRolesAsync(string userId, IReadOnlyList<string> roles, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return (false, "User not found.");

        var validRoles = await GetRoleNamesAsync(ct);
        var toAssign = roles.Where(r => validRoles.Contains(r)).Distinct().ToList();

        var currentRoles = await _userManager.GetRolesAsync(user);
        var toRemove = currentRoles.Except(toAssign).ToList();
        var toAdd = toAssign.Except(currentRoles).ToList();

        if (toRemove.Count > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, toRemove);
            if (!removeResult.Succeeded)
                return (false, string.Join("; ", removeResult.Errors.Select(e => e.Description)));
        }

        if (toAdd.Count > 0)
        {
            var addResult = await _userManager.AddToRolesAsync(user, toAdd);
            if (!addResult.Succeeded)
                return (false, string.Join("; ", addResult.Errors.Select(e => e.Description)));
        }

        return (true, null);
    }
}
