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
    private readonly IConfiguration _config;

    public AccountController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, IConfiguration config)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _config = config;
    }

    [HttpGet("ExternalLogin")]
    public IActionResult ExternalLogin([FromQuery] string provider, [FromQuery] string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(provider))
            return Redirect("/login?message=Invalid+provider");
        var clientId = _config[$"Authentication:{provider}:ClientId"];
        if (string.IsNullOrWhiteSpace(clientId))
            return Redirect($"/login?message={Uri.EscapeDataString(provider + " sign-in is not configured. Add ClientId and ClientSecret in appsettings.json.")}");
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
            var existingUser = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            var defaultUrl = existingUser != null && (await _userManager.IsInRoleAsync(existingUser, "Admin") || await _userManager.IsInRoleAsync(existingUser, "Librarian"))
                ? "/dashboard"
                : "/books";
            return LocalRedirect(returnUrl ?? defaultUrl);
        }

        if (result.IsLockedOut)
            return Redirect("/login?message=Account+locked+out.");

        // Microsoft often returns preferred_username; Google returns email
        var email = info.Principal.FindFirstValue(ClaimTypes.Email)
            ?? info.Principal.FindFirstValue("email")
            ?? info.Principal.FindFirstValue("preferred_username");
        if (string.IsNullOrEmpty(email))
            return Redirect("/login?message=Email+claim+not+received+from+external+provider.");

        var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
        var createResult = await _userManager.CreateAsync(user);
        if (!createResult.Succeeded)
            return Redirect("/login?message=" + Uri.EscapeDataString(string.Join(" ", createResult.Errors.Select(e => e.Description))));

        await _userManager.AddToRoleAsync(user, RoleSeed.Member);
        await _userManager.AddLoginAsync(user, info);
        await _signInManager.SignInAsync(user, isPersistent: false);
        return LocalRedirect(returnUrl ?? "/books");
    }

    [HttpGet("Logout")]
    [HttpPost("Logout")]
    public async Task<IActionResult> Logout([FromQuery] string? returnUrl = null)
    {
        await _signInManager.SignOutAsync();
        return LocalRedirect(returnUrl ?? "/login");
    }
}
