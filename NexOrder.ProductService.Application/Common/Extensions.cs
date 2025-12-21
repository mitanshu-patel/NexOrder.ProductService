using FluentValidation.Results;
using NexOrder.ProductService.Shared.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexOrder.ProductService.Application.Common
{
    public static class Extensions
    {
        private static Dictionary<string, string> GetValidationErrors(this List<ValidationFailure> errors)
        {
            return errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => string.Join(", ", g.Select(e => e.ErrorMessage))
                );
        }

        public static CustomResponse<TResult> GetValidationResult<TResult>(this ValidationResult validationResult)
        {
            return CustomHttpResult.BadRequest<TResult>("One or more validation errors", validationResult.Errors.GetValidationErrors());
        }
    }
}
