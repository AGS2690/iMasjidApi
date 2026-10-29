using FluentValidation;
using iMasjid.Application.Common.Extensions;
using iMasjid.Application.DTOs.Requests.Roles;

namespace iMasjid.Application.Validators.Roles;

public class CreateRoleRequestValidator : AbstractValidator<CreateRoleRequest>
{
    public CreateRoleRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Role name is required.")
            .MaximumLength(50).WithMessage("Role name cannot exceed 50 characters.")
            .Matches(@"^[a-zA-Z0-9_\-]+$").WithMessage("Role name may only contain alphanumeric characters, underscores, and hyphens.")
            .NoScriptTags();

        RuleFor(x => x.Description)
            .MaximumLength(250).WithMessage("Description cannot exceed 250 characters.")
            .NoScriptTags();
    }
}

public class UpdateRoleRequestValidator : AbstractValidator<UpdateRoleRequest>
{
    public UpdateRoleRequestValidator()
    {
        RuleFor(x => x.Description)
            .MaximumLength(250).WithMessage("Description cannot exceed 250 characters.")
            .NoScriptTags();
    }
}

public class UpdateRolePermissionsRequestValidator : AbstractValidator<UpdateRolePermissionsRequest>
{
    public UpdateRolePermissionsRequestValidator()
    {
        RuleFor(x => x.Permissions)
            .NotNull().WithMessage("Permissions list cannot be null.");

        RuleForEach(x => x.Permissions)
            .NotEmpty().WithMessage("Permission code cannot be empty.")
            .NoScriptTags();
    }
}
