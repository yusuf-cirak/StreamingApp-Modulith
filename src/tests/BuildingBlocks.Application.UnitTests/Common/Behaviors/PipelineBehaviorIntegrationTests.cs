using BuildingBlocks.Application.Abstractions.Caching;
using BuildingBlocks.Application.Abstractions.Locking;
using BuildingBlocks.Application.Abstractions.Managers;
using BuildingBlocks.Application.Abstractions.Security;
using BuildingBlocks.Application.Common.Behaviors;
using BuildingBlocks.Application.Common.Services;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using BuildingBlocks.Application.UnitTests.Mocks;
using Xunit;
using YC.Monad;

namespace BuildingBlocks.Application.UnitTests.Common.Behaviors;

public class PipelineBehaviorIntegrationTests
{
    private record TestRequest : IRequest<IResult>, ISecuredRequest, ISensitiveRequest, ILockRequest
    {
        public string Name { get; init; } = string.Empty;
        public string Key => "test-key";
        public int Expiration => 30;
        public bool ReleaseImmediately => true;
    }

    // Validator for TestRequest
    private class TestRequestValidator : AbstractValidator<TestRequest>
    {
        public TestRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
        }
    }

    // Handler for TestRequest
    private class TestRequestHandler : IRequestHandler<TestRequest, IResult>
    {
        public Task<IResult> Handle(TestRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult<IResult>(Results.Ok($"Hello, {request.Name}!"));
        }
    }

    private IServiceProvider CreateServiceProvider(bool isAuthenticated = true, bool isBlacklisted = false)
    {
        var services = new ServiceCollection();

        // Register MediatR
        services.AddMediatR(cfg => { cfg.RegisterServicesFromAssemblyContaining<TestRequest>(); });

        // Register validators
        services.AddScoped<IValidator<TestRequest>, TestRequestValidator>();

        // Register behaviors in correct order
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(SensitiveRequestBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LockBehavior<,>));

        // Register handler
        services.AddScoped<IRequestHandler<TestRequest, IResult>, TestRequestHandler>();

        // Setup authentication
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "test-user")
        };
        var identity = new ClaimsIdentity(claims, "test");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = isAuthenticated ? principal : new ClaimsPrincipal(new ClaimsIdentity())
        };

        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = httpContext
        };

        services.AddSingleton<IHttpContextAccessor>(httpContextAccessor);
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Setup blacklist manager
        services.AddScoped<IBlackListManager>(_ => new MockBlackListManager(isBlacklisted));

        // Setup cache service
        services.AddScoped<ICacheService>(_ => new MockCacheService(true));

        return services.BuildServiceProvider();
    }


    [Fact]
    public async Task Pipeline_WithValidAuthenticatedRequest_Succeeds()
    {
        // Arrange
        var provider = CreateServiceProvider(isAuthenticated: true, isBlacklisted: false);
        var mediator = provider.GetRequiredService<IMediator>();
        var request = new TestRequest { Name = "Test User" };

        // Act
        var result = await mediator.Send(request);

        // Assert
        Assert.IsType<Ok<string>>(result);
        var okResult = (Ok<string>)result;
        Assert.Equal("Hello, Test User!", okResult.Value);
    }

    [Fact]
    public async Task Pipeline_WithValidationFailure_ReturnsBadRequest()
    {
        // Arrange
        var provider = CreateServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();
        var request = new TestRequest { Name = string.Empty };

        // Act
        var result = await mediator.Send(request);

        // Assert
        Assert.IsType<BadRequest<string>>(result);
        var badRequestResult = (BadRequest<string>)result;
        Assert.Contains("Name is required", badRequestResult.Value?.ToString());
    }

    [Fact]
    public async Task Pipeline_WithUnauthenticatedUser_ReturnsUnauthorized()
    {
        // Arrange
        var provider = CreateServiceProvider(isAuthenticated: false);
        var mediator = provider.GetRequiredService<IMediator>();
        var request = new TestRequest { Name = "Test User" };

        // Act
        var result = await mediator.Send(request);

        // Assert
        Assert.IsType<UnauthorizedHttpResult>(result);
    }

    [Fact]
    public async Task Pipeline_WithBlacklistedUser_ReturnsForbid()
    {
        // Arrange
        var provider = CreateServiceProvider(isAuthenticated: true, isBlacklisted: true);
        var mediator = provider.GetRequiredService<IMediator>();
        var request = new TestRequest { Name = "Test User" };

        // Act
        var result = await mediator.Send(request);

        // Assert
        Assert.IsType<ForbidHttpResult>(result);
    }
}