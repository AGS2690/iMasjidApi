using System.Security.Cryptography;
using iMasjid.Application.Interfaces.Services;

namespace iMasjid.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16; // 128 bit
    private const int KeySize = 32;  // 256 bit
    private const int Iterations = 100_000;
    private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA512;
    private const char SegmentDelimiter = ':';

    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty.", nameof(password));

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithm,
            KeySize);

        return string.Join(
            SegmentDelimiter,
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash),
            Iterations,
            HashAlgorithm.Name);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
            return false;

        var segments = passwordHash.Split(SegmentDelimiter);
        if (segments.Length != 4)
            return false;

        try
        {
            var salt = Convert.FromBase64String(segments[0]);
            var hash = Convert.FromBase64String(segments[1]);

            if (!int.TryParse(segments[2], out var iterations) || iterations <= 0)
                return false;

            var algorithm = new HashAlgorithmName(segments[3]);

            var inputHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                algorithm,
                hash.Length);

            return CryptographicOperations.FixedTimeEquals(hash, inputHash);
        }
        catch
        {
            return false;
        }
    }
}
