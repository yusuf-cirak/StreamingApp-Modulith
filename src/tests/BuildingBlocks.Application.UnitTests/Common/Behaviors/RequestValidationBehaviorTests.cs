using BuildingBlocks.Application.Common.Behaviors;
using FluentValidation;
using MediatR;
using YC.Monad;

namespace BuildingBlocks.Application.UnitTests.Common.Behaviors;

public class RequestValidationBehaviorTests
{
    private class TestRequest : IRequest<Result>
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
        var behavior = new RequestValidationBehavior<TestRequest, Result>(validators);
        var request = new TestRequest { Name = "Test" };
        var expectedResult = Result.Success();
        RequestHandlerDelegate<Result> next = () => Task.FromResult(expectedResult);

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
        var behavior = new RequestValidationBehavior<TestRequest, Result>(validators);
        var request = new TestRequest { Name = string.Empty };

        // Act
        var result = await behavior.Handle(request, () => Task.FromResult(Result.Success()), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Name is required", result.Error.Message);
    }

    [Fact]
    public async Task Handle_WithNoValidators_CallsNextDelegate()
    {
        // Arrange
        var validators = new List<IValidator<TestRequest>>();
        var behavior = new RequestValidationBehavior<TestRequest, Result>(validators);
        var request = new TestRequest();
        var expectedResult = Result.Success();
        RequestHandlerDelegate<Result> next = () => Task.FromResult(expectedResult);

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
        var behavior = new RequestValidationBehavior<TestRequest, Result>(validators);
        var request = new TestRequest { Name = string.Empty };

        // Act
        var result = await behavior.Handle(request, () => Task.FromResult(Result.Success()), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Name is required", result.Error.Message);
        Assert.Contains("Name must be at least 3 characters", result.Error.Message);
    }

    [Fact]
    public async Task Handle_WithEmptyValidator_CallsNextDelegate()
    {
        // Arrange
        var validators = new List<IValidator<TestRequest>> { new EmptyValidator() };
        var behavior = new RequestValidationBehavior<TestRequest, Result>(validators);
        var request = new TestRequest();
        var expectedResult = Result.Success();
        RequestHandlerDelegate<Result> next = () => Task.FromResult(expectedResult);

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        Assert.Same(expectedResult, result);
    }
} 