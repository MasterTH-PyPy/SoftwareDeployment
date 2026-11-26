using System.Security.Cryptography;

namespace SoftwareDeployment.Shared.Security;

/// <summary>
/// PBKDF2 (SHA-256) password hashing, shared by the server (which verifies the
/// admin password on web login) and the server console (which sets it). Output
/// is a single self-describing string: <c>iterations.saltBase64.hashBase64</c>.
/// </summary>
public static class PasswordHasher
{
    private const int DefaultIterations = 210_000;
    private const int SaltSize = 16;   // 128 bit
    private const int KeySize = 32;    // 256 bit

    public static string Hash(string password, int iterations = DefaultIterations)
    {
        ArgumentException.ThrowIfNullOrEmpty(password);

        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] key = Rfc2898DeriveBytes.Pbkdf2(
            password, salt, iterations, HashAlgorithmName.SHA256, KeySize);

        return string.Join('.',
            iterations,
            Convert.ToBase64String(salt),
            Convert.ToBase64String(key));
    }

    /// <summary>Constant-time verification of a password against a stored hash string.</summary>
    public static bool Verify(string password, string? stored)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(stored))
            return false;

        string[] parts = stored.Split('.', 3);
        if (parts.Length != 3)
            return false;

        if (!int.TryParse(parts[0], out int iterations) || iterations <= 0)
            return false;

        byte[] salt, expected;
        try
        {
            salt = Convert.FromBase64String(parts[1]);
            expected = Convert.FromBase64String(parts[2]);
        }
        catch (FormatException)
        {
            return false;
        }

        byte[] actual = Rfc2898DeriveBytes.Pbkdf2(
            password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);

        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
