using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SoftwareDeployment.Server.Services;
using SoftwareDeployment.Shared;

namespace SoftwareDeployment.Server.Data;

/// <summary>
/// Startup work: create the data directory, ensure the schema exists, make sure
/// a settings row + enrollment key are present, and apply any admin-password
/// bootstrap written by the server console.
/// </summary>
public static class DbInitializer
{
    public static async Task InitializeAsync(WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        IServiceProvider sp = scope.ServiceProvider;

        var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ServerOptions>>().Value;
        var env = sp.GetRequiredService<IHostEnvironment>();
        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");

        Directory.CreateDirectory(options.ResolveDataDirectory(env.ContentRootPath));
        Directory.CreateDirectory(options.ResolvePackagesDirectory(env.ContentRootPath));

        var factory = sp.GetRequiredService<IDbContextFactory<DeploymentDbContext>>();
        await using (DeploymentDbContext db = await factory.CreateDbContextAsync())
        {
            await db.Database.EnsureCreatedAsync();
        }

        var settings = sp.GetRequiredService<SettingsService>();
        await settings.GetAsync(); // ensures settings row + enrollment key

        await ApplyBootstrapSecretsAsync(env, settings, factory, logger);

        bool hasPassword = await settings.HasAdminPasswordAsync();
        if (!hasPassword)
        {
            logger.LogWarning(
                "No admin password is set yet. Open the web interface to set one on first run, " +
                "or configure it with the server console.");
        }
    }

    private static async Task ApplyBootstrapSecretsAsync(
        IHostEnvironment env,
        SettingsService settings,
        IDbContextFactory<DeploymentDbContext> factory,
        ILogger logger)
    {
        string path = Path.Combine(env.ContentRootPath, BootstrapSecrets.FileName);
        if (!File.Exists(path)) return;

        try
        {
            BootstrapSecrets? secrets = JsonSerializer.Deserialize<BootstrapSecrets>(
                await File.ReadAllTextAsync(path));
            if (secrets?.AdminPasswordHash is null) return;

            await using DeploymentDbContext db = await factory.CreateDbContextAsync();
            SettingsEntity s = await db.Settings.FirstAsync();

            if (secrets.Revision > s.AppliedSecretsRevision)
            {
                s.AdminPasswordHash = secrets.AdminPasswordHash;
                s.AppliedSecretsRevision = secrets.Revision;
                await db.SaveChangesAsync();
                logger.LogInformation(
                    "Applied admin password from {File} (revision {Rev}).",
                    BootstrapSecrets.FileName, secrets.Revision);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to apply bootstrap secrets from {File}.", path);
        }
    }
}
