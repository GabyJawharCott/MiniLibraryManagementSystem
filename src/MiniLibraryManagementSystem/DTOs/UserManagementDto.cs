namespace MiniLibraryManagementSystem.DTOs;

/// <summary>User with current role names for admin management.</summary>
public record UserWithRolesDto(string Id, string UserName, string? Email, IReadOnlyList<string> Roles);

/// <summary>Request to set a user's roles.</summary>
public record UpdateUserRolesRequest(IReadOnlyList<string> Roles);
