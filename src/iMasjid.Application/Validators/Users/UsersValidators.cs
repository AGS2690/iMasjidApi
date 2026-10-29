using FluentValidation;
using iMasjid.Application.Common.Extensions;
using iMasjid.Application.DTOs.Requests.Users;

namespace iMasjid.Application.Validators.Users;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters.")
            .NoScriptTags();

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters.")
            .NoScriptTags();

        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("User name is required.")
            .MinimumLength(3).WithMessage("User name must be at least 3 characters.")
            .MaximumLength(50).WithMessage("User name cannot exceed 50 characters.")
            .Matches(@"^[a-zA-Z0-9_\.\-]+$").WithMessage("User name may only contain alphanumeric characters, dots, underscores, and hyphens.")
            .NoScriptTags();

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(256).WithMessage("Email cannot exceed 256 characters.")
            .NoScriptTags();

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .Matches(@"[A-Z]").WithMessage("New password must contain at least one uppercase letter.")
            .Matches(@"[a-z]").WithMessage("New password must contain at least one lowercase letter.")
            .Matches(@"[0-9]").WithMessage("New password must contain at least one digit.")
            .Matches(@"[\!\?\*\@\#\$\%\^\&\+\=]").WithMessage("New password must contain at least one special character.");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters.")
            .NoScriptTags();
    }
}

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters.")
            .NoScriptTags();

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters.")
            .NoScriptTags();

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters.")
            .NoScriptTags();
    }
}

public class AssignUserRolesRequestValidator : AbstractValidator<AssignUserRolesRequest>
{
    public AssignUserRolesRequestValidator()
    {
        RuleFor(x => x)
            .Must(x => (x.Roles != null && x.Roles.Count > 0) || (x.RoleIds != null && x.RoleIds.Count > 0))
            .WithMessage("At least one role or role ID must be provided.");

        When(x => x.Roles != null, () =>
        {
            RuleForEach(x => x.Roles)
                .NotEmpty().WithMessage("Role name cannot be empty.")
                .NoScriptTags();
        });
    }
}
