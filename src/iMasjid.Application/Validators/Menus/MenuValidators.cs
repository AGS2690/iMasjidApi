using FluentValidation;
using iMasjid.Application.Common.Extensions;
using iMasjid.Application.DTOs.Requests.Menus;

namespace iMasjid.Application.Validators.Menus;

public class CreateMenuRequestValidator : AbstractValidator<CreateMenuRequest>
{
    public CreateMenuRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Menu title is required.")
            .MaximumLength(100).WithMessage("Menu title cannot exceed 100 characters.")
            .NoScriptTags();

        RuleFor(x => x.Route)
            .MaximumLength(256).WithMessage("Route cannot exceed 256 characters.")
            .NoScriptTags();

        RuleFor(x => x.Icon)
            .MaximumLength(100).WithMessage("Icon cannot exceed 100 characters.")
            .NoScriptTags();

        RuleFor(x => x.OrderIndex)
            .GreaterThanOrEqualTo(0).WithMessage("Order index must be 0 or greater.");
    }
}

public class UpdateMenuRequestValidator : AbstractValidator<UpdateMenuRequest>
{
    public UpdateMenuRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Menu title is required.")
            .MaximumLength(100).WithMessage("Menu title cannot exceed 100 characters.")
            .NoScriptTags();

        RuleFor(x => x.Route)
            .MaximumLength(256).WithMessage("Route cannot exceed 256 characters.")
            .NoScriptTags();

        RuleFor(x => x.Icon)
            .MaximumLength(100).WithMessage("Icon cannot exceed 100 characters.")
            .NoScriptTags();

        RuleFor(x => x.OrderIndex)
            .GreaterThanOrEqualTo(0).WithMessage("Order index must be 0 or greater.");
    }
}

public class AssignMenuActionsRequestValidator : AbstractValidator<AssignMenuActionsRequest>
{
    public AssignMenuActionsRequestValidator()
    {
        RuleFor(x => x.ActionIds)
            .NotNull().WithMessage("ActionIds list cannot be null.");
    }
}

public class AssignRoleMenusRequestValidator : AbstractValidator<AssignRoleMenusRequest>
{
    public AssignRoleMenusRequestValidator()
    {
        RuleFor(x => x.MenuAssignments)
            .NotNull().WithMessage("MenuAssignments list cannot be null.");
    }
}
