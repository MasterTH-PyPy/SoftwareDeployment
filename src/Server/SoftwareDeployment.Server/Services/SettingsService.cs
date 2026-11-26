using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using SoftwareDeployment.Server.Data;
using SoftwareDeployment.Shared.Security;

namespace SoftwareDeployment.Server.Services;

/// <summary>
/// Reads and writes the single global settings row. Uses a short-lived context
/// per call (via the factory) so it is safe to use from both the request
/// pipeline and long-lived Blazor circuits.
/// </summary>
public sealed class SettingsService(
    IDbContextFactory<DeploymentDbContext> dbFactory,
    ISecretProtector protector)
{
    public async Task<SettingsEntity> GetAsync(CancellationToken ct = default)
    {
        await using DeploymentDbContext db = await dbFactory.CreateDbContextAsync(ct);
        return await EnsureAsync(db, ct);
    }

    public async Task SaveGeneralAsync(
        int pollIntervalSeconds, bool autoApprove, string organizationName, CancellationToken ct = default)
    {
        await using DeploymentDbContext db = await dbFactory.CreateDbContextAsync(ct);
        SettingsEntity s = await EnsureAsync(db, ct);
        s.PollIntervalSeconds = Math.Clamp(pollIntervalSeconds, 30, 86_400);
        s.AutoApproveClients = autoApprove;
        s.OrganizationName = string.IsNullOrWhiteSpace(organizationName) ? "SonOG" : organizationName.Trim();
        await db.SaveChangesAsync(ct);
    }

    // ---- admin password --------------------------------------------------

    public async Task<bool> HasAdminPasswordAsync(CancellationToken ct = default)
    {
        SettingsEntity s = await GetAsync(ct);
        return !string.IsNullOrEmpty(s.AdminPasswordHash);
    }

    public async Task SetAdminPasswordAsync(string newPassword, CancellationToken ct = default)
    {
        await using DeploymentDbContext db = await dbFactory.CreateDbContextAsync(ct);
        SettingsEntity s = await EnsureAsync(db, ct);
        s.AdminPasswordHash = PasswordHasher.Hash(newPassword);
        await db.SaveChangesAsync(ct);
    }

    public async Task<bool> VerifyAdminPasswordAsync(string password, CancellationToken ct = default)
    {
        SettingsEntity s = await GetAsync(ct);
        return PasswordHasher.Verify(password, s.AdminPasswordHash);
    }

    // ---- enrollment key --------------------------------------------------

    public async Task<string> GetEnrollmentKeyAsync(CancellationToken ct = default)
    {
        await using DeploymentDbContext db = await dbFactory.CreateDbContextAsync(ct);
        SettingsEntity s = await EnsureAsync(db, ct);
        return protector.Unprotect(s.EnrollmentKeyProtected!);
    }

    public async Task<string> RegenerateEnrollmentKeyAsync(CancellationToken ct = default)
    {
        await using DeploymentDbContext db = await dbFactory.CreateDbContextAsync(ct);
        SettingsEntity s = await EnsureAsync(db, ct);
        string key = GenerateEnrollmentKey();
        s.EnrollmentKeyProtected = protector.Protect(key);
        await db.SaveChangesAsync(ct);
        return key;
    }

    public async Task<bool> VerifyEnrollmentKeyAsync(string provided, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(provided)) return false;
        string expected = await GetEnrollmentKeyAsync(ct);
        return CryptographicOperations.FixedTimeEquals(
            System.Text.Encoding.UTF8.GetBytes(Normalize(provided)),
            System.Text.Encoding.UTF8.GetBytes(Normalize(expected)));
    }

    // ---- helpers ---------------------------------------------------------

    private async Task<SettingsEntity> EnsureAsync(DeploymentDbContext db, CancellationToken ct)
    {
        SettingsEntity? s = await db.Settings.FirstOrDefaultAsync(ct);
        if (s is null)
        {
            s = new SettingsEntity { Id = 1 };
            db.Settings.Add(s);
        }
        // Always make sure an enrollment key exists.
        if (string.IsNullOrEmpty(s.EnrollmentKeyProtected))
            s.EnrollmentKeyProtected = protector.Protect(GenerateEnrollmentKey());

        await db.SaveChangesAsync(ct);
        return s;
    }

    private static string Normalize(string key)
        => key.Trim().Replace("-", string.Empty).ToUpperInvariant();

    /// <summary>Readable 20-char key in 4 groups, no ambiguous characters.</summary>
    private static string GenerateEnrollmentKey()
    {
        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // no I,O,0,1
        Span<char> chars = stackalloc char[20];
        for (int i = 0; i < chars.Length; i++)
            chars[i] = alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)];

        // 20 characters + 3 separators ("-----" every 5) = 23.
        return string.Create(23, chars.ToArray(), static (dst, src) =>
        {
            int j = 0;
            for (int i = 0; i < src.Length; i++)
            {
                if (i > 0 && i % 5 == 0) dst[j++] = '-';
                dst[j++] = src[i];
            }
        });
    }
}
