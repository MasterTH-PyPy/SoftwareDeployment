using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using SoftwareDeployment.Server.Services;

namespace SoftwareDeployment.Server.Api;

/// <summary>Form-post endpoints backing the login page. Kept out of Blazor so the
/// cookie is written on a normal HTTP response.</summary>
public static class AccountEndpoints
{
    public const string AdminRole = "Administrator";

    public static void MapAccountEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/account");

        group.MapPost("/login", LoginAsync).DisableAntiforgery();
        group.MapPost("/set-initial-password", SetInitialPasswordAsync).DisableAntiforgery();
        group.MapGet("/logout", LogoutAsync);
        group.MapPost("/logout", LogoutAsync);
    }

    private static async Task<IResult> LoginAsync(HttpContext http, SettingsService settings)
    {
        IFormCollection form = await http.Request.ReadFormAsync();
        string password = form["password"].ToString();
        string returnUrl = SafeReturnUrl(form["returnUrl"].ToString());

        if (!await settings.VerifyAdminPasswordAsync(password))
            return Results.Redirect("/account/login?error=1&returnUrl=" + Uri.EscapeDataString(returnUrl));

        await SignInAsync(http);
        return Results.Redirect(returnUrl);
    }

    private static async Task<IResult> SetInitialPasswordAsync(HttpContext http, SettingsService settings)
    {
        // Only valid during first run, before any password exists.
        if (await settings.HasAdminPasswordAsync())
            return Results.Redirect("/account/login");

        IFormCollection form = await http.Request.ReadFormAsync();
        string password = form["password"].ToString();
        string confirm = form["confirm"].ToString();

        if (password.Length < 10 || password != confirm)
            return Results.Redirect("/account/login?error=2");

        await settings.SetAdminPasswordAsync(password);
        await SignInAsync(http);
        return Results.Redirect("/");
    }

    private static async Task<IResult> LogoutAsync(HttpContext http)
    {
        await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Results.Redirect("/account/login");
    }

    private static Task SignInAsync(HttpContext http)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "Administrator"),
            new(ClaimTypes.Role, AdminRole),
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return http.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = true });
    }

    /// <summary>Prevent open redirects: only allow local, single-slash paths.</summary>
    private static string SafeReturnUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl)) return "/";
        if (!returnUrl.StartsWith('/') || returnUrl.StartsWith("//")) return "/";
        return returnUrl;
    }
}
