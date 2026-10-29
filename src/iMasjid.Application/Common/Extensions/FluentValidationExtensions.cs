using FluentValidation;

namespace iMasjid.Application.Common.Extensions;

public static class FluentValidationExtensions
{
    private static readonly string[] DangerousPatterns =
    {
        "javascript:",
        "vbscript:",
        "onload=",
        "onerror=",
        "onclick=",
        "onmouseover=",
        "<script",
        "</script>",
        "eval(",
        "expression("
    };

    /// <summary>
    /// Validates that a string does not contain HTML/script injection characters (&lt; or &gt;) or malicious script payloads.
    /// Allows null or empty values if the field is optional.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> NoScriptTags<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .Must(BeSafeText)
            .WithMessage("{PropertyName} must not contain HTML elements, script tags, or '<' and '>' characters.");
    }

    private static bool BeSafeText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return true; // Allow null or empty for optional fields

        // Disallow '<' and '>' to prevent any HTML/XML/Script tags
        if (value.Contains('<') || value.Contains('>'))
            return false;

        // Check for common malicious script keywords (case-insensitive)
        foreach (var pattern in DangerousPatterns)
        {
            if (value.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0)
                return false;
        }

        return true;
    }
}
