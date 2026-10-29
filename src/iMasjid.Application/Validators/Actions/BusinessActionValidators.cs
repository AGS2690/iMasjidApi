using FluentValidation;
using iMasjid.Application.Common.Extensions;
using iMasjid.Application.DTOs.Requests.Actions;

namespace iMasjid.Application.Validators.Actions;

public class CreateBusinessActionRequestValidator : AbstractValidator<CreateBusinessActionRequest>
{
    public CreateBusinessActionRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Action name is required.")
            .MaximumLength(100).WithMessage("Action name cannot exceed 100 characters.")
            .NoScriptTags();

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Action code is required.")
            .MaximumLength(50).WithMessage("Action code cannot exceed 50 characters.")
            .Matches(@"^[A-Z0-9_]+$").WithMessage("Action code must be uppercase alphanumeric and underscores only (e.g., 'CREATE', 'EXPORT_PDF').")
            .NoScriptTags();

        RuleFor(x => x.Description)
            .MaximumLength(250).WithMessage("Description cannot exceed 250 characters.")
            .NoScriptTags();
    }
}

public class UpdateBusinessActionRequestValidator : AbstractValidator<UpdateBusinessActionRequest>
{
    public UpdateBusinessActionRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Action name is required.")
            .MaximumLength(100).WithMessage("Action name cannot exceed 100 characters.")
            .NoScriptTags();

        RuleFor(x => x.Description)
            .MaximumLength(250).WithMessage("Description cannot exceed 250 characters.")
            .NoScriptTags();
    }
}
