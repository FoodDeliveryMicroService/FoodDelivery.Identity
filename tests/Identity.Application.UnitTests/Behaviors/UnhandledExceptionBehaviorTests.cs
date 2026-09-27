using Identity.Application.Common.Behaviors;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Identity.Application.UnitTests.Behaviors;

public class UnhandledExceptionBehaviorTests
{
    private readonly ILogger<DummyRequest> _logger = Substitute.For<ILogger<DummyRequest>>();
    private readonly UnhandledExceptionBehaviour<DummyRequest, string> _sut;

    public UnhandledExceptionBehaviorTests()
    {
        _sut = new UnhandledExceptionBehaviour<DummyRequest, string>(_logger);
    }

    [Fact]
    public async Task Handle_WhenNoException_ShouldReturnResultUnchanged()
    {
        var request = new DummyRequest();

        var result = await _sut.Handle(request, _ => Task.FromResult("OK"), CancellationToken.None);

        Assert.Equal("OK", result);
    }

    [Fact]
    public async Task Handle_WhenNextThrows_ShouldLogAndRethrow()
    {
        var request = new DummyRequest();
        var exception = new InvalidOperationException("boom");

        var thrown = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.Handle(request, _ => throw exception, CancellationToken.None));

        Assert.Same(exception, thrown);

        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Unhandled Exception")),
            exception,
            Arg.Any<Func<object, Exception?, string>>());
    }

    public class DummyRequest;
}