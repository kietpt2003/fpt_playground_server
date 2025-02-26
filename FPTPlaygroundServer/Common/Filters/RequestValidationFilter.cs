using FluentValidation;
using FPTPlaygroundServer.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FPTPlaygroundServer.Common.Filters;

public class RequestValidationAttribute<TRequest> : ActionFilterAttribute
{
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Nếu ModelState không hợp lệ, nghĩa là có lỗi khi binding dữ liệu (vd: sai enum)
        if (!context.ModelState.IsValid)
        {
            var errorResponse = new FPTPlaygroundErrorResponse
            {
                Code = FPTPlaygroundErrorCode.FPV_00.Code,
                Title = FPTPlaygroundErrorCode.FPV_00.Title,
                Reasons = context.ModelState.Keys
                    .SelectMany(key => context.ModelState[key]!.Errors.Select(error => new Reason(key.Normalize(), error.ErrorMessage)))
                    .ToList()
            };

            context.Result = new JsonResult(errorResponse)
            {
                StatusCode = (int)FPTPlaygroundErrorCode.FPV_00.Status
            };
            return;
        }

        var validator = context.HttpContext.RequestServices.GetService<IValidator<TRequest>>();

        if (validator == null)
        {
            await next();
            return;
        }

        var request = context.ActionArguments.Values.OfType<TRequest>().FirstOrDefault();

        var validationResult = await validator.ValidateAsync(request!, context.HttpContext.RequestAborted);
        if (!validationResult.IsValid)
        {
            var errorResponse = new FPTPlaygroundErrorResponse
            {
                Code = FPTPlaygroundErrorCode.FPV_00.Code,
                Title = FPTPlaygroundErrorCode.FPV_00.Title,
                Reasons = validationResult.Errors.Select(err => new Reason(err.PropertyName, err.ErrorMessage)).ToList()
            };
            context.Result = new JsonResult(errorResponse)
            {
                StatusCode = (int)FPTPlaygroundErrorCode.FPV_00.Status
            };
            return;
        }

        await next();
    }
}
