using System.Security.Cryptography;

namespace FabrikaBackend.Services;

public static class PasswordService
{
    private const string Prefix = "PBKDF2";
    private const int CurrentIterations = 100_000;
    private const int SaltSize = 16;
    private const int KeySize = 32;

    public static string HashPassword(string password)
    {
        ArgumentNullException.ThrowIfNull(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            CurrentIterations,
            HashAlgorithmName.SHA256,
            KeySize);

        return string.Join(
            '$',
            Prefix,
            CurrentIterations,
            Convert.ToBase64String(salt),
            Convert.ToBase64String(key));
    }

    public static bool VerifyPassword(string storedPassword, string providedPassword, out bool needsRehash)
    {
        needsRehash = false;

        if (string.IsNullOrEmpty(storedPassword) || providedPassword is null)
            return false;

        if (!IsHashedPassword(storedPassword))
        {
            var isLegacyMatch = string.Equals(storedPassword, providedPassword, StringComparison.Ordinal);
            needsRehash = isLegacyMatch;
            return isLegacyMatch;
        }

        var parts = storedPassword.Split('$', 4);
        if (parts.Length != 4 || !int.TryParse(parts[1], out var iterations))
            return false;

        try
        {
            var salt = Convert.FromBase64String(parts[2]);
            var expectedKey = Convert.FromBase64String(parts[3]);
            var actualKey = Rfc2898DeriveBytes.Pbkdf2(
                providedPassword,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                expectedKey.Length);

            var isMatch = CryptographicOperations.FixedTimeEquals(actualKey, expectedKey);
            needsRehash = isMatch && iterations < CurrentIterations;
            return isMatch;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    public static bool IsHashedPassword(string password)
    {
        return password.StartsWith($"{Prefix}$", StringComparison.Ordinal);
    }
}
