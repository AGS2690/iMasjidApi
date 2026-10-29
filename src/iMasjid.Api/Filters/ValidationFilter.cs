using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using iMasjid.Api.Middleware;
using iMasjid.Application.Common.Models;

namespace iMasjid.Api.Filters;

public class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var serviceProvider = context.HttpContext.RequestServices;

        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument == null) continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (serviceProvider.GetService(validatorType) is IValidator validator)
            {
                var validationContext = new ValidationContext<object>(argument);
                var validationResult = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);

                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors
                        .Select(e => new ErrorDetail(e.PropertyName, e.ErrorMessage))
                        .ToList();

                    var correlationId = context.HttpContext.Items.TryGetValue(CorrelationIdMiddleware.CorrelationIdItemKey, out var id)
                        ? id?.ToString()
                        : context.HttpContext.TraceIdentifier;

                    var response = ApiResponse.FailureResult("Validation failed.", errors, correlationId);
                    context.Result = new BadRequestObjectResult(response);
                    return;
                }
            }
        }

        await next();
    }
}
