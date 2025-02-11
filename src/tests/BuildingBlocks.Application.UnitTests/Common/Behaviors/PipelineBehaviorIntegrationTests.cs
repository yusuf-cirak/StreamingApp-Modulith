using BuildingBlocks.Application.Abstractions.Caching;
using BuildingBlocks.Application.Abstractions.Locking;
using BuildingBlocks.Application.Abstractions.Managers;
using BuildingBlocks.Application.Abstractions.Security;
using BuildingBlocks.Application.Common.Behaviors;
using BuildingBlocks.Application.Common.Services;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using Xunit;
using YC.Monad;

namespace BuildingBlocks.Application.UnitTests.Common.Behaviors;

public class PipelineBehaviorIntegrationTests
{
    // Test request with Result<T>
    private record TestRequest : IRequest<Result<TestResponse>>, ISecuredRequest, ISensitiveRequest, ILockRequest
    {
        public string Name { get; init; } = string.Empty;
        public string Key => "test-key";
        public int Expiration => 30;
        public bool ReleaseImmediately => true;
    }

    private record TestResponse(string Message);

    // Validator for TestRequest
    private class TestRequestValidator : AbstractValidator<TestRequest>
    {
        public TestRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
        }
    }

    // Handler for TestRequest
    private class TestRequestHandler : IRequestHandler<TestRequest, Result<TestResponse>>
    {
        public Task<Result<TestResponse>> Handle(TestRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<TestResponse>.Success(new TestResponse($"Hello, {request.Name}!")));
        }
    }

    private IServiceProvider CreateServiceProvider(bool isAuthenticated = true, bool isBlacklisted = false)
    {
        var services = new ServiceCollection();

        // Register MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<TestRequest>();
        });

        // Register validators
        services.AddScoped<IValidator<TestRequest>, TestRequestValidator>();

        // Register behaviors in correct order
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(SensitiveRequestBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LockBehavior<,>));

        // Register handler
        services.AddScoped<IRequestHandler<TestRequest, Result<TestResponse>>, TestRequestHandler>();

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
        services.AddScoped<IBlackListManager>(_ => new TestBlackListManager(isBlacklisted));

        // Setup cache service
        services.AddScoped<ICacheService>(_ => new TestCacheService(true));

        return services.BuildServiceProvider();
    }

    private class TestBlackListManager : IBlackListManager
    {
        private readonly bool _isBlacklisted;

        public TestBlackListManager(bool isBlacklisted)
        {
            _isBlacklisted = isBlacklisted;
        }

        public Task<Result> AddToBlackListAsync(string key) => Task.FromResult(Result.Success());
        public Task<Result> IsBlackListedAsync(string key) => Task.FromResult(_isBlacklisted ? Result.Success() : Result.Failure());
        public Task<Result> RemoveFromBlackListAsync(string key) => Task.FromResult(Result.Success());
    }

    private class TestCacheService : ICacheService
    {
        private readonly bool _lockAcquired;

        public TestCacheService(bool lockAcquired)
        {
            _lockAcquired = lockAcquired;
        }

        public Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default) 
            => throw new NotImplementedException();

        public Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiresIn = null, CancellationToken cancellationToken = default) 
            => throw new NotImplementedException();

        public Task<T> GetOrUseFactoryAsync<T>(string key, Func<Task<T>> factory, CancellationToken cancellationToken = default) 
            => throw new NotImplementedException();

        public Task<bool> RemoveAsync(string key) 
            => throw new NotImplementedException();

        public Task<bool> ReleaseLockAsync(string key) => Task.FromResult(true);

        public Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiresIn = null, CancellationToken cancellationToken = default) 
            => throw new NotImplementedException();

        public Task<bool> TakeLockAsync(string key, TimeSpan expiration) => Task.FromResult(_lockAcquired);
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
        Assert.True(result.IsSuccess);
        Assert.Equal("Hello, Test User!", result.Value.Message);
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
        Assert.True(result.IsFailure);
        Assert.Contains("Name is required", result.Error.Message);
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
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCache.Unauthorized, result.Error);
    }

    [Fact]
    public async Task Pipeline_WithBlacklistedUser_ReturnsUnauthorized()
    {
        // Arrange
        var provider = CreateServiceProvider(isAuthenticated: true, isBlacklisted: true);
        var mediator = provider.GetRequiredService<IMediator>();
        var request = new TestRequest { Name = "Test User" };

        // Act
        var result = await mediator.Send(request);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCache.Unauthorized, result.Error);
    }
} 