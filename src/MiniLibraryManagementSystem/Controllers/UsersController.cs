using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MiniLibraryManagementSystem.DTOs;
using MiniLibraryManagementSystem.Services;

namespace MiniLibraryManagementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IUserManagementService _userManagement;

    public UsersController(IUserManagementService userManagement)
    {
        _userManagement = userManagement;
    }

    /// <summary>Get all users with their roles (Admin only).</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserWithRolesDto>>> GetUsers(CancellationToken ct)
    {
        var users = await _userManagement.GetUsersWithRolesAsync(ct);
        return Ok(users);
    }

    /// <summary>Get available role names (Admin only).</summary>
    [HttpGet("roles")]
    public async Task<ActionResult<IEnumerable<string>>> GetRoles(CancellationToken ct)
    {
        var roles = await _userManagement.GetRoleNamesAsync(ct);
        return Ok(roles);
    }

    /// <summary>Update a user's roles (Admin only).</summary>
    [HttpPut("{userId}/roles")]
    public async Task<ActionResult> UpdateRoles(string userId, [FromBody] UpdateUserRolesRequest? request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest("User ID is required.");
        var roles = request?.Roles ?? Array.Empty<string>();
        var (success, error) = await _userManagement.UpdateUserRolesAsync(userId, roles, ct);
        if (!success)
            return BadRequest(error ?? "Update failed.");
        return NoContent();
    }
}
