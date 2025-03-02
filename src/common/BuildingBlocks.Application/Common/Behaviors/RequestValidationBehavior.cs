﻿using System.Text;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Application.Common.Behaviors;

public sealed class RequestValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IResult
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
            var result = validationErrorMessage;
            return Task.FromResult((TResponse)Results.BadRequest(result));
        }

        return next();
    }
}