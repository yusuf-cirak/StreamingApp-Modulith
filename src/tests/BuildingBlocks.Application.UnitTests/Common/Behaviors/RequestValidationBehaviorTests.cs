using BuildingBlocks.Application.Common.Behaviors;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace BuildingBlocks.Application.UnitTests.Common.Behaviors;

public class RequestValidationBehaviorTests
{
    private class TestRequest : IRequest<IResult>
    {
        public string Name { get; set; } = string.Empty;
    }

    private class TestRequestValidator : AbstractValidator<TestRequest>
    {
        public TestRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
        }
    }

    private class EmptyValidator : AbstractValidator<TestRequest> { }

    [Fact]
    public async Task Handle_WithValidRequest_CallsNextDelegate()
    {
        // Arrange
        var validators = new List<IValidator<TestRequest>> { new TestRequestValidator() };
        var behavior = new RequestValidationBehavior<TestRequest, IResult>(validators);
        var request = new TestRequest { Name = "Test" };
        var expectedResult = Results.Ok();
        RequestHandlerDelegate<IResult> next = () => Task.FromResult(expectedResult);

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        Assert.Same(expectedResult, result);
    }

    [Fact]
    public async Task Handle_WithInvalidRequest_ReturnsValidationError()
    {
        // Arrange
        var validators = new List<IValidator<TestRequest>> { new TestRequestValidator() };
        var behavior = new RequestValidationBehavior<TestRequest, IResult>(validators);
        var request = new TestRequest { Name = string.Empty };

        // Act
        var result = await behavior.Handle(request, () => Task.FromResult(Results.Ok()), CancellationToken.None);

        // Assert
        Assert.IsType<BadRequest<string>>(result);
        var badRequestResult = (BadRequest<string>)result;
        Assert.Contains("Name is required", badRequestResult.Value?.ToString());
    }

    [Fact]
    public async Task Handle_WithNoValidators_CallsNextDelegate()
    {
        // Arrange
        var validators = new List<IValidator<TestRequest>>();
        var behavior = new RequestValidationBehavior<TestRequest, IResult>(validators);
        var request = new TestRequest();
        var expectedResult = Results.Ok();
        RequestHandlerDelegate<IResult> next = () => Task.FromResult(expectedResult);

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        Assert.Same(expectedResult, result);
    }

    [Fact]
    public async Task Handle_WithMultipleValidators_AggregatesErrors()
    {
        // Arrange
        var validator1 = new TestRequestValidator();
        var validator2 = new InlineValidator<TestRequest>();
        validator2.RuleFor(x => x.Name).Must(x => x.Length >= 3).WithMessage("Name must be at least 3 characters");

        var validators = new List<IValidator<TestRequest>> { validator1, validator2 };
        var behavior = new RequestValidationBehavior<TestRequest, IResult>(validators);
        var request = new TestRequest { Name = string.Empty };

        // Act
        var result = await behavior.Handle(request, () => Task.FromResult(Results.Ok()), CancellationToken.None);

        // Assert
        Assert.IsType<BadRequest<string>>(result);
        var badRequestResult = (BadRequest<string>)result;
        var errorMessage = badRequestResult.Value?.ToString();
        Assert.Contains("Name is required", errorMessage);
        Assert.Contains("Name must be at least 3 characters", errorMessage);
    }

    [Fact]
    public async Task Handle_WithEmptyValidator_CallsNextDelegate()
    {
        // Arrange
        var validators = new List<IValidator<TestRequest>> { new EmptyValidator() };
        var behavior = new RequestValidationBehavior<TestRequest, IResult>(validators);
        var request = new TestRequest();
        var expectedResult = Results.Ok();
        RequestHandlerDelegate<IResult> next = () => Task.FromResult(expectedResult);

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        Assert.Same(expectedResult, result);
    }
} 