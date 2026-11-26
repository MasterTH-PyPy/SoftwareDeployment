using System.Security.Cryptography;
using Microsoft.Extensions.Options;

namespace SoftwareDeployment.Server.Services;

public sealed record StoredPayload(string StoredName, string Sha256, long Size);

/// <summary>
/// Stores uploaded package payloads on disk under a GUID file name and hands
/// them back for download. The database only keeps metadata + the stored name.
/// </summary>
public sealed class PackageStorage
{
    private readonly string _root;

    public PackageStorage(IOptions<ServerOptions> options, IHostEnvironment env)
    {
        _root = options.Value.ResolvePackagesDirectory(env.ContentRootPath);
        Directory.CreateDirectory(_root);
    }

    /// <summary>Persist a stream, returning its stored name, SHA-256 and size.</summary>
    public async Task<StoredPayload> SaveAsync(Stream source, CancellationToken ct = default)
    {
        string storedName = Guid.NewGuid().ToString("N");
        string finalPath = Path.Combine(_root, storedName);
        string tempPath = finalPath + ".tmp";

        long size;
        byte[] hash;
        try
        {
            await using (var fs = new FileStream(
                tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None,
                bufferSize: 1 << 20, useAsync: true))
            using (var sha = SHA256.Create())
            using (var crypto = new CryptoStream(fs, sha, CryptoStreamMode.Write))
            {
                await source.CopyToAsync(crypto, ct);
                await crypto.FlushFinalBlockAsync(ct);
                size = fs.Length;
                hash = sha.Hash!;
            }

            File.Move(tempPath, finalPath, overwrite: false);
        }
        catch
        {
            TryDelete(tempPath);
            throw;
        }

        return new StoredPayload(storedName, Convert.ToHexString(hash).ToLowerInvariant(), size);
    }

    public string GetPath(string storedName) => Path.Combine(_root, storedName);

    public bool Exists(string storedName) => File.Exists(GetPath(storedName));

    public Stream OpenRead(string storedName)
        => new FileStream(GetPath(storedName), FileMode.Open, FileAccess.Read, FileShare.Read,
                          bufferSize: 1 << 20, useAsync: true);

    public void Delete(string storedName) => TryDelete(GetPath(storedName));

    private static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); }
        catch { /* best effort */ }
    }
}
