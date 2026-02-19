using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MiniLibraryManagementSystem.Data.Seed;

namespace MiniLibraryManagementSystem.Controllers;

[AllowAnonymous]
[Route("Account")]
public class AccountController : Controller
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public AccountController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpGet("ExternalLogin")]
    public IActionResult ExternalLogin([FromQuery] string provider, [FromQuery] string? returnUrl = null)
    {
        var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(properties, provider);
    }

    [HttpGet("ExternalLoginCallback")]
    public async Task<IActionResult> ExternalLoginCallback([FromQuery] string? returnUrl = null, [FromQuery] string? remoteError = null, CancellationToken ct = default)
    {
        if (remoteError != null)
            return Redirect($"/login?message={Uri.EscapeDataString(remoteError)}");

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
            return Redirect("/login?message=Error+loading+external+login+information.");

        var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
        if (result.Succeeded)
        {
            return LocalRedirect(returnUrl ?? "/dashboard");
        }

        if (result.IsLockedOut)
            return Redirect("/login?message=Account+locked+out.");

        var email = info.Principal.FindFirstValue(ClaimTypes.Email) ?? info.Principal.FindFirstValue("email");
        if (string.IsNullOrEmpty(email))
            return Redirect("/login?message=Email+claim+not+received+from+external+provider.");

        var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
        var createResult = await _userManager.CreateAsync(user);
        if (!createResult.Succeeded)
            return Redirect("/login?message=" + Uri.EscapeDataString(string.Join(" ", createResult.Errors.Select(e => e.Description))));

        await _userManager.AddToRoleAsync(user, RoleSeed.Member);
        await _userManager.AddLoginAsync(user, info);
        await _signInManager.SignInAsync(user, isPersistent: false);
        return LocalRedirect(returnUrl ?? "/dashboard");
    }

    [HttpGet("Logout")]
    [HttpPost("Logout")]
    public async Task<IActionResult> Logout([FromQuery] string? returnUrl = null)
    {
        await _signInManager.SignOutAsync();
        return LocalRedirect(returnUrl ?? "/login");
    }
}
