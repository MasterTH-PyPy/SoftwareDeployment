using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SoftwareDeployment.Server;
using SoftwareDeployment.Server.Api;
using SoftwareDeployment.Server.Components;
using SoftwareDeployment.Server.Data;
using SoftwareDeployment.Server.Services;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    // When hosted as a Windows Service the working dir is system32, so pin the
    // content root to the executable's folder.
    ContentRootPath = AppContext.BaseDirectory,
});

// Run as a Windows Service when there is no console (installed mode).
builder.Host.UseWindowsService(o => o.ServiceName = SoftwareDeployment.Shared.DeploymentConstants.ServerServiceName);

builder.Services.Configure<ServerOptions>(builder.Configuration.GetSection(ServerOptions.SectionName));
var serverOptions = builder.Configuration.GetSection(ServerOptions.SectionName).Get<ServerOptions>()
                    ?? new ServerOptions();

// ---- data access -----------------------------------------------------------
string dbPath = serverOptions.ResolveDatabasePath(builder.Environment.ContentRootPath);
Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

builder.Services.AddDbContextFactory<DeploymentDbContext>(o =>
    o.UseSqlite($"Data Source={dbPath};Cache=Shared"));
// A request-scoped context for the API pipeline, created from the same factory.
builder.Services.AddScoped<DeploymentDbContext>(sp =>
    sp.GetRequiredService<IDbContextFactory<DeploymentDbContext>>().CreateDbContext());

// ---- platform-specific services --------------------------------------------
if (OperatingSystem.IsWindows())
{
    builder.Services.AddSingleton<IDirectoryService, WindowsDirectoryService>();
    builder.Services.AddSingleton<ISecretProtector, WindowsSecretProtector>();
}
else
{
    builder.Services.AddSingleton<IDirectoryService, NullDirectoryService>();
    builder.Services.AddSingleton<ISecretProtector, PassthroughSecretProtector>();
}

builder.Services.AddSingleton<PackageStorage>();
builder.Services.AddSingleton<BrandingService>();
builder.Services.AddScoped<SettingsService>();
builder.Services.AddScoped<DeploymentService>();

// ---- authentication (admin web interface) ----------------------------------
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.Cookie.Name = "SonOGDeploy.Admin";
        o.Cookie.HttpOnly = true;
        o.Cookie.SecurePolicy = CookieSecurePolicy.Always; // TLS only
        o.Cookie.SameSite = SameSiteMode.Strict;
        o.LoginPath = "/account/login";
        o.LogoutPath = "/account/logout";
        o.ExpireTimeSpan = TimeSpan.FromHours(8);
        o.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();

// ---- Blazor + API ----------------------------------------------------------
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddHubOptions(o => o.MaximumReceiveMessageSize = 16L * 1024 * 1024); // room for file chunks

builder.WebHost.ConfigureKestrel(k =>
{
    k.Limits.MaxRequestBodySize = serverOptions.MaxUploadMegabytes * 1024 * 1024;
});

builder.Services.AddHsts(o =>
{
    o.MaxAge = TimeSpan.FromDays(365);
    o.IncludeSubDomains = true;
});

var app = builder.Build();

await DbInitializer.InitializeAsync(app);

// ---- middleware pipeline ----------------------------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Gate the admin web interface: anything that is not the client API, the login
// endpoints, the Blazor framework plumbing or a static asset requires a logged-in
// admin. The client API authenticates itself with its own header credentials.
app.Use(async (ctx, next) =>
{
    PathString p = ctx.Request.Path;
    bool isPublic =
        p.StartsWithSegments("/api") ||
        p.StartsWithSegments("/account") ||
        p.StartsWithSegments("/_blazor") ||
        p.StartsWithSegments("/_framework") ||
        p.StartsWithSegments("/_content") ||
        p.StartsWithSegments("/css") ||
        p.StartsWithSegments("/js") ||
        p.StartsWithSegments("/img") ||
        p == "/favicon.ico" ||
        p == "/error";

    if (!isPublic && ctx.User.Identity?.IsAuthenticated != true)
    {
        string returnUrl = Uri.EscapeDataString(p + ctx.Request.QueryString);
        ctx.Response.Redirect($"/account/login?returnUrl={returnUrl}");
        return;
    }

    await next();
});

app.UseAntiforgery();

app.MapClientApi();          // /api/v1/* — header-authenticated clients
app.MapAccountEndpoints();   // /account/login (POST), /account/logout
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
