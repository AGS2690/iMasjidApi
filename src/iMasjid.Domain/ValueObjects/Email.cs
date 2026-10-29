using System.Text.RegularExpressions;
using iMasjid.Domain.Common;
using iMasjid.Domain.Exceptions;

namespace iMasjid.Domain.ValueObjects;

public sealed class Email : ValueObject
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ValidationException("Email address is required.");

        var trimmed = email.Trim().ToLowerInvariant();
        if (!EmailRegex.IsMatch(trimmed) || trimmed.Length > 256)
            throw new ValidationException("Email address format is invalid.");

        return new Email(trimmed);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}
