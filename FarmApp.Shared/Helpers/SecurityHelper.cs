using System.Security.Cryptography;
using System.Text;

namespace FarmApp.Shared.Helpers;

public static class SecurityHelper
{
    
    public static string Sha256(string input)
    {
        input ??= string.Empty;

        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha.ComputeHash(bytes);

        return ConvertToHex(hash);
    }
    
    // Creates a hashed password with salt
    public static (string Hash, string Salt) HashPassword(string password)
    {
        // Generate a 16-byte salt using RNGCryptoServiceProvider
        byte [] saltBytes = new byte [16];
        using var rng = RandomNumberGenerator.Create();

        rng.GetBytes(saltBytes);

        // Derive a 256-bit subkey (hash) using PBKDF2 with 10,000 iterations
        var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 10000, HashAlgorithmName.SHA256);
        byte [] hashBytes = pbkdf2.GetBytes(32); // 256 bits

        return (Convert.ToBase64String(hashBytes), Convert.ToBase64String(saltBytes));
    }

    // Verifies a password against a stored hash and salt
    public static bool VerifyPassword(string password, string storedHash, string storedSalt)
    {
        byte [] saltBytes = Convert.FromBase64String(storedSalt);
        byte [] hashBytes = Convert.FromBase64String(storedHash);

        // Derive the key using the same algorithm, iterations, and salt
        using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 10000, HashAlgorithmName.SHA256);
        byte [] testHash = pbkdf2.GetBytes(32);

        // Compare byte by byte to avoid timing attacks
        return CryptographicOperations.FixedTimeEquals(testHash, hashBytes);
    }
    
    private static string ConvertToHex(byte[] bytes)
    {
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes)
            sb.Append(b.ToString("x2"));
        return sb.ToString();
    }
}