using System.Text;
using BuildingBlocks.SharedKernel;
using FluentValidation;
using MediatR;

namespace BuildingBlocks.Application.Common.Behaviors;

public sealed class RequestValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        ValidationContext<object> context = new(request);

        StringBuilder sb = new();

        validators.Select(validator => validator.Validate(context))
            .SelectMany(result => result.Errors)
            .Where(failure => failure != null)
            .ToList()
            .ForEach(failure => sb.AppendLine(failure.ErrorMessage));


        string validationErrorMessage = sb.ToString();

        if (validationErrorMessage.Length > 0)
        {
            var result = Result.Failure(validationErrorMessage).ToTypedResult<TResponse>();
            return Task.FromResult(result);
        }

        return next();
    }
}