using MiniLibraryManagementSystem.DTOs;

namespace MiniLibraryManagementSystem.Services;

public interface IUserManagementService
{
    Task<IReadOnlyList<UserWithRolesDto>> GetUsersWithRolesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetRoleNamesAsync(CancellationToken ct = default);
    Task<(bool Success, string? Error)> UpdateUserRolesAsync(string userId, IReadOnlyList<string> roles, CancellationToken ct = default);
}
