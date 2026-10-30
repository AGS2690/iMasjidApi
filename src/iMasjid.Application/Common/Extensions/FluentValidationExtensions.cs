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

        if (value.Contains('<') || value.Contains('>'))
            return false;

        foreach (var pattern in DangerousPatterns)
        {
            if (value.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0)
                return false;
        }

        return true;
    }
}
