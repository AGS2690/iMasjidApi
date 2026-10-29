using FluentValidation;
using iMasjid.Application.Common.Extensions;
using iMasjid.Application.DTOs.Requests.Settings;

namespace iMasjid.Application.Validators.Settings;

public class CreateSettingRequestValidator : AbstractValidator<CreateSettingRequest>
{
    public CreateSettingRequestValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty().WithMessage("Setting key is required.")
            .MaximumLength(100).WithMessage("Key cannot exceed 100 characters.")
            .Matches(@"^[a-zA-Z0-9_\.\-]+$").WithMessage("Key may only contain alphanumeric characters, dots, underscores, and hyphens.")
            .NoScriptTags();

        RuleFor(x => x.Value)
            .NotNull().WithMessage("Value cannot be null.")
            .NoScriptTags();

        RuleFor(x => x.Module)
            .NotEmpty().WithMessage("Module name is required.")
            .MaximumLength(50).WithMessage("Module name cannot exceed 50 characters.")
            .NoScriptTags();

        RuleFor(x => x.Description)
            .MaximumLength(250).WithMessage("Description cannot exceed 250 characters.")
            .NoScriptTags();
    }
}

public class UpdateSettingRequestValidator : AbstractValidator<UpdateSettingRequest>
{
    public UpdateSettingRequestValidator()
    {
        RuleFor(x => x.Value)
            .NotNull().WithMessage("Value cannot be null.")
            .NoScriptTags();

        RuleFor(x => x.Description)
            .MaximumLength(250).WithMessage("Description cannot exceed 250 characters.")
            .NoScriptTags();
    }
}
