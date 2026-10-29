namespace iMasjid.Application.Interfaces.Services;

/// <summary>
/// Service abstraction for secure password hashing and verification.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes the plain-text password using a cryptographically secure key derivation function with random salt.
    /// </summary>
    /// <param name="password">Plain text password.</param>
    /// <returns>Formatted salt + hash string.</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verifies a plain-text password against a stored password hash in constant time.
    /// </summary>
    /// <param name="password">Plain text password to test.</param>
    /// <param name="passwordHash">Stored password hash.</param>
    /// <returns>True if password matches; otherwise false.</returns>
    bool VerifyPassword(string password, string passwordHash);
}
