using Identity.Application.Common.Behaviors;
using Identity.Application.Common.Interfaces;
using Identity.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Identity.Application.UnitTests.Behaviors;

public class CachingBehaviorTests
{
    private readonly HybridCache _cache = Substitute.For<HybridCache>();
    private readonly ILogger<CachingBehavior<CachedQuery, Result<string>>> _logger =
        Substitute.For<ILogger<CachingBehavior<CachedQuery, Result<string>>>>();

    private readonly CachingBehavior<CachedQuery, Result<string>> _sut;

    public CachingBehaviorTests()
    {
        _sut = new CachingBehavior<CachedQuery, Result<string>>(_cache, _logger);
    }

    [Fact]
    public async Task Handle_WhenNotCachedQuery_ShouldSkipCacheAndReturnResult()
    {
        var uncachedRequest = new NonCachedQuery();
        var behavior = new CachingBehavior<NonCachedQuery, string>(
            _cache, Substitute.For<ILogger<CachingBehavior<NonCachedQuery, string>>>());

        var result = await behavior.Handle(uncachedRequest, _ => Task.FromResult("OK"), CancellationToken.None);

        Assert.Equal("OK", result);
        await _cache.DidNotReceive().SetAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<HybridCacheEntryOptions>(),
            Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCachedQueryAndResultIsSuccess_ShouldCacheResultUnderQueryKeyAndTags()
    {
        var request = new CachedQuery();
        var response = (Result<string>)"test-value";

        string? actualKey = null;
        HybridCacheEntryOptions? actualOptions = null;
        IEnumerable<string>? actualTags = null;

        _cache.SetAsync(
            Arg.Do<string>(k => actualKey = k),
            Arg.Any<Result<string>>(),
            Arg.Do<HybridCacheEntryOptions>(o => actualOptions = o),
            Arg.Do<IEnumerable<string>>(t => actualTags = t),
            Arg.Any<CancellationToken>()).Returns(ValueTask.CompletedTask);

        var result = await _sut.Handle(request, _ => Task.FromResult(response), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(request.CacheKey, actualKey);
        Assert.Equal(request.Expiration, actualOptions!.Expiration);
        Assert.Equal(request.Tags, actualTags);
    }

    [Fact]
    public async Task Handle_WhenCachedQueryAndResultIsError_ShouldNotCacheResult()
    {
        var request = new CachedQuery();
        var errorResult = (Result<string>)Error.Validation("code", "message");

        var result = await _sut.Handle(request, _ => Task.FromResult(errorResult), CancellationToken.None);

        Assert.True(result.IsError);
        await _cache.DidNotReceive().SetAsync(
            Arg.Any<string>(), Arg.Any<Result<string>>(), Arg.Any<HybridCacheEntryOptions>(),
            Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>());
    }

    public class NonCachedQuery;

    public class CachedQuery : ICachedQuery
    {
        public string CacheKey => "lookups:governorates";
        public string[] Tags => ["governorates"];
        public TimeSpan Expiration => TimeSpan.FromHours(24);
        public TimeSpan? LocalCacheExpiration { get; init; } = TimeSpan.FromMinutes(5);
    }
}