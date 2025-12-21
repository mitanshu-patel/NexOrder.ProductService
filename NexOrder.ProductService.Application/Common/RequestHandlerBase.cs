using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexOrder.ProductService.Application.Common
{
    public abstract class RequestHandlerBase<TCommand, TResult> : IHandler<TCommand, TResult>
        where TCommand : class
    {
        public async Task<TResult> Handle(TCommand command)
        {
            var validator = GetValidator();
            var validationResult = validator.Validate(command);

            if (!validationResult.IsValid)
            {
                return GetValidationFailedResult(validationResult);
            }

            return await ExecuteCommandAsync(command);
        }

        protected abstract IValidator<TCommand> GetValidator();
        protected abstract TResult GetValidationFailedResult(FluentValidation.Results.ValidationResult validationResult);
        protected abstract Task<TResult> ExecuteCommandAsync(TCommand command);
    }
}
